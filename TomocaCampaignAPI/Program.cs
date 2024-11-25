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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", builder =>
    {
        builder.WithOrigins("https://referral.tomocacloud.com") // Update with your Next.js URL
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ReferralCodeService>();
builder.Services.AddHttpClient<Bot>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); 
});

builder.Services.AddHttpClient<Bot>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);


    var telegramBaseUrl = Environment.GetEnvironmentVariable("TELEGRAM_BASE_URL") ?? "https://default.telegram.api";

    client.BaseAddress = new Uri(telegramBaseUrl);
});

builder.WebHost.UseUrls("http://localhost:9000/");


var app = builder.Build();

// Automatically apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();  // Apply any pending migrations
}
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowNextJs");

app.MapControllers();



string webhookUrl = "https://faf8-196-188-123-14.ngrok-free.app/api/BotWebhook/";
var bot = app.Services.GetRequiredService<Bot>();
await bot.SetWebhookAsync(webhookUrl);

app.Run();
