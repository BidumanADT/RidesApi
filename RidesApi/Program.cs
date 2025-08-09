using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RidesApi.Models; 

var builder = WebApplication.CreateBuilder(args);

// Read the signing key from config (fallback to your current dev key)
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? "super-long-jwt-signing-secret-1234";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

// AuthZ
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,   // keep false while your AuthServer doesn't set Issuer
            ValidateAudience = false, // same for Audience
            ValidateLifetime = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// (Optional but handy while iterating)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// Simple health check
app.MapGet("/healthz", () => Results.Ok("ok"));

// Protected endpoint
app.MapGet("/api/rides", () =>
{
    var data = new[]
    {
        new Ride(DateTime.UtcNow,         12.3),
        new Ride(DateTime.UtcNow.AddDays(-1), 7.8)
    };
    return Results.Ok(data);
})
.RequireAuthorization();

app.Run();
