namespace AutomatedClaimChecker.Model
{
    public class PolicyInfo
    {
        public int Id { get; set; }
        public string PolicyNo { get; set; }
        public decimal PrimeumAmount { get; set; }
        public decimal MaturedAmount { get; set; }
        public int PolicyStatus { get; set; }
        public int CustomerId { get; set; }



    }
}
