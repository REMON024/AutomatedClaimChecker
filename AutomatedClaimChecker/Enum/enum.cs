namespace AutomatedClaimChecker.Enum
{
    public enum DocumentType
    {
        DeathCertificate=2,
        AgeOfProof=3,
        ClaimApplication=4
    }



    public enum PolicyStatus
    {
        NonClaimable = 1,
        Claim = 2,
        Matured=3
    }

    public enum ClaimStatus
    {
        submit = 1,
        pending = 2,
        Complete = 3,
        Reject = 4,


    }

    public enum DocumentStatus
    {
        submit = 1,
        verified = 2,
        fail = 3,


    }

    public enum DeathClaimIssure
    {
        CityCorporation = 1,
        Hospital = 2,

    }

    public enum AgeProofIssure
    {
        NID = 1,
        BirthCertificate = 2,

    }
}
