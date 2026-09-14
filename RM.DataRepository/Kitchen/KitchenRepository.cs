using Dapper;
using Newtonsoft.Json;
using RM.DataModel.Admin;
using RM.DataModel.Kitchen;
using RM.DataRepository.DBDapper;
using RM.DataRepository.MobileVerification;
using RM.Infrastructure.CommonClass;
using System.Data;

namespace RM.DataRepository.Kitchen
{
    public interface IKitchenRepository
    {
        KitchenRegistrationResponse RegisterKitchen(KitchenRegistrationRequest request);
        List<PendingKitchenSummary> GetPendingKitchens();
        KitchenApprovalResponse ApproveKitchen(string kitchenId);
        KitchenApprovalResponse RejectKitchen(string kitchenId, RejectKitchenRequest request);
        KitchenApprovalResponse RequestAdditionalDocuments(string kitchenId, RequestAdditionalDocumentsRequest request);
        ResubmitDocumentsResponse ResubmitDocuments(string kitchenId, ResubmitDocumentsRequest request);
    }

    public class KitchenRepository : IKitchenRepository
    {
        private readonly DapperContext _context;
        private readonly IOtpRepository _otpRepository;

        private static readonly KitchenStatus[] PendingStatuses =
        {
            KitchenStatus.SUBMITTED,
            KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
            KitchenStatus.UNDER_REVIEW,
            KitchenStatus.RESUBMITTED
        };

        public KitchenRepository(DapperContext context, IOtpRepository otpRepository)
        {
            _context = context;
            _otpRepository = otpRepository;
        }

        public KitchenRegistrationResponse RegisterKitchen(KitchenRegistrationRequest request)
        {
            if (!_otpRepository.IsMobileVerified(request.VerificationToken, request.MobileNumber))
            {
                throw new KitchenException(StatusMessage.StatusInformation.Mobile_Not_Verified,
                    "Mobile verification is mandatory before kitchen registration. Please verify your mobile number first.");
            }

            var kitchenId = $"KIT_{Guid.NewGuid():N}"[..16].ToUpperInvariant();
            var now = DateTime.UtcNow;
            var status = KitchenStatus.SUBMITTED.ToString();

            const string insertSql = @"
                INSERT INTO RM_Kitchens (
                    KitchenId, KitchenName, OwnerName, MobileNumber, Email,
                    AddressLine1, AddressLine2, City, State, Pincode,
                    KitchenType, CuisineTypes, OperatingHours, BankDetails,
                    PanCard, KitchenPhoto, Status, VerificationToken,
                    CreatedAt, UpdatedAt, SubmittedAt
                ) VALUES (
                    @KitchenId, @KitchenName, @OwnerName, @MobileNumber, @Email,
                    @AddressLine1, @AddressLine2, @City, @State, @Pincode,
                    @KitchenType, @CuisineTypes, @OperatingHours, @BankDetails,
                    @PanCard, @KitchenPhoto, @Status, @VerificationToken,
                    @CreatedAt, @UpdatedAt, @SubmittedAt
                )";

            var parameters = new DynamicParameters();
            parameters.Add("@KitchenId", kitchenId);
            parameters.Add("@KitchenName", request.KitchenName.Trim());
            parameters.Add("@OwnerName", request.OwnerName.Trim());
            parameters.Add("@MobileNumber", request.MobileNumber.Trim());
            parameters.Add("@Email", request.Email.Trim());
            parameters.Add("@AddressLine1", request.Address.Line1.Trim());
            parameters.Add("@AddressLine2", request.Address.Line2?.Trim());
            parameters.Add("@City", request.Address.City.Trim());
            parameters.Add("@State", request.Address.State.Trim());
            parameters.Add("@Pincode", request.Address.Pincode.Trim());
            parameters.Add("@KitchenType", request.KitchenType.Trim());
            parameters.Add("@CuisineTypes", JsonConvert.SerializeObject(request.CuisineTypes));
            parameters.Add("@OperatingHours", JsonConvert.SerializeObject(request.OperatingHours));
            parameters.Add("@BankDetails", JsonConvert.SerializeObject(request.BankDetails));
            parameters.Add("@PanCard", request.PanCard.Trim().ToUpperInvariant());
            parameters.Add("@KitchenPhoto", request.KitchenPhoto.Trim());
            parameters.Add("@Status", status);
            parameters.Add("@VerificationToken", request.VerificationToken.Trim());
            parameters.Add("@CreatedAt", now);
            parameters.Add("@UpdatedAt", now);
            parameters.Add("@SubmittedAt", now);

            _context.Insert<int>(insertSql, parameters, CommandType.Text);

            UpdateKitchenStatus(kitchenId, KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
                "Registration submitted. Document verification pending.");

            return new KitchenRegistrationResponse
            {
                Success = true,
                KitchenId = kitchenId,
                Status = KitchenStatus.DOCUMENT_VERIFICATION_PENDING.ToString(),
                Message = "Kitchen registration submitted successfully. Awaiting document verification."
            };
        }

