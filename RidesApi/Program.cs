using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RidesApi.Storage;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:5005", "http://localhost:5004");

// JWT (dev)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-long-jwt-signing-secret-1234";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In-memory store for MVP (thread-safe)
builder.Services.AddSingleton<IRidesStore, InMemoryRidesStore>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// Health
app.MapGet("/healthz", () => Results.Ok("ok"));
app.MapGet("/health", () => Results.Ok("ok"));

// Map controllers (this was missing)
app.MapControllers();

// Seed a couple of sample items so the list isn’t empty on first run
Seed(app.Services.GetRequiredService<IRidesStore>());

app.Run();

static void Seed(IRidesStore store)
{
    if (store.Count > 0) return;
    store.Create(new("R-1001", DateTime.Now.AddHours(-2), "O'Hare FBO, Chicago, IL", "The Langham, Chicago, IL", "Completed", 86.50m));
    store.Create(new("R-1002", DateTime.Now.AddDays(-1).AddHours(-3), "Midway, Chicago, IL", "Aon Center, Chicago, IL", "Completed", 64.00m));
}
