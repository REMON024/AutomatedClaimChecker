using AutomatedClaimChecker.Context;
using AutomatedClaimChecker.Enum;
using AutomatedClaimChecker.Model;
using AutomatedClaimChecker.Model.Vm;
using Microsoft.EntityFrameworkCore;

namespace AutomatedClaimChecker.Service
{
    public class ClaimService : IClaimService
    {
        public AutoClaimContext context;
        public ClaimService(AutoClaimContext context)
        {
            this.context = context;
        }
        public Task<SubmitClaim> GetClaimById(int Id)
        {
            throw new NotImplementedException();
        }

        public async Task<SubmitClaim> GetClaimByPolicyNo(string policyNo)
        {
            var policy = await this.context.PolicyInfos.Where(x => x.PolicyNo == policyNo).FirstOrDefaultAsync();
            var claim = await this.context.ClaimInfos.Where(x => x.PolicyNo == policyNo).FirstOrDefaultAsync();
            var document = await this.context.ClaimDocuments.Where(x => x.ClaimInfoId == claim.Id).Select(c => new documentList()
            {
                DocumentPath = c.DocumentPath,
                DocumentType = c.DocumentType,
                Status = c.DocumentStatus
            }).ToListAsync();


            var data = new SubmitClaim()
            {
                CauseOfDeath = claim.CauseOfDeath,
                ClaimType = claim.ClaimType,
                DeathOfDate = claim.DeathOfDate,
                Id = claim.Id,
                PolicyNo = policyNo,
                Status = claim.ClaimStatus,
                lstDocument = document
            };

            return data;
        }

        public async Task<SubmitClaim> SaveOrUpdate(SubmitClaim submitClaim)
        {
            var claim = await this.context.ClaimInfos.Where(x => x.PolicyNo == submitClaim.PolicyNo).FirstOrDefaultAsync();
            if (claim is null)
            {
                claim = new ClaimInfo()
                {
                    CauseOfDeath = submitClaim.CauseOfDeath,
                    ClaimDate = DateTime.Now,
                    ClaimStatus = (int)ClaimStatus.submit,
                    ClaimType = submitClaim.ClaimType,
                    DeathOfDate = submitClaim.DeathOfDate,
                    PolicyNo = submitClaim.PolicyNo
                };

                await this.context.ClaimInfos.AddAsync(claim);
                await this.context.SaveChangesAsync();
            }

            else
            {
                claim.CauseOfDeath = submitClaim.CauseOfDeath;
                claim.DeathOfDate = submitClaim.DeathOfDate;
                this.context.ClaimInfos.Update(claim);
                this.context.SaveChanges();

            }

            var documet = await this.context.ClaimDocuments.Where(c => c.ClaimInfoId == claim.Id).ToListAsync();

            if (claim.Id > 0)
            {
                if (documet.Any())
                {
                    this.context.ClaimDocuments.RemoveRange(documet);
                    this.context.SaveChanges();
                }
                documet = new List<ClaimDocument>();
                foreach (var item in submitClaim.lstDocument)
                {
                    var res = new ClaimDocument()
                    {
                        ClaimInfoId = claim.Id,
                        DocumentPath = item.DocumentPath,
                        DocumentStatus = (int)ClaimStatus.submit,
                        DocumentType = item.DocumentType
                    };

                }

                await this.context.ClaimDocuments.AddRangeAsync(documet);
                await this.context.SaveChangesAsync();

                foreach (var item in documet)
                {
                    var response = await CheckBasicValidation(item.DocumentType, item.DocumentPath, claim);
                    if (response)
                    {
                    }
                }

            }



            ///Write the document verification logic



            var info = await GetClaimByPolicyNo(claim.PolicyNo);

            return info;
        }


        private async Task<bool> CheckBasicValidation(int documentType, string path, ClaimInfo claimInfo)
        {

            if (documentType == (int)Enum.DocumentType.DeathCertificate)
            {
                return await VerifyDeathCertificate(documentType, path, claimInfo);
            }

            else if (documentType == (int)Enum.DocumentType.AgeOfProof)
            {

            }


            return false;
        }


        private async Task<bool> VerifyDeathCertificate(int documentType, string path, ClaimInfo claimInfo)
        {
            var data = new { Name = "remon", DeathOfBirth = "1996-04-14", CauseOfDeath = "accident", accuracy = 0.0 };
            var policy = await this.context.PolicyInfos.Where(c => c.PolicyNo == claimInfo.PolicyNo).FirstOrDefaultAsync();
            var customer = await this.context.Customers.Where(c => c.Id == policy.CustomerId).FirstOrDefaultAsync();
            var documentTypes = await this.context.DocumentTypes.Where(c => c.Id == documentType).FirstOrDefaultAsync();
            if (claimInfo.DeathOfDate.Date == Convert.ToDateTime(data.DeathOfBirth).Date && claimInfo.CauseOfDeath == data.CauseOfDeath && customer.Name == data.Name)
            {
                return true;
            }

            return false;
        }


        public async Task<ClaimApplication> GetClaimFormData(string path)
        {
            return await ConvertImageToText(path);
        }


        private async Task<ClaimApplication> ConvertImageToText(string path)
        {
            return new ClaimApplication();
        }


    }

    public interface IClaimService
    {
        Task<SubmitClaim> SaveOrUpdate(SubmitClaim submitClaim);

        Task<ClaimApplication> GetClaimFormData(string path);

        Task<SubmitClaim> GetClaimById(int Id);
        Task<SubmitClaim> GetClaimByPolicyNo(string policyNo);


    }
}
