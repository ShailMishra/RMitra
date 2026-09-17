using System.Data;
using Dapper;
using RMitra.Application.Abstractions;
using RMitra.Application.Customer;
using RMitra.Application.Identity;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.Domain.Customer;
using RMitra.Infrastructure.Identity;

namespace RMitra.Infrastructure.Customer;

public class CustomerService : ICustomerService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;
    private readonly IPasswordHasher _passwords;
    private readonly ITokenService _tokens;
    private readonly IdentityService _identity;

    public CustomerService(
        ISqlConnectionFactory connections,
        IPublicIdGenerator ids,
        IPasswordHasher passwords,
        ITokenService tokens,
        IdentityService identity)
    {
        _connections = connections;
        _ids = ids;
        _passwords = passwords;
        _tokens = tokens;
        _identity = identity;
    }

    public async Task<RegisterCustomerResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        _passwords.ValidateStrength(request.Password);
        using var db = _connections.Create();
        var otp = await _identity.RequireFreshToken(db, request.VerificationToken);
        if (!string.Equals(otp.Purpose, "CUSTOMER", StringComparison.OrdinalIgnoreCase))
            throw AppException.Forbidden("OTP purpose must be CUSTOMER.");

        if (await IdentityService.FindUser(db, otp.MobileNumber, "CUSTOMER") is not null)
            throw AppException.Conflict("Customer already exists for this mobile.", "CUSTOMER_ALREADY_EXISTS");

        var user = await _identity.InsertUser(db, otp.MobileNumber, "CUSTOMER", request.Name.Trim(), _passwords.Hash(request.Password), null);
        return new RegisterCustomerResponse
        {
            CustomerId = user.UserCode,
            AccessToken = _tokens.CreateAccessToken(user),
            ExpiresAt = _tokens.GetExpiryUtc(),
            User = UserProfileDto.From(user)
        };
    }

    public async Task<UserProfileDto> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var user = await db.QuerySingleOrDefaultAsync<Domain.Identity.User>(
            "uspGetMe",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure)
                   ?? throw AppException.NotFound("Customer not found.");
        return UserProfileDto.From(user);
    }

    public async Task<UserProfileDto> PatchMeAsync(Guid userId, PatchCustomerRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        await db.ExecuteAsync(
            "uspUpdateMe",
            new { UserId = userId, request.Name, request.Email },
            commandType: CommandType.StoredProcedure);
        return await GetMeAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<AddressDto>> GetAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rows = await db.QueryAsync<CustomerAddress>(
            "uspGetAddresses",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
        return rows.Select(Map).ToList();
    }

    public async Task<AddressDto> AddAddressAsync(Guid userId, UpsertAddressRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Latitude == 0 && request.Longitude == 0)
            throw AppException.Validation("latitude and longitude are required.");

        using var db = _connections.Create();
        var address = new CustomerAddress
        {
            Id = Guid.NewGuid(),
            AddressId = await _ids.NextAsync("ADR"),
            UserId = userId,
            Label = request.Label.Trim(),
            Line1 = request.Line1.Trim(),
            Line2 = request.Line2,
            City = request.City.Trim(),
            State = request.State.Trim(),
            Pincode = request.Pincode.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };
        await db.ExecuteAsync("uspAddAddress", address, commandType: CommandType.StoredProcedure);
        return Map(address);
    }

    private static AddressDto Map(CustomerAddress address) => new()
    {
        AddressId = address.AddressId,
        Label = address.Label,
        Line1 = address.Line1,
        Line2 = address.Line2,
        City = address.City,
        State = address.State,
        Pincode = address.Pincode,
        Latitude = address.Latitude,
        Longitude = address.Longitude,
        IsDefault = address.IsDefault
    };
}
