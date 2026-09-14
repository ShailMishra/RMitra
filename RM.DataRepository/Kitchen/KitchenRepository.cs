using Newtonsoft.Json;
using RM.DataModel.Admin;
using RM.DataModel.Kitchen;
using RM.DataRepository.ExcelDb;
using RM.DataRepository.MobileVerification;
using RM.Infrastructure.CommonClass;

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
        private readonly IExcelKitchenStore _excelStore;
        private readonly IOtpRepository _otpRepository;

        private static readonly KitchenStatus[] PendingStatuses =
        {
            KitchenStatus.SUBMITTED,
            KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
            KitchenStatus.UNDER_REVIEW,
            KitchenStatus.RESUBMITTED
        };

        public KitchenRepository(IExcelKitchenStore excelStore, IOtpRepository otpRepository)
        {
            _excelStore = excelStore;
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

            _excelStore.InsertKitchen(new ExcelKitchenRow
            {
                KitchenId = kitchenId,
                KitchenName = request.KitchenName.Trim(),
                OwnerName = request.OwnerName.Trim(),
                MobileNumber = request.MobileNumber.Trim(),
                Email = request.Email.Trim(),
                AddressLine1 = request.Address.Line1.Trim(),
                AddressLine2 = request.Address.Line2?.Trim() ?? string.Empty,
                City = request.Address.City.Trim(),
                State = request.Address.State.Trim(),
                Pincode = request.Address.Pincode.Trim(),
                KitchenType = request.KitchenType.Trim(),
                CuisineTypes = JsonConvert.SerializeObject(request.CuisineTypes),
                OperatingHours = JsonConvert.SerializeObject(request.OperatingHours),
                BankDetails = JsonConvert.SerializeObject(request.BankDetails),
                PanCard = request.PanCard.Trim().ToUpperInvariant(),
                KitchenPhoto = request.KitchenPhoto.Trim(),
                Status = KitchenStatus.SUBMITTED.ToString(),
                VerificationToken = request.VerificationToken.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
                SubmittedAt = now
            });

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
            var statuses = PendingStatuses.Select(s => s.ToString()).ToArray();
            return _excelStore.GetKitchensByStatuses(statuses)
                .Select(kitchen => new PendingKitchenSummary
                {
                    KitchenId = kitchen.KitchenId,
                    KitchenName = kitchen.KitchenName,
                    OwnerName = kitchen.OwnerName,
                    MobileNumber = kitchen.MobileNumber,
                    Email = kitchen.Email,
                    KitchenType = kitchen.KitchenType,
                    Status = kitchen.Status,
                    SubmittedAt = kitchen.SubmittedAt ?? kitchen.CreatedAt
                })
                .ToList();
        }

        public KitchenApprovalResponse ApproveKitchen(string kitchenId)
        {
            var kitchen = GetRequiredKitchen(kitchenId);
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
            var kitchen = GetRequiredKitchen(kitchenId);
            ValidateStatusTransition(kitchen.Status, new[]
            {
                KitchenStatus.SUBMITTED,
                KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
                KitchenStatus.UNDER_REVIEW,
                KitchenStatus.RESUBMITTED,
                KitchenStatus.ADDITIONAL_DOCUMENTS_REQUIRED
            });

            kitchen.Status = KitchenStatus.REJECTED.ToString();
            kitchen.RejectionReason = request.Reason.Trim();
            kitchen.UpdatedAt = DateTime.UtcNow;
            _excelStore.UpdateKitchen(kitchen);
            _excelStore.InsertStatusHistory(kitchenId, kitchen.Status, request.Reason.Trim(), kitchen.UpdatedAt);

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
            var kitchen = GetRequiredKitchen(kitchenId);
            ValidateStatusTransition(kitchen.Status, new[]
            {
                KitchenStatus.SUBMITTED,
                KitchenStatus.DOCUMENT_VERIFICATION_PENDING,
                KitchenStatus.UNDER_REVIEW,
                KitchenStatus.RESUBMITTED
            });

            kitchen.Status = KitchenStatus.ADDITIONAL_DOCUMENTS_REQUIRED.ToString();
            kitchen.AdditionalDocumentsRequired = JsonConvert.SerializeObject(request.DocumentsRequired);
            kitchen.AdminRemarks = request.Remarks?.Trim() ?? string.Empty;
            kitchen.UpdatedAt = DateTime.UtcNow;
            _excelStore.UpdateKitchen(kitchen);
            _excelStore.InsertStatusHistory(kitchenId, kitchen.Status, request.Remarks?.Trim(), kitchen.UpdatedAt);

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
            var kitchen = GetRequiredKitchen(kitchenId);

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

            if (hasPanCard)
                kitchen.PanCard = request.PanCard!.Trim().ToUpperInvariant();
            if (hasKitchenPhoto)
                kitchen.KitchenPhoto = request.KitchenPhoto!.Trim();
            if (hasDocuments)
                kitchen.ResubmittedDocuments = JsonConvert.SerializeObject(request.Documents);

            kitchen.Status = KitchenStatus.RESUBMITTED.ToString();
            kitchen.UpdatedAt = DateTime.UtcNow;
            _excelStore.UpdateKitchen(kitchen);
            _excelStore.InsertStatusHistory(kitchenId, KitchenStatus.RESUBMITTED.ToString(), "Kitchen owner resubmitted requested documents.", kitchen.UpdatedAt);
            UpdateKitchenStatus(kitchenId, KitchenStatus.UNDER_REVIEW, "Resubmitted documents are under admin review.");

            return new ResubmitDocumentsResponse
            {
                Success = true,
                KitchenId = kitchenId,
                Status = KitchenStatus.UNDER_REVIEW.ToString(),
                Message = "Documents resubmitted successfully. Kitchen is now under review."
            };
        }

        private ExcelKitchenRow GetRequiredKitchen(string kitchenId)
        {
            var kitchen = _excelStore.GetKitchen(kitchenId);
            if (kitchen == null)
            {
                throw new KitchenException(StatusMessage.StatusInformation.Kitchen_Not_Found,
                    $"Kitchen with ID '{kitchenId}' was not found.");
            }

            return kitchen;
        }

        private void UpdateKitchenStatus(string kitchenId, KitchenStatus status, string? remarks = null)
        {
            var kitchen = GetRequiredKitchen(kitchenId);
            kitchen.Status = status.ToString();
            kitchen.UpdatedAt = DateTime.UtcNow;
            _excelStore.UpdateKitchen(kitchen);
            _excelStore.InsertStatusHistory(kitchenId, kitchen.Status, remarks, kitchen.UpdatedAt);
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
