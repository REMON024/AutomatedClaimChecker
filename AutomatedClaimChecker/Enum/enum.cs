namespace AutomatedClaimChecker.Enum
{
    public enum DocumentType
    {
        DeathCertificate=1,
        AgeOfProof=2,

    }



    public enum PolicyStatus
    {
        submit = 1,
        pending = 2,

    }

    public enum ClaimStatus
    {
        submit = 1,
        pending = 2,
        verified = 3,

    }

    public enum DocumentStatus
    {
        submit = 1,
        pending = 2,
        verified = 3,
        fail = 4,


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
