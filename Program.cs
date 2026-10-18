using Fynd.Api.Data;
using Microsoft.EntityFrameworkCore;
using Fynd.Api.Services;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.OpenApi.Models;
using Resend;

var builder = WebApplication.CreateBuilder(args);

// Existing Fynd service registrations remain here.

var firebaseProjectId =
    builder.Configuration["Firebase:ProjectId"];

if (string.IsNullOrWhiteSpace(firebaseProjectId))
{
    throw new InvalidOperationException(
        "Firebase:ProjectId is missing from configuration.");
}

if (FirebaseApp.DefaultInstance is null)
{
    var firebaseCredentialsPath =
        builder.Configuration["Firebase:CredentialsPath"];

    if (string.IsNullOrWhiteSpace(firebaseCredentialsPath))
    {
        firebaseCredentialsPath =
            @"D:\Sites\site90397\private\fynd-app-8165f-firebase-adminsdk-fbsvc-bfa40828c5.json";
    }

    if (!File.Exists(firebaseCredentialsPath))
    {
        throw new FileNotFoundException(
            "Firebase service account JSON file was not found.",
            firebaseCredentialsPath
        );
    }

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(
            firebaseCredentialsPath
        ),
        ProjectId = firebaseProjectId
    });
}


// Add services to the container.
builder.Services.AddControllers();
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ValidateIssuer = false,
                ValidateAudience = false,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddResend(options =>
{
    options.ApiToken =
        builder.Configuration["Resend:ApiKey"]
        ?? throw new InvalidOperationException(
            "Resend API Key is missing."
        );
});

builder.Services.AddScoped<IEmailService, EmailService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
