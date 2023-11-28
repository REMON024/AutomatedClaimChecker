using AutomatedClaimChecker.Context;
using AutomatedClaimChecker.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutomatedClaimChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContextPool<AutoClaimContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("autoclaim")));
            // Add services to the container.


            builder.Services.AddTransient<IClaimService, ClaimService>();
            builder.Services.AddTransient<IPolicyInfoService, PolicyInfoService>();
            
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}