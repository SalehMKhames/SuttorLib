using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SuttorLib.Core.Services.Blog;
using SuttorLib.Core.Services.Blogs;
using SuttorLib.Data;
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

//Connect to MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("Default")!)
);

//Configuring MongoDB connection
builder.Services.Configure<BlogDbSettings>(
    builder.Configuration.GetSection("BlogDbSettings")
)
.AddOptions<BlogDbSettings>()
.Bind(builder.Configuration.GetSection("BlogDbSettings"))
.ValidateDataAnnotations();


builder.Services.AddIdentity<AppUser, IdentityRole>(
    options => {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
    }
).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//CORS Configurations
var corsPolicy = "AllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policyBuilder =>
    {
        var allowedOrigins = builder.Configuration["CorsSettings:AllowedOrigins"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .Where(o => !string.IsNullOrEmpty(o))
            .ToArray()
            ?? new[] { "http://localhost:3000" };

        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()  //For JWT cookies/headers
            .WithExposedHeaders("Content-Disposition")  //For file downloads
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});


builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IBlogServices, BlogService>();

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

app.UseForwardedHeaders(new ForwardedHeadersOptions { 
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

var provider = new FileExtensionContentTypeProvider();
//Files
provider.Mappings[".epub"] = "application/epub+zip";
provider.Mappings[".pdf"] = "application/pdf";
provider.Mappings[".doc"] = "application/msword";
provider.Mappings[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
//Photos
provider.Mappings[".webp"] = "image/webp";
provider.Mappings[".heic"] = "image/heic";
provider.Mappings[".jpg"] = "image/jpeg";
provider.Mappings[".jpeg"] = "image/jpeg";
provider.Mappings[".png"] = "image/png";
provider.Mappings[".heif"] = "image/heif";
provider.Mappings[".bmp"] = "image/bmp";
provider.Mappings[".raw"] = "image/heic";

app.UseStaticFiles( new StaticFileOptions 
{
    FileProvider = new PhysicalFileProvider(builder.Environment.ContentRootPath),
    ContentTypeProvider = provider 
});

app.UseCors(corsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
