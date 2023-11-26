using AutomatedClaimChecker.Context;
using AutomatedClaimChecker.Model.Vm;
using Microsoft.EntityFrameworkCore;

namespace AutomatedClaimChecker.Service
{
    public class PolicyInfoService : IPolicyInfoService
    {
        public AutoClaimContext context;
        public PolicyInfoService(AutoClaimContext context)
        {
            this.context = context;
        }
        public async Task<PolicyDetailInfoes> GetPolicyInfoes(string policyNo)
        {
            var policyInfoes = await this.context.PolicyInfos.Where(c => c.PolicyNo == policyNo).FirstOrDefaultAsync();
            if (policyInfoes is not null)
            {
                var customer = await this.context.Customers.Where(v => v.Id == policyInfoes.CustomerId).FirstOrDefaultAsync();

                if (customer is not null)
                {
                    var policyDetailInfoes = new PolicyDetailInfoes()
                    {
                        Amount=policyInfoes.MaturedAmount.ToString(),
                        BeneficiaryName=customer.BeneficeryName,
                        CustomerName=customer.Name,
                        Mobile=customer.CustomerMobile,
                        PolicyNo=policyInfoes.PolicyNo,
                        PolicyStatus=policyInfoes.PolicyStatus.ToString(),
                        DOB=customer.DOB.ToString()
                    };
                }
            }

            return null;
        }
    }

    public interface IPolicyInfoService
    {
        Task<PolicyDetailInfoes> GetPolicyInfoes(string policyNo);
    }
}
