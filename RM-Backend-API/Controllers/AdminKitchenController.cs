using RM_Backend_API.ActionFilters;
using RM.DataModel.Admin;
using RM.DataRepository.Kitchen;
using RM.Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static RM.Infrastructure.CommonClass.StatusMessage;

namespace RM_Backend_API.Controllers
{
    [ApiController]
    [AdminAuth]
    [Route("api/rasoi-mitra/v1/admin/kitchens")]
    public class AdminKitchenController : ControllerBase
    {
        private readonly IKitchenRepository _kitchenRepository;

        public AdminKitchenController(IKitchenRepository kitchenRepository)
        {
            _kitchenRepository = kitchenRepository;
        }

        /// <summary>
        /// Fetches kitchens pending admin review.
        /// </summary>
        [HttpGet("pending")]
        public ActionResult<ApiResponseMessage> GetPendingKitchens()
        {
            var kitchens = _kitchenRepository.GetPendingKitchens();

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = kitchens.Count > 0 ? "Pending kitchens retrieved successfully." : "No pending kitchens found.",
                Method_Name = nameof(GetPendingKitchens),
                Data = kitchens
            });
        }

        /// <summary>
        /// Approves a kitchen registration and activates it for orders.
        /// </summary>
        [HttpPost("{kitchenId}/approve")]
        public ActionResult<ApiResponseMessage> ApproveKitchen(string kitchenId)
        {
            var result = _kitchenRepository.ApproveKitchen(kitchenId);

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = result.Message,
                Method_Name = nameof(ApproveKitchen),
                Data = result
            });
        }

        /// <summary>
        /// Rejects a kitchen registration with a reason.
        /// </summary>
        [HttpPost("{kitchenId}/reject")]
        public ActionResult<ApiResponseMessage> RejectKitchen(string kitchenId, [FromBody] RejectKitchenRequest request)
        {
            var result = _kitchenRepository.RejectKitchen(kitchenId, request);

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = result.Message,
                Method_Name = nameof(RejectKitchen),
                Data = result
            });
        }

        /// <summary>
        /// Requests additional documents from the kitchen owner.
        /// </summary>
        [HttpPost("{kitchenId}/request-documents")]
        public ActionResult<ApiResponseMessage> RequestDocuments(string kitchenId, [FromBody] RequestAdditionalDocumentsRequest request)
        {
            var result = _kitchenRepository.RequestAdditionalDocuments(kitchenId, request);

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = result.Message,
                Method_Name = nameof(RequestDocuments),
                Data = result
            });
        }
    }
}
