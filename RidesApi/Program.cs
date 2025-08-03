// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1) Use the same signing key setup as AuthServer:
var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("super-long-jwt-signing-secret-1234"));

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new()
      {
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateLifetime = true,
          IssuerSigningKey = key
      };
  });

builder.Services.AddAuthorization();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// 2) A protected GET /api/rides
app.MapGet("/api/rides", [Authorize] () =>
{
    return new[]
    {
      new Ride(DateTime.UtcNow, 12.3),
      new Ride(DateTime.UtcNow.AddDays(-1), 7.8)
    };
});

record Ride(DateTime Date, double Distance);

app.Run();
