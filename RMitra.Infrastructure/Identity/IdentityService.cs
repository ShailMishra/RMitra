using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using RMitra.Application.Abstractions;
using RMitra.Application.Identity;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.BuildingBlocks.Security;
using RMitra.Domain.Common;
using RMitra.Domain.Identity;
using RMitra.Infrastructure.Options;
using RMitra.Infrastructure.Security;

namespace RMitra.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly ITokenService _tokens;
    private readonly IPasswordHasher _passwords;
    private readonly IPublicIdGenerator _ids;
    private readonly OtpOptions _otp;

    public IdentityService(
        ISqlConnectionFactory connections,
        ITokenService tokens,
        IPasswordHasher passwords,
        IPublicIdGenerator ids,
        IOptions<OtpOptions> otp)
    {
        _connections = connections;
        _tokens = tokens;
        _passwords = passwords;
        _ids = ids;
        _otp = otp.Value;
    }

    public async Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request, CancellationToken cancellationToken = default)
    {
        if (!AuthPurposes.IsValid(request.Purpose))
            throw AppException.Validation("purpose must be CUSTOMER, KITCHEN_OWNER, or RIDER.");

        var mobile = request.MobileNumber.Trim();
        var purpose = request.Purpose.Trim().ToUpperInvariant();
        using var db = _connections.Create();

        var otp = Random.Shared.Next(0, 1_000_000).ToString($"D{_otp.Length}");
        var requestId = "OTP_REQ_" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        var sendResult = await db.QuerySingleAsync<SendOtpDbResult>(
            "uspSendOtp",
            new
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                MobileNumber = mobile,
                Purpose = purpose,
                OtpHash = OtpHasher.Hash(otp, mobile),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_otp.ExpiryMinutes),
                MaxSendPer15Minutes = _otp.MaxSendPer15Minutes
            },
            commandType: CommandType.StoredProcedure);

        if (!sendResult.Inserted)
            throw AppException.RateLimited();

        return new SendOtpResponse
        {
            RequestId = requestId,
            DebugOtp = _otp.ReturnOtpInResponse ? otp : null
        };
    }

    public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        var requestId = request.RequestId.Split(':')[0];
        var mobile = request.MobileNumber.Trim();
        using var db = _connections.Create();

        var otpRow = await db.QuerySingleOrDefaultAsync<OtpRequest>(
            "uspVerifyOtp",
            new { requestId },
            commandType: CommandType.StoredProcedure) ?? throw AppException.NotFound("OTP requestId not found.");

        if (!string.Equals(otpRow.MobileNumber, mobile, StringComparison.Ordinal))
            throw AppException.Unauthorized("OTP does not match this mobile number.");
        if (otpRow.VerifiedAt is not null)
            throw AppException.Unauthorized("OTP is single-use and already consumed.");
        if (otpRow.ExpiresAt < DateTime.UtcNow)
            throw AppException.Unauthorized("OTP has expired.");
        if (otpRow.AttemptCount >= _otp.MaxVerificationAttempts)
            throw AppException.Unauthorized("Too many incorrect OTP attempts.");

        if (!string.Equals(otpRow.OtpHash, OtpHasher.Hash(request.Otp, mobile), StringComparison.Ordinal))
        {
            await db.ExecuteAsync("uspVerifyOtpIncrementAttempt", new { otpRow.Id }, commandType: CommandType.StoredProcedure);
            throw AppException.Unauthorized("Invalid OTP.");
        }

        var token = "mob_ver_" + Guid.NewGuid().ToString("N")[..12];
        await db.ExecuteAsync(
            "uspVerifyOtpConsume",
            new { otpRow.Id, token },
            commandType: CommandType.StoredProcedure);

        var user = await FindUser(db, mobile, otpRow.Purpose);
        if (user is not null)
        {
            return new VerifyOtpResponse
            {
                IsNewUser = false,
                AccessToken = _tokens.CreateAccessToken(user),
                ExpiresAt = _tokens.GetExpiryUtc(),
                User = UserProfileDto.From(user)
            };
        }

        return new VerifyOtpResponse
        {
            IsNewUser = true,
            VerificationToken = token
        };
    }

    public async Task SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        _passwords.ValidateStrength(request.Password);
        using var db = _connections.Create();
        var otp = await RequireFreshToken(db, request.VerificationToken);

        var user = await FindUser(db, otp.MobileNumber, otp.Purpose);
        if (user is null)
        {
            user = await InsertUser(db, otp.MobileNumber, otp.Purpose, $"User {otp.MobileNumber[^4..]}", _passwords.Hash(request.Password), null);
        }
        else
        {
            await db.ExecuteAsync(
                "uspSetPassword",
                new { user.Id, PasswordHash = _passwords.Hash(request.Password) },
                commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<AuthSessionResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var purpose = request.Purpose.Trim().ToUpperInvariant();
        if (!AuthPurposes.IsValid(purpose) && purpose != Roles.Admin)
            throw AppException.Validation("purpose must be CUSTOMER, KITCHEN_OWNER, RIDER, or ADMIN.");

        var mobile = request.MobileNumber.Trim();
        using var db = _connections.Create();

        if (!string.IsNullOrWhiteSpace(request.RequestId) && !string.IsNullOrWhiteSpace(request.Otp))
        {
            var verify = await VerifyOtpAsync(new VerifyOtpRequest
            {
                RequestId = request.RequestId,
                MobileNumber = mobile,
                Otp = request.Otp
            }, cancellationToken);

            if (verify.User is null)
                throw AppException.Unauthorized("Complete registration before login.");

            return new AuthSessionResponse
            {
                AccessToken = verify.AccessToken!,
                ExpiresAt = verify.ExpiresAt!.Value,
                User = verify.User
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
            throw AppException.Validation("Password or OTP login is required.");

        var user = await FindUser(db, mobile, purpose)
                   ?? throw AppException.Unauthorized("Invalid mobile or password.");

        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !_passwords.Verify(request.Password, user.PasswordHash))
            throw AppException.Unauthorized("Invalid mobile or password.");

        return new AuthSessionResponse
        {
            AccessToken = _tokens.CreateAccessToken(user),
            ExpiresAt = _tokens.GetExpiryUtc(),
            User = UserProfileDto.From(user)
        };
    }

    internal async Task<OtpRequest> RequireFreshToken(System.Data.IDbConnection db, string verificationToken)
    {
        var otp = await db.QuerySingleOrDefaultAsync<OtpRequest>(
            "uspRequireFreshToken",
            new { verificationToken },
            commandType: CommandType.StoredProcedure) ?? throw AppException.Unauthorized("Verification token is invalid.");

        if (otp.TokenExpiresAt < DateTime.UtcNow)
            throw AppException.Unauthorized("Verification token has expired.");

        return otp;
    }

    internal static async Task<User?> FindUser(System.Data.IDbConnection db, string mobile, string purpose) =>
        await db.QuerySingleOrDefaultAsync<User>(
            "uspFindUser",
            new { MobileNumber = mobile, Purpose = purpose },
            commandType: CommandType.StoredProcedure);

    internal async Task<User> InsertUser(System.Data.IDbConnection db, string mobile, string role, string name, string? passwordHash, string? email)
    {
        var prefix = role switch
        {
            AuthPurposes.Customer => PublicIdPrefixes.Customer,
            AuthPurposes.Rider => PublicIdPrefixes.Rider,
            _ => "USR"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserCode = await _ids.NextAsync(prefix),
            FullName = name,
            MobileNumber = mobile,
            Email = email,
            Role = role,
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await db.ExecuteAsync("uspInsertUser", user, commandType: CommandType.StoredProcedure);
        return user;
    }

    private sealed class SendOtpDbResult
    {
        public int RecentCount { get; set; }
        public bool Inserted { get; set; }
    }
}
