using Hangfire;
using Hangfire.SqlServer;
using JoSale.Data;
using JoSale.Interfaces;
using JoSale.Interfaces.IServices;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using JoSale.Repositories;
using JoSale.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// This is used to prevent Cycle Errors
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

//SQL Server Service
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("Conn")));

//Hangfire Service for Background Services
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("HangfireConnection")));

builder.Services.AddHangfireServer();

//Serilog Logger Configuration
builder.Host.UseSerilog((hostingContext, loggerInformation) =>
loggerInformation.ReadFrom.Configuration(hostingContext.Configuration));

//Redis Caching Configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer
    .Connect("localhost,abortConnect=false"));

//Google Embedding/AI Service
builder.Services.AddGoogleAIEmbeddingGenerator(
    modelId: "gemini-embedding-2",
    apiKey: builder.Configuration["Gemini:ApiKey"]!,
    apiVersion: GoogleAIVersion.V1
);

//Services Linking

//repositories
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IStoreItem, StoreItemRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

//Store Scrapings
builder.Services.AddScoped<IStoreScraper<OrientalStoreScraper>, OrientalStoreScraper>();
builder.Services.AddScoped<IStoreScraper<CityCenterScraper>, CityCenterScraper>();
builder.Services.AddScoped<IStoreScraper<MidasStore>, MidasStore>();

//Services
builder.Services.AddScoped<ICatergorization, Categorization>();
builder.Services.AddScoped<IEmbedding<EmbeddingService>, EmbeddingService>();
builder.Services.AddScoped<IEmbedding<JinaEmbedding>, JinaEmbedding>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<ISemanticSearch, SemanticSearchService>();
builder.Services.AddScoped<IScrapingJob, ScrapingJobService>();


builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

//Identity Configuration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
}
).AddEntityFrameworkStores<AppDbContext>();

//JWT service
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}
).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
            )
    };
}
    );

// Adding the Token placeholder to Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field. Example: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Hangfire Job Scheduling and Dashboard Middleware
app.UseHangfireDashboard();

app.UseSerilogRequestLogging();

//"0 0 */2 * *" => This expression is a Cron Expression and it is for time
//This expression means every 2 days on 00:00 AM it will run the Scraper
RecurringJob.AddOrUpdate<IScrapingJob>(
    "city-center-scraping",
    job => job.RunCityCenter(),
    "0 0 */2 * *" );

//Here the time expression is shifted 30 minutes from the before scraper, and same for the next ones
RecurringJob.AddOrUpdate<IScrapingJob>(
    "midas-scraping",
    job => job.RunMidas(),
    "30 0 */2 * *" );

RecurringJob.AddOrUpdate<IScrapingJob>(
    "oriental-scraping",
    job => job.RunOriental(),
    "0 1 */2 * *" );

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