        public List<PendingKitchenSummary> GetPendingKitchens()
        {
            var statusList = string.Join(",", PendingStatuses.Select(s => $"'{s}'"));

            var sql = $@"
                SELECT
                    KitchenId AS KitchenId,
                    KitchenName AS KitchenName,
                    OwnerName AS OwnerName,
                    MobileNumber AS MobileNumber,
                    Email AS Email,
                    KitchenType AS KitchenType,
                    Status AS Status,
                    SubmittedAt AS SubmittedAt
                FROM RM_Kitchens
                WHERE Status IN ({statusList})
                ORDER BY SubmittedAt ASC";

            return _context.GetAll<PendingKitchenSummary>(sql, new DynamicParameters(), CommandType.Text);
        }

        public KitchenApprovalResponse ApproveKitchen(string kitchenId)
        {
            var kitchen = GetKitchenById(kitchenId);
            ValidateStatusTransition(kitchen.Status, new[]
            {
                KitchenStatus.SUBMITTED,
                KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
                KitchenStatus.UNDER_REVIEW,
                KitchenStatus.RESUBMITTED
            });

            UpdateKitchenStatus(kitchenId, KitchenStatus.APPROVED, "Kitchen approved by admin.");
            UpdateKitchenStatus(kitchenId, KitchenStatus.ACTIVE, "Kitchen is now active and can receive orders.");

            return new KitchenApprovalResponse
            {
                Success = true,
                KitchenId = kitchenId,
                Status = KitchenStatus.ACTIVE.ToString(),
                Message = "Kitchen approved and activated successfully."
            };
        }

        public KitchenApprovalResponse RejectKitchen(string kitchenId, RejectKitchenRequest request)
        {
            var kitchen = GetKitchenById(kitchenId);
            ValidateStatusTransition(kitchen.Status, new[]
            {
                KitchenStatus.SUBMITTED,
                KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
                KitchenStatus.UNDER_REVIEW,
                KitchenStatus.RESUBMITTED,
                KitchenStatus.ADDITIONAL_DOCUMENTS_REQUIRED
            });

            const string sql = @"
                UPDATE RM_Kitchens
                SET Status = @Status,
                    RejectionReason = @RejectionReason,
                    UpdatedAt = @UpdatedAt
                WHERE KitchenId = @KitchenId";

            var parameters = new DynamicParameters();
            parameters.Add("@KitchenId", kitchenId);
            parameters.Add("@Status", KitchenStatus.REJECTED.ToString());
            parameters.Add("@RejectionReason", request.Reason.Trim());
            parameters.Add("@UpdatedAt", DateTime.UtcNow);

            _context.Update<int>(sql, parameters, CommandType.Text);

            return new KitchenApprovalResponse
            {
                Success = true,
                KitchenId = kitchenId,
                Status = KitchenStatus.REJECTED.ToString(),
                Message = "Kitchen registration rejected."
            };
        }

