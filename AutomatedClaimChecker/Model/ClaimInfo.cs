namespace AutomatedClaimChecker.Model
{
    public class ClaimInfo
    {
        public int Id { get; set; }
        public string PolicyNo { get; set; }
        public int ClaimType { get; set; }
        public int ClaimStatus { get; set; }
        public DateTime ClaimDate { get; set; }
        public DateTime DeathOfDate { get; set; }
        public string CauseOfDeath { get; set; }




    }
}
