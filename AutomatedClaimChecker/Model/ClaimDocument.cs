namespace AutomatedClaimChecker.Model
{
    public class ClaimDocument
    {
        public int Id { get; set; }
        public int ClaimInfoId { get; set; }
        public string DocumentPath { get; set; }
        public int DocumentType { get; set; }
        public int DocumentStatus { get; set; }
        public int? Issure { get; set; }
        public decimal? DocumentAccuracy { get; set; }
        public string? Remarks { get; set; }


    }
}
