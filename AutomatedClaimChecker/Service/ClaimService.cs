using AutomatedClaimChecker.Context;
using AutomatedClaimChecker.Enum;
using AutomatedClaimChecker.Model;
using AutomatedClaimChecker.Model.Vm;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var policy = await this.context.PolicyInfos.AsNoTracking().Where(x => x.PolicyNo == policyNo).FirstOrDefaultAsync();
            var claim = await this.context.ClaimInfos.AsNoTracking().Where(x => x.PolicyNo == policyNo).FirstOrDefaultAsync();
            var document = await this.context.ClaimDocuments.AsNoTracking().Where(x => x.ClaimInfoId == claim.Id).Select(c => new documentList()
            {
                DocumentPath = c.DocumentPath,
                DocumentType = c.DocumentType,
                Status = c.DocumentStatus,
                Remarks = c.Remarks
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
            var claim = this.context.ClaimInfos.AsNoTracking().Where(x => x.PolicyNo == submitClaim.PolicyNo).FirstOrDefault();
            if (claim is null)
            {
                claim = new ClaimInfo()
                {
                    CauseOfDeath = submitClaim.CauseOfDeath,
                    ClaimDate = DateTime.Now,
                    ClaimStatus = (int)ClaimStatus.submit,
                    ClaimType = 1,
                    DeathOfDate = submitClaim.DeathOfDate,
                    PolicyNo = submitClaim.PolicyNo
                };

                this.context.ClaimInfos.Add(claim);
                this.context.SaveChanges();
            }

            else
            {
                claim.CauseOfDeath = submitClaim.CauseOfDeath;
                claim.DeathOfDate = submitClaim.DeathOfDate;
                claim.ClaimStatus = (int)ClaimStatus.submit;
                this.context.ClaimInfos.Update(claim);
                this.context.SaveChanges();

            }

            var documet = this.context.ClaimDocuments.Where(c => c.ClaimInfoId == claim.Id).ToList();

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
                    documet.Add(res);
                }
                this.context.ClaimDocuments.AddRange(documet);

                this.context.SaveChanges();



                /// Document auto verification 

                claim.ClaimStatus = (int)ClaimStatus.pending;
                foreach (var item in documet)
                {
                    item.DocumentStatus = (int)ClaimStatus.pending;
                    //var response = await CheckBasicValidation(item.DocumentType, item.DocumentPath, claim);
                    var response = (false, "aaaa");


                    if (response.Item1)
                    {
                        item.DocumentStatus = (int)ClaimStatus.Complete;

                    }
                    else
                    {
                        item.DocumentStatus = (int)ClaimStatus.Reject;
                        item.Remarks = response.Item2;
                    }
                }


                var countComplete = documet.Where(c => c.DocumentStatus == (int)ClaimStatus.Complete).Count();
                var countReject = documet.Where(c => c.DocumentStatus == (int)ClaimStatus.Reject).Count();

                if (countComplete == documet.Count)
                {
                    claim.ClaimStatus = (int)ClaimStatus.Complete;

                }

                else if (countReject == documet.Count)
                {
                    claim.ClaimStatus = (int)ClaimStatus.Reject;

                }

                else
                {
                    claim.ClaimStatus = (int)ClaimStatus.pending;

                }


                this.context.ClaimInfos.Update(claim);

                this.context.ClaimDocuments.UpdateRange(documet);
                this.context.SaveChanges();
            }

            var info = await GetClaimByPolicyNo(claim.PolicyNo);

            return info;
        }


        private async Task<(bool, string)> CheckBasicValidation(int documentType, string path, ClaimInfo claimInfo)
        {

            if (documentType == (int)Enum.DocumentType.DeathCertificate)
            {
                return await VerifyDeathCertificate(documentType, path, claimInfo);
            }

            else if (documentType == (int)Enum.DocumentType.AgeOfProof)
            {

                return await VerifyNID(documentType, path, claimInfo);
            }


            return (false, "Invalid document");
        }


        private async Task<(bool, string)> VerifyDeathCertificate(int documentType, string path, ClaimInfo claimInfo)
        {
            var data = new DeathCertificateData();
            var policy = await this.context.PolicyInfos.Where(c => c.PolicyNo == claimInfo.PolicyNo).FirstOrDefaultAsync();
            var customer = await this.context.Customers.Where(c => c.Id == policy.CustomerId).FirstOrDefaultAsync();
            var documentTypes = await this.context.DocumentTypes.Where(c => c.Id == documentType).FirstOrDefaultAsync();
            if (claimInfo.DeathOfDate.Date == Convert.ToDateTime(data.DeathOfBirth).Date
                && claimInfo.CauseOfDeath == data.CauseOfDeath && customer.Name == data.Name
                && documentTypes?.RequiredDocumentAccuracy <= data.accuracy)
            {
                return (true, "");

            }

            return (false, string.Format("Invalid {0}", documentTypes.DocumentName));

        }


        private async Task<(bool, string)> VerifyNID(int documentType, string path, ClaimInfo claimInfo)
        {
            var data = new NIDData();
            var policy = await this.context.PolicyInfos.Where(c => c.PolicyNo == claimInfo.PolicyNo).FirstOrDefaultAsync();
            var customer = await this.context.Customers.Where(c => c.Id == policy.CustomerId).FirstOrDefaultAsync();
            var documentTypes = await this.context.DocumentTypes.Where(c => c.Id == documentType).FirstOrDefaultAsync();
            if (customer.DOB?.Date == Convert.ToDateTime(data.Dob).Date && customer.Name == data.Name
                && documentTypes?.RequiredDocumentAccuracy <= data.accuracy)
            {
                return (true, "");
            }

            return (false, string.Format("Invalid {0}", documentTypes.DocumentName));
        }


        public async Task<SubmitClaim> GetClaimInfo(string policyNo)
        {
            var info = await GetClaimByPolicyNo(policyNo);

            return info;
        }


    }

    public interface IClaimService
    {
        Task<SubmitClaim> SaveOrUpdate(SubmitClaim submitClaim);

        Task<SubmitClaim> GetClaimInfo(string policyNo);

        Task<SubmitClaim> GetClaimById(int Id);
        Task<SubmitClaim> GetClaimByPolicyNo(string policyNo);


    }
}
