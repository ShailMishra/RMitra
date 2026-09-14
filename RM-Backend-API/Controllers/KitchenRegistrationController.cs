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
    public class KitchenRegistrationController : ControllerBase
    {
        private readonly IKitchenRepository _kitchenRepository;

        public KitchenRegistrationController(IKitchenRepository kitchenRepository)
        {
            _kitchenRepository = kitchenRepository;
        }

        /// <summary>
        /// Registers a new kitchen. Mobile verification is mandatory before registration.
        /// </summary>
        [HttpPost("register")]
        public ActionResult<ApiResponseMessage> Register([FromBody] KitchenRegistrationRequest request)
        {
            var result = _kitchenRepository.RegisterKitchen(request);

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = result.Message,
                Method_Name = nameof(Register),
                Data = result
            });
        }
    }
}
