
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

// Firebase configuration
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
        firebaseCredentialsPath = Path.GetFullPath(
            Path.Combine(
                builder.Environment.ContentRootPath,
                "..",
                "private",
                "fynd-app-8165f-firebase-adminsdk-fbsvc-bfa40828c5.json"
            )
        );
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

// Controllers
builder.Services.AddControllers();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
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

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"
            )
        )
);

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT access token."
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
        }
    );
});

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Resend email configuration
builder.Services.AddResend(options =>
{
    options.ApiToken =
        builder.Configuration["Resend:ApiKey"]
        ?? throw new InvalidOperationException(
            "Resend API Key is missing."
        );
});

builder.Services.AddScoped<IEmailService, EmailService>();

// Build application
var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
