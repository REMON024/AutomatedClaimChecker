namespace AutomatedClaimChecker.Model
{
    public class SubmitClaim
    {
        public SubmitClaim()
        {
            lstDocument = new List<documentList>();
        }
        public int Id { get; set; } = 0;
        public string PolicyNo { get; set; }
        public int ClaimType { get; set; }
        public DateTime DeathOfDate { get; set; }
        public string CauseOfDeath { get; set; }
        public int Status { get; set; } = 0;

        public List<documentList> lstDocument { get; set; }

    }

    public class documentList
    {
        public string DocumentPath { get; set; }
        public int DocumentType { get; set; }
        public int Status { get; set; }

        public string? Remarks { get; set; }

    }

}
