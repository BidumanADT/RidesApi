using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RidesApi.Models;
using System.Collections.Concurrent;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:5005", "http://localhost:5004");

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
app.MapGet("/health", () => Results.Ok("ok"));


// Protected endpoint returning UI-friendly shape
app.MapGet("/api/rides", () =>
{
    var now = DateTime.Now; // local time looks nicer in UI; use Utc if you prefer
    var data = new[]
    {
        new RideListItem(
            Id: "R-1001",
            PickupTime: now.AddHours(-2),
            PickupAddress: "O'Hare International Airport, Chicago, IL",
            DropoffAddress: "The Langham, Chicago, IL",
            Status: "Completed",
            Price: 86.50m
        ),
        new RideListItem(
            Id: "R-1002",
            PickupTime: now.AddDays(-1).AddHours(-3),
            PickupAddress: "Midway International Airport, Chicago, IL",
            DropoffAddress: "Aon Center, Chicago, IL",
            Status: "Completed",
            Price: 64.00m
        )
    };
    return Results.Ok(data);
})
.RequireAuthorization();

// In-memory store (dev only)
var ridesStore = new ConcurrentDictionary<string, RideListItem>();

app.MapPost("/api/rides", (RideCreate req) =>
{
    var id = $"R-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    var item = new RideListItem(
        Id: id,
        PickupTime: req.PickupTime == default ? DateTime.Now : req.PickupTime,
        PickupAddress: req.PickupAddress ?? "Unknown pickup",
        DropoffAddress: req.DropoffAddress ?? "Unknown dropoff",
        Status: "Created",
        Price: 0m
    );
    ridesStore[id] = item;
    return Results.Created($"/api/rides/{id}", item);
})
.RequireAuthorization();

app.MapGet("/api/rides/{id}", (string id) =>
{
    return ridesStore.TryGetValue(id, out var item)
        ? Results.Ok(item)
        : Results.NotFound();
})
.RequireAuthorization();

app.Run();
