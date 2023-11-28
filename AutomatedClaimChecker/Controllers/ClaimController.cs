using AutomatedClaimChecker.Model;
using AutomatedClaimChecker.Model.Vm;
using AutomatedClaimChecker.Service;
using AzureCognitiveService.ImageToText;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace AutomatedClaimChecker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService claimService;
        private IHostEnvironment _hostingEnvironmen;
        public ClaimController(IClaimService claimService, IHostEnvironment hostingEnvironmen)
        {
            this.claimService = claimService;
            _hostingEnvironmen = hostingEnvironmen;

        }

        [HttpPost("SubmitForm")]
        public async Task<IActionResult> SubmitForm(SubmitClaim submitClaim)
        {
            var data =await this.claimService.SaveOrUpdate(submitClaim);
            return Ok(data);
        }

        [HttpPost("UploadDocument")]
        public async Task<IActionResult> SubmitFormDocument(IFormFile file)
        {
            string uploads = Path.Combine(_hostingEnvironmen.ContentRootPath, "uploads");
            if (file.Length > 0)
            {
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);

                }
                string filePath = Path.Combine(uploads, file.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                CognitiveImageToText cognitiveImageToText = new CognitiveImageToText();
                var keyGraph = await cognitiveImageToText.ImageToText(filePath);
                ClaimApplication claimApplication = new ClaimApplication();

                claimApplication.PolicyNo = keyGraph.Where(x => x.key.Contains("Policy Number(s)") && x.key.Contains("Decessed")).Select(x => x.value).FirstOrDefault();
                claimApplication.DateOfDeath = keyGraph.Where(x => x.key.Contains("Date of Death") && x.key.Contains("Decessed")).Select(x => x.value).FirstOrDefault();
                claimApplication.CauseOfDeath = keyGraph.Where(x => x.key.Contains("Cause of Death") && x.key.Contains("Decessed")).Select(x => x.value).FirstOrDefault();
                claimApplication.FirstName = keyGraph.Where(x => x.key.Contains("First Name") && x.key.Contains("Decessed")).Select(x => x.value).FirstOrDefault();
                claimApplication.LastName = keyGraph.Where(x => x.key.Contains("Last Name") && x.key.Contains("Decessed")).Select(x => x.value).FirstOrDefault();

                return Ok(new { FilePath = filePath , keyInfo = claimApplication });

            }
            return NotFound();
        }

        [HttpPost("UploadFormDocument")]
        public async Task<IActionResult> UploadFormDocument(IFormFile file)
        {
            string uploads = Path.Combine(_hostingEnvironmen.ContentRootPath, "uploads");
            if (file.Length > 0)
            {
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);

                }
                string filePath = Path.Combine(uploads, file.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }


                var data = await this.claimService.GetClaimFormData(filePath);
                return Ok(data);

            }


            return NotFound();
        }
        [HttpGet("GetClaimByPolicy")]
        public async Task<IActionResult> GetClaimInfoes(string policyNo)
        {
            return Ok();
        }

    }
}
