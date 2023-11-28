namespace AutomatedClaimChecker.Model
{
    public class DocumentType
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public decimal RequiredDocumentAccuracy { get; set; }
    }
}
