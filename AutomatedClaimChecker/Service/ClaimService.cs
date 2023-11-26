using AutomatedClaimChecker.Model;

namespace AutomatedClaimChecker.Service
{
    public class ClaimService : IClaimService
    {

        public ClaimService()
        {
                
        }
        public Task<SubmitClaim> GetClaimById(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<SubmitClaim> SaveOrUpdate(SubmitClaim submitClaim)
        {
            throw new NotImplementedException();
        }

       
    }

    public interface IClaimService
    {
        Task<SubmitClaim> SaveOrUpdate(SubmitClaim submitClaim);
       


        Task<SubmitClaim> GetClaimById(int Id);

    }
}
