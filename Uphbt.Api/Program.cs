// Temporarily add this line for debugging:


using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Uphbt.Data;
using Uphbt.Identity;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// database and authentication
{
    var connectionString
        = builder.Configuration.GetConnectionString("DefaultConnection")
          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

    Console.WriteLine("Connection string: " + connectionString);
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

    builder.Services.AddIdentityCore<User>(options =>
        {
            // Password settings (example) - these are still important for user creation
            options.Password.RequireDigit           = true;
            options.Password.RequireLowercase       = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase       = true;
            options.Password.RequiredLength         = 8;
            options.Password.RequiredUniqueChars    = 1;

            // Lockout settings (example)
            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers      = true;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Sign-in settings (these will primarily be used by SignInManager, which is still registered by AddIdentityCore)
            options.SignIn.RequireConfirmedAccount = false; // Set to true for email confirmation
        })
        .AddRoles<IdentityRole<long>>()                   // Add role support explicitly for IdentityRole<long>
        .AddEntityFrameworkStores<ApplicationDbContext>() // Specifies your DbContext for Identity
        .AddDefaultTokenProviders();                      // Used for password resets, email confirmations etc.


    // --- JWT settings configuration (reading from appsettings.json) also precedes it ---
    // Get JWT settings from configuration
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ??
                                      throw new InvalidOperationException("JWT Secret not configured."));

    builder.Services.AddAuthentication(options =>
        {
            // Make JWT Bearer the default if most protected endpoints will use it
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            // Configuration for a REFRESH TOKEN cookie (if you choose to have one)
            options.Cookie.Name         = "refreshToken"; // Name your refresh token cookie
            options.Cookie.HttpOnly     = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // IMPORTANT
            options.Cookie.SameSite     = SameSiteMode.Strict;
            options.ExpireTimeSpan      = TimeSpan.FromDays(jwtSettings.GetValue<int>("RefreshTokenExpiryDays"));
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
            };
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken            = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(key),
                ValidateIssuer           = true,
                ValidIssuer              = jwtSettings["Issuer"],
                ValidateAudience         = true,
                ValidAudience            = jwtSettings["Audience"],
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            };

            // --- CRUCIAL CHANGE HERE: Extract JWT from HttpOnly cookie ---
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Try to read the token from a specific cookie first
                    if (context.Request.Cookies.TryGetValue("accessToken", out var accessToken))
                        context.Token = accessToken;

                    // If not in cookie, or if you want to support Authorization header as a fallback
                    // else if (context.Request.Headers.ContainsKey("Authorization"))
                    // {
                    //     string? bearerToken = context.Request.Headers["Authorization"].FirstOrDefault();
                    //     if (!string.IsNullOrEmpty(bearerToken) && bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    //     {
                    //         context.Token = bearerToken.Substring("Bearer ".Length);
                    //     }
                    // }
                    return Task.CompletedTask;
                }
                // You can add other events like OnAuthenticationFailed, OnTokenValidated if needed
            };
        });
}

var app = builder.Build();


// Add services to the container.
Console.WriteLine(app.Environment.IsDevelopment());
Console.WriteLine($"Current ASPNETCORE_ENVIRONMENT: {app.Environment.EnvironmentName}");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/openapi/v1.json", "V1 API"); });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();