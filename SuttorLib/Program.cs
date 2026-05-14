using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

using SuttorLibrary.Core;
using SuttorLibrary.Core.Services;
using SuttorLibrary.Data;
using SuttorLibrary.Middlewares;
using SuttorLibrary.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//CORS Configurations
var corsPolicy = "AllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policyBuilder =>
    {
        var allowedOrigins = builder.Configuration["CorsSettings:AllowedOrigins"]?.Split(",") ?? new[] { "http://localhost:3000" };

        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()  //For JWT cookies/headers
            .WithExposedHeaders("Content-Disposition")  //For file downloads
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});


// Configure form limits
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueCountLimit = 2048;
    options.KeyLengthLimit = 8192;
    options.ValueLengthLimit = 104857600; // 100 MB
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
    options.MultipartHeadersLengthLimit = 16384; // 16 KB
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("Default")!)
);

builder.Services.AddIdentity<AppUser, IdentityRole>(
    options => {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
    }
).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();



builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Add Jwt Authentication
builder.Services.AddAuthentication(
    options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = true;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
        RequireExpirationTime = true,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    o.TokenValidationParameters.ValidTypes = new[] { "JWT" };

    o.Events = new JwtBearerEvents()
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("JwtAuth");

            foreach (var keyValuePair in context.Response.Headers)
            {
                Console.WriteLine($"{keyValuePair.Key} : {keyValuePair.Value}");
                logger.LogWarning(context.Exception.Message, $"{keyValuePair.Key} : {keyValuePair.Value}");
            }
            Console.WriteLine("OnAuthenticationFailed : " + context.Exception.Message);

            return Task.CompletedTask;
        },

        OnTokenValidated = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuth");
            logger.LogInformation("Token validated for {sub}", ctx.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(new
            {
                Status = 401,
                Message = "You are not authorized",
                Detail = context.ErrorDescription
            });
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(new
            {
                Status = 403,
                Message = "You do not have the permission to access this resource"
            });
        },
    };
});

builder.Services.AddAuthorization();

builder.Services.AddLogging();

builder.Services.AddExceptionHandler<AppExceptionHandler>();

var app = builder.Build();

//using var scope = app.Services.CreateScope();
//var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
//var fixedJoinedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);

//if (await um.FindByNameAsync("AdminSaleh") == null)
//{
//    string adminId = "29d93d5b-efbc-4ac7-999b-7b211629d8b0";


//    //Users
//    AppUser admin = new AppUser
//    {
//        Id = adminId,
//        UserName = "AdminSaleh",
//        NormalizedUserName = "ADMINSALEH",
//        FullName = "Saleh",
//        Email = "salehalk512@gmail.com",
//        NormalizedEmail = "SALEHALK512@GMAIL.COM",
//        EmailConfirmed = true,
//        JoinedAt = fixedJoinedAt,
//        IsAuthor = false,
//        XP = 100000,
//        PhotoPath = "",
//        IsAuthed = true,
//        Token = "",
//        ExpiresAt = DateTime.MinValue,
//    };


//    PasswordHasher<AppUser> ph = new PasswordHasher<AppUser>();
//    admin.PasswordHash = ph.HashPassword(admin, "Tbmfilj@72534");
//}
//if (um.FindByNameAsync("User") == null)
//{
//    string userId = "f422f142-09b2-40b9-a886-a14b213973d5";
//    AppUser user = new AppUser
//    {
//        Id = userId,
//        UserName = "User",
//        NormalizedUserName = "USER",
//        FullName = "Demo User",
//        Email = "userDemo@example.com",
//        NormalizedEmail = "USERDEMO@EXAMPLE.COM",
//        EmailConfirmed = true,
//        JoinedAt = fixedJoinedAt,
//        IsAuthor = false,
//        XP = 0,
//        PhotoPath = "",
//        IsAuthed = true,
//        Token = "",
//        ExpiresAt = DateTime.MinValue,
//    };

//    PasswordHasher<AppUser> ph = new PasswordHasher<AppUser>();
//    user.PasswordHash = ph.HashPassword(user, "UserDemo12345!");   
//}
//if (um.FindByNameAsync("AuthorUser") == null)
//{
//    string authorId = "b7359b68-b61a-4991-8c9f-b6394494b11a";

//    AppUser author = new AppUser
//    {
//        Id = authorId,
//        UserName = "AuthorUser",
//        NormalizedUserName = "Author",
//        FullName = "Author User",
//        Email = "authorDemo@example.com",
//        NormalizedEmail = "AuthorDEMO@EXAMPLE.COM",
//        EmailConfirmed = true,
//        JoinedAt = fixedJoinedAt,
//        IsAuthor = true,
//        XP = 1000,
//        PhotoPath = "",
//        IsAuthed = true,
//        Token = "",
//        ExpiresAt = DateTime.MinValue,
//    };


//    PasswordHasher<AppUser> ph = new PasswordHasher<AppUser>();
//    author.PasswordHash = ph.HashPassword(author, "AuthorDemo12345!");
//}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();

//    app.MapScalarApiReference(options => {
//        options.Title = "Suttor Library API Documentation";
//        options.Theme = ScalarTheme.DeepSpace;
//        options.AddPreferredSecuritySchemes("Bearer");
//        // Authentication/Security configuration
//        options.Authentication = new ScalarAuthenticationOptions
//        {
//            PreferredSecuritySchemes = ["Bearer"]
//        };
//    });
//}

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.Title = "Suttor Library API Documentation";
    options.Theme = ScalarTheme.DeepSpace;
    options.AddPreferredSecuritySchemes("Bearer");
    // Authentication/Security configuration
    options.Authentication = new ScalarAuthenticationOptions
    {
        PreferredSecuritySchemes = ["Bearer"]
    };
});

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