        public KitchenApprovalResponse RequestAdditionalDocuments(string kitchenId, RequestAdditionalDocumentsRequest request)
        {
            var kitchen = GetKitchenById(kitchenId);
            ValidateStatusTransition(kitchen.Status, new[]
            {
                KitchenStatus.SUBMITTED,
                KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
                KitchenStatus.UNDER_REVIEW,
                KitchenStatus.RESUBMITTED
            });

            const string sql = @"
                UPDATE RM_Kitchens
                SET Status = @Status,
                    AdditionalDocumentsRequired = @AdditionalDocumentsRequired,
                    AdminRemarks = @AdminRemarks,
                    UpdatedAt = @UpdatedAt
                WHERE KitchenId = @KitchenId";

            var parameters = new DynamicParameters();
            parameters.Add("@KitchenId", kitchenId);
            parameters.Add("@Status", KitchenStatus.ADDITIONAL_DOCUMENTS_REQUIRED.ToString());
            parameters.Add("@AdditionalDocumentsRequired", JsonConvert.SerializeObject(request.DocumentsRequired));
            parameters.Add("@AdminRemarks", request.Remarks?.Trim());
            parameters.Add("@UpdatedAt", DateTime.UtcNow);

            _context.Update<int>(sql, parameters, CommandType.Text);

            return new KitchenApprovalResponse
            {
                Success = true,
                KitchenId = kitchenId,
                Status = KitchenStatus.ADDITIONAL_DOCUMENTS_REQUIRED.ToString(),
                Message = "Additional documents requested from kitchen owner."
            };
        }

        public ResubmitDocumentsResponse ResubmitDocuments(string kitchenId, ResubmitDocumentsRequest request)
        {
            var kitchen = GetKitchenWithMobile(kitchenId);

            if (!Enum.TryParse<KitchenStatus>(kitchen.Status, out var currentStatus) ||
                currentStatus != KitchenStatus.ADDITIONAL_DOCUMENTS_REQUIRED)
            {
                throw new KitchenException(StatusMessage.StatusInformation.Invalid_Kitchen_Status_Transition,
                    $"Documents can only be resubmitted when kitchen status is {KitchenStatus.ADDITIONAL_DOCUMENTS_REQUIRED}. Current status: '{kitchen.Status}'.");
            }

            if (!string.Equals(kitchen.MobileNumber, request.MobileNumber.Trim(), StringComparison.Ordinal))
            {
                throw new KitchenException(StatusMessage.StatusInformation.Mobile_Not_Verified,
                    "Mobile number does not match the kitchen registration record.");
            }

            var hasPanCard = !string.IsNullOrWhiteSpace(request.PanCard);
            var hasKitchenPhoto = !string.IsNullOrWhiteSpace(request.KitchenPhoto);
            var hasDocuments = request.Documents != null && request.Documents.Count > 0;

            if (!hasPanCard && !hasKitchenPhoto && !hasDocuments)
            {
                throw new KitchenException(StatusMessage.StatusInformation.Manadatory_Feild_Required,
                    "At least one document (panCard, kitchenPhoto, or documents) must be provided for resubmission.");
            }

            const string sql = @"
                UPDATE RM_Kitchens
                SET PanCard = COALESCE(@PanCard, PanCard),
                    KitchenPhoto = COALESCE(@KitchenPhoto, KitchenPhoto),
                    ResubmittedDocuments = @ResubmittedDocuments,
                    Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE KitchenId = @KitchenId";

            var parameters = new DynamicParameters();
            parameters.Add("@KitchenId", kitchenId);
            parameters.Add("@PanCard", hasPanCard ? request.PanCard!.Trim().ToUpperInvariant() : null);
            parameters.Add("@KitchenPhoto", hasKitchenPhoto ? request.KitchenPhoto!.Trim() : null);
            parameters.Add("@ResubmittedDocuments", hasDocuments ? JsonConvert.SerializeObject(request.Documents) : null);
            parameters.Add("@Status", KitchenStatus.RESUBMITTED.ToString());
            parameters.Add("@UpdatedAt", DateTime.UtcNow);

            _context.Update<int>(sql, parameters, CommandType.Text);

            RecordStatusHistory(kitchenId, KitchenStatus.RESUBMITTED, "Kitchen owner resubmitted requested documents.");
            UpdateKitchenStatus(kitchenId, KitchenStatus.UNDER_REVIEW, "Resubmitted documents are under admin review.");

            return new ResubmitDocumentsResponse
            {
                Success = true,
                KitchenId = kitchenId,
                Status = KitchenStatus.UNDER_REVIEW.ToString(),
                Message = "Documents resubmitted successfully. Kitchen is now under review."
            };
        }

