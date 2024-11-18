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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ReferralCodeService>();
builder.Services.AddHttpClient<Bot>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); 
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Register Bot service and configure HttpClient with the TelegramBaseUrl from the .env file
builder.Services.AddHttpClient<Bot>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);

    // Get the Telegram base URL from the environment variable
    var telegramBaseUrl = Environment.GetEnvironmentVariable("TELEGRAM_BASE_URL");
    client.BaseAddress = new Uri(telegramBaseUrl);
});
string webhookUrl = "https://faf8-196-188-123-14.ngrok-free.app/api/BotWebhook/";
var bot = app.Services.GetRequiredService<Bot>();
// Ensure the webhook is set up asynchronously
await bot.SetWebhookAsync(webhookUrl);

app.Run();
