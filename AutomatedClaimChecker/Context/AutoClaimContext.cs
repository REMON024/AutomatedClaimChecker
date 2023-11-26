using AutomatedClaimChecker.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace AutomatedClaimChecker.Context
{
   
        public class AutoClaimContext : DbContext
        {
        public AutoClaimContext(DbContextOptions<AutoClaimContext> options)
               : base(options)
        { }


        public DbSet<Customer> Customers { get; set; }
        public DbSet<PolicyInfo> PolicyInfos { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }
        public DbSet<ClaimInfo> ClaimInfos { get; set; }




    }

}
