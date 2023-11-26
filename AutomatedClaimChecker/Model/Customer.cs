namespace AutomatedClaimChecker.Model
{
    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? CustomerMobile { get; set; }
        public DateTime? DOB { get; set; }


        public string? BeneficeryName { get; set; }
        public string? BeneficeryMobile { get; set; }

    }
}
