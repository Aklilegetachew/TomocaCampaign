using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TomocaCampaignAPI;
using TomocaCampaignAPI.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the .env file
Env.Load();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

// Register AppDbContext with MySQL connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 23))
    )
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtIssuer = Environment.GetEnvironmentVariable("Jwt_Issuer");
        var jwtAudience = Environment.GetEnvironmentVariable("Jwt_Audience");
        var jwtKey = Environment.GetEnvironmentVariable("Jwt_Key");

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            throw new ArgumentNullException("Jwt configuration is incomplete in the .env file.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Configure CORS to allow requests from referral.tomocacloud.com
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReferralApp", builder =>
    {
        builder.WithOrigins("https://referral.tomocacloud.com") // Allow requests from this origin
               .AllowAnyHeader() // Allow all headers
               .AllowAnyMethod() // Allow all HTTP methods (GET, POST, etc.)
               .AllowCredentials(); // Allow cookies if needed
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ReferralCodeService>();
builder.Services.AddHttpClient<Bot>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    var telegramBaseUrl = Environment.GetEnvironmentVariable("TELEGRAM_BASE_URL") ?? "https://default.telegram.api";
    client.BaseAddress = new Uri(telegramBaseUrl);
});

var app = builder.Build();

// Automatically apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Apply any pending migrations
}

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReferralApp"); // Apply the AllowReferralApp CORS policy

app.UseAuthorization();

app.MapControllers();

string webhookUrl = "https://faf8-196-188-123-14.ngrok-free.app/api/BotWebhook/";
var bot = app.Services.GetRequiredService<Bot>();
await bot.SetWebhookAsync(webhookUrl);

app.Run();