        private KitchenRecord GetKitchenById(string kitchenId)
        {
            const string sql = @"
                SELECT KitchenId, Status
                FROM RM_Kitchens
                WHERE KitchenId = @KitchenId";

            var parameters = new DynamicParameters();
            parameters.Add("@KitchenId", kitchenId);

            var kitchen = _context.Get<KitchenRecord>(sql, parameters, CommandType.Text);
            if (kitchen == null)
            {
                throw new KitchenException(StatusMessage.StatusInformation.Kitchen_Not_Found,
                    $"Kitchen with ID '{kitchenId}' was not found.");
            }

            return kitchen;
        }

        private KitchenRecord GetKitchenWithMobile(string kitchenId)
        {
            const string sql = @"
                SELECT KitchenId, Status, MobileNumber
                FROM RM_Kitchens
                WHERE KitchenId = @KitchenId";

            var parameters = new DynamicParameters();
            parameters.Add("@KitchenId", kitchenId);

            var kitchen = _context.Get<KitchenRecord>(sql, parameters, CommandType.Text);
            if (kitchen == null)
            {
                throw new KitchenException(StatusMessage.StatusInformation.Kitchen_Not_Found,
                    $"Kitchen with ID '{kitchenId}' was not found.");
            }

            return kitchen;
        }

        private void UpdateKitchenStatus(string kitchenId, KitchenStatus status, string? remarks = null)
        {
            const string sql = @"
                UPDATE RM_Kitchens
                SET Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE KitchenId = @KitchenId";

            var parameters = new DynamicParameters();
            parameters.Add("@KitchenId", kitchenId);
            parameters.Add("@Status", status.ToString());
            parameters.Add("@UpdatedAt", DateTime.UtcNow);

            _context.Update<int>(sql, parameters, CommandType.Text);

            RecordStatusHistory(kitchenId, status, remarks);
        }

        private void RecordStatusHistory(string kitchenId, KitchenStatus status, string? remarks = null)
        {
            const string historySql = @"
                INSERT INTO RM_KitchenStatusHistory (KitchenId, Status, Remarks, CreatedAt)
                VALUES (@KitchenId, @Status, @Remarks, @CreatedAt)";

            var historyParams = new DynamicParameters();
            historyParams.Add("@KitchenId", kitchenId);
            historyParams.Add("@Status", status.ToString());
            historyParams.Add("@Remarks", remarks);
            historyParams.Add("@CreatedAt", DateTime.UtcNow);

            _context.Insert<int>(historySql, historyParams, CommandType.Text);
        }

        private static void ValidateStatusTransition(string currentStatus, KitchenStatus[] allowedStatuses)
        {
            if (!Enum.TryParse<KitchenStatus>(currentStatus, out var status) ||
                !allowedStatuses.Contains(status))
            {
                throw new KitchenException(StatusMessage.StatusInformation.Invalid_Kitchen_Status_Transition,
                    $"Kitchen cannot be updated from status '{currentStatus}'.");
            }
        }

        private sealed class KitchenRecord
        {
            public string KitchenId { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string MobileNumber { get; set; } = string.Empty;
        }
    }

    public class KitchenException : Exception
    {
        public StatusMessage.StatusInformation StatusCode { get; }

        public KitchenException(StatusMessage.StatusInformation statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
