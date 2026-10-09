using Hangfire;
using JoSale.Data;
using JoSale.Extensions;
using JoSale.Interfaces;
using JoSale.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// This is used to prevent Cycle Errors
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

//Services Linking from the Extensions
builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddHelperServices(builder.Configuration, builder.Host);

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

//Authentication/Authorization Service added from the Extensions
builder.Services.AddAuthorizationService(builder.Configuration);

//Swagger Token Placeholder from the extensions
builder.Services.AddSwaggerTokenPlacer();

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
//RecurringJob.AddOrUpdate<IScrapingJob>(
//    "city-center-scraping",
//    job => job.RunCityCenter(),
//    "0 0 */2 * *" );

////Here the time expression is shifted 30 minutes from the before scraper, and same for the next ones
//RecurringJob.AddOrUpdate<IScrapingJob>(
//    "midas-scraping",
//    job => job.RunMidas(),
//    "30 0 */2 * *" );

//RecurringJob.AddOrUpdate<IScrapingJob>(
//    "oriental-scraping",
//    job => job.RunOriental(),
//    "0 1 */2 * *" );

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
