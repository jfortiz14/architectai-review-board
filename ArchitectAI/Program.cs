using ArchitectAI.Commands;
using ArchitectAI.Configuration;
using ArchitectAI.Interfaces;
using ArchitectAI.Services;


namespace ArchitectAI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient();

            builder.Services.Configure<AzureOpenAISettings>(builder.Configuration.GetSection("AzureOpenAI"));
            builder.Services.Configure<FoundrySettings>(builder.Configuration.GetSection("Foundry"));
            builder.Services.AddScoped<SecurityAgentService>();
            builder.Services.AddScoped<ArchitectureReviewOrchestrator>();
            builder.Services.AddScoped<ReliabilityAgentService>();
            builder.Services.AddScoped<AzureOpenAIRestClient>();
            builder.Services.AddScoped<IntegrationAgentService>();
            builder.Services.AddScoped<ComplianceAgentService>();
            builder.Services.AddScoped<ChiefArchitectAgentService>();
            builder.Services.AddScoped<IChiefArchitectAgentService, ChiefArchitectFoundryAgentService>();


            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });


            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowFrontend");

            app.MapControllers();

            app.Run();
        }
    }
}
