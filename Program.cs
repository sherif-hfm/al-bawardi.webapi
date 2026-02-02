using janaez.webapi;
using janaez.webapi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity;
using System;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);



var allowedCors = builder.Configuration.GetSection("AllowedCors").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCors", builder =>
    {
        builder.WithOrigins(allowedCors)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("Database");

builder.Services.AddSingleton<ITimeZoneService, TimeZoneService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.Configure<BasicAuthSettings>(builder.Configuration.GetSection(BasicAuthSettings.Key));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.Key));


// Add services to the container.
var connectionString = builder.Configuration.GetSection("DatabaseSettings").GetValue<string>("MySqlConnectionStrings");

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6))));

builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6)));
}
); 


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Lockout.AllowedForNewUsers = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<UsersDbContext>()
.AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // good for security
    options.SlidingExpiration = true; // extend cookie expiration on each request
    options.Cookie.SameSite = SameSiteMode.None; //  required for cross-origin
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // Critical for Web API:
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // Return 401 Unauthorized
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403; // Return 403 Forbidden
        return Task.CompletedTask;
    };
});


builder.Services.AddHttpContextAccessor();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //options.JsonSerializerOptions.PropertyNamingPolicy = null; // disable camelCase
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowCors");

app.MapHealthChecks("/_health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            errors = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
