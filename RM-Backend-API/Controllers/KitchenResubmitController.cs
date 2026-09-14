using RM.DataModel.Kitchen;
using RM.DataRepository.Kitchen;
using RM.Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static RM.Infrastructure.CommonClass.StatusMessage;

namespace RM_Backend_API.Controllers
{
    [ApiController]
    [Route("api/rasoi-mitra/v1/kitchens")]
    public class KitchenResubmitController : ControllerBase
    {
        private readonly IKitchenRepository _kitchenRepository;

        public KitchenResubmitController(IKitchenRepository kitchenRepository)
        {
            _kitchenRepository = kitchenRepository;
        }

        /// <summary>
        /// Resubmits documents when kitchen status is ADDITIONAL_DOCUMENTS_REQUIRED.
        /// Transitions: ADDITIONAL_DOCUMENTS_REQUIRED → RESUBMITTED → UNDER_REVIEW.
        /// </summary>
        [HttpPost("{kitchenId}/resubmit-documents")]
        public ActionResult<ApiResponseMessage> ResubmitDocuments(string kitchenId, [FromBody] ResubmitDocumentsRequest request)
        {
            var result = _kitchenRepository.ResubmitDocuments(kitchenId, request);

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = result.Message,
                Method_Name = nameof(ResubmitDocuments),
                Data = result
            });
        }
    }
}
