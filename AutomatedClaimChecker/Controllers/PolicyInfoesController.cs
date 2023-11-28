using AutomatedClaimChecker.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedClaimChecker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyInfoesController : ControllerBase
    {
        private readonly IPolicyInfoService policyInfoService;
        public PolicyInfoesController(IPolicyInfoService policyInfoService)
        {
            this.policyInfoService = policyInfoService;
        }
        [HttpGet("CheckPolicy")]
        public async Task<IActionResult> CheckPolicy(string policyNo, string dob, string phoneNumber)
        {

            var data = await policyInfoService.GetPolicyInfoes(policyNo);
            if (data is not null)
                return Ok(new { PolicyNo = data.PolicyNo, Dob = data.DOB, PhoneNumber = data.Mobile });


            return NotFound();


        }


        [HttpGet("validateOtp")]
        public async Task<IActionResult> validateOtp(string policyNo, string otp)
        {
            if (string.IsNullOrEmpty(otp) || !otp.Equals("123456"))
            {
                return Unauthorized("Invalid Otp.");
            }
            var data = await policyInfoService.GetPolicyInfoes(policyNo);

            return Ok(data);
        }
    }
}
