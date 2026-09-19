using HMS_360_PMS.ProjectEntitiesModels.KOT;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectAPI.Controllers.GeneralSettings
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSettingsController : BaseController
    {
        private readonly ILogger<EmailSettingsController> _logger;
        private readonly EmailSender_Service _emailsender;
        public EmailSettingsController(ILogger<EmailSettingsController> logger, EmailSender_Service emailsender)
        {
            _logger = logger;
            _emailsender = emailsender;
        }

        #region EmailNotfication
        [HttpPost("SentEmailNotfication")]
        public async Task<IActionResult> SentEmailNotfication([FromBody] EmailNotficationModel model)
        {
            var result = await _emailsender.SentEmailNotfication(model);

            if (!result)
            {
                return NotFoundResponse("Failed Email Deliverd successfully.");
            }

            return Success(true, "Email Deliverd successfully");
        }
        #endregion
    }
}
