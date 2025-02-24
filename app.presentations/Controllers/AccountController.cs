using app.interfaces;
using app.shared.Dto.PersonalDetail;
using app.shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.presentations.Controllers
{
    [Route($"api/{PresentationAssemblyReference.Name}/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IServicesManager<IPersonalDetailService> _personalDetail;
        private readonly ILogger<PersonalDetailController> _logger;
        public AccountController(
              IServicesManager<IPersonalDetailService> personalDetail,
              ILogger<PersonalDetailController> logger
            )
        {
            _personalDetail = personalDetail;
            _logger = logger;
        }

        [HttpGet("GetViewPersonalDetailAsync")]
        public async Task<PaginatedOffsetResultDto<ViewPersonalDetailDto>> GetViewPersonalDetailAsync([FromQuery] OffsetQueryDto offsetQuery)
        {
            try
            {
                return await _personalDetail.Service.GetViewPersonalDetailAsync(offsetQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
