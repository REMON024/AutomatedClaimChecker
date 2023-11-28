using AutomatedClaimChecker.Context;
using AutomatedClaimChecker.Enum;
using AutomatedClaimChecker.Model;
using AutomatedClaimChecker.Model.Vm;
using AzureCognitiveService.DocumentSimilarity;
using AzureCognitiveService.ImageToText;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutomatedClaimChecker.Service
{
    public class ClaimService : IClaimService
    {
        public AutoClaimContext context;
        private IHostEnvironment _hostingEnvironment;
        public ClaimService(AutoClaimContext context, IHostEnvironment hostingEnvironment)
        {
            this.context = context;
            _hostingEnvironment = hostingEnvironment;
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
                    var response = await CheckBasicValidation(item.DocumentType, item.DocumentPath, claim);
                   // var response = (false, "aaaa");


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
                return await VerifyDeathCertificate(documentType, path, claimInfo.PolicyNo);
            }

            else if (documentType == (int)Enum.DocumentType.AgeOfProof)
            {

                return await VerifyNID(documentType, path, claimInfo.PolicyNo);
            }


            return (false, "Invalid document");
        }

        private string RawStringToDateString(string text)
        {
            string[] a = text.Split(" ");
            string date = a[0] + a[1];
            string month = a[2] + a[3];
            string year = a[4] + a[5] + a[5] + a[6];
            return date + " " + month + " " + year;
        }
        private async Task<(bool, string)> VerifyDeathCertificate(int documentType, string path, string policyNo)
        {
            var data = new DeathCertificateData();

            CognitiveImageToText cognitiveImageToText = new CognitiveImageToText();
           

            CognitiveSimilarity sim = new CognitiveSimilarity();
            string root = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot");
            var result = sim.SubmitDC("", "", root + "/DC Vector.xlsx", outputImage: path);
            data.accuracy = result.ratio;
            var claimInfo = await this.context.ClaimInfos.Where(c => c.PolicyNo == policyNo).FirstOrDefaultAsync();
            var policy = await this.context.PolicyInfos.Where(c => c.PolicyNo == claimInfo.PolicyNo).FirstOrDefaultAsync();
            var customer = await this.context.Customers.Where(c => c.Id == policy.CustomerId).FirstOrDefaultAsync();
            var documentTypes = await this.context.DocumentTypes.Where(c => c.Id == documentType).FirstOrDefaultAsync();

            if(documentTypes?.RequiredDocumentAccuracy > data.accuracy)
            {
                return (false, result.remakrs);
            }

            var text = await cognitiveImageToText.GetRawTextFromImg(path);

            if (text.Contains(claimInfo.DeathOfDate.Date.ToString("dd MM yyyy")) 
                && text.Contains(claimInfo.CauseOfDeath)  && text.Contains(customer.Name))
            {
                return (true, "");

            }

            return (false, string.Format("Invalid {0}", documentTypes?.DocumentName));

        }


        private async Task<(bool, string)> VerifyNID(int documentType, string path, string policyNo)
        {
            var data = new NIDData();

            CognitiveImageToText cognitiveImageToText = new CognitiveImageToText();
            var text = await cognitiveImageToText.GetRawTextFromImg(path);

            CognitiveSimilarity sim = new CognitiveSimilarity();
            string root = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot");
            var result = sim.SubmitNid("", "", root + "/NID Vector.xlsx", outputImage: path);
            data.accuracy = result.ratio;

            var claimInfo = await this.context.ClaimInfos.Where(c => c.PolicyNo == policyNo).FirstOrDefaultAsync();
            var policy = await this.context.PolicyInfos.Where(c => c.PolicyNo == claimInfo.PolicyNo).FirstOrDefaultAsync();
            var customer = await this.context.Customers.Where(c => c.Id == policy.CustomerId).FirstOrDefaultAsync();
            var documentTypes = await this.context.DocumentTypes.Where(c => c.Id == documentType).FirstOrDefaultAsync();

            if(documentTypes?.RequiredDocumentAccuracy > data.accuracy)
            {
                return (false, result.remakrs);
            }

            if (text.Contains(customer.DOB?.Date.ToString("dd MMM yyyy")) && text.Contains(customer.Name))
                 
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
