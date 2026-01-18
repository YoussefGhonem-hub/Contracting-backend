using Contracting.API;
using Contracting.API.Middleware;
using Contracting.Application;
using Contracting.Domain.Entities;
using Contracting.Infrustructure;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Logging.Serilog;
using Hangfire;
using Hangfire.MemoryStorage;
using Storage.AWS3;
using Emails.SendGrid;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAmazonS3(builder.Configuration);
builder.Services.AddSendGridEmail(builder.Configuration);

// Hangfire configuration (requires Hangfire.AspNetCore and a storage provider)
builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();


var jwtSection = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Contracting API", 
        Version = "v1",
        Description = "API for Contracting Management System"
    });

    // Include XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Security scheme (Authorization button)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT token. Example: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Apply to all operations
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddCors(o => o.AddPolicy("default", p =>
{
    p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
}));
#region Logger
builder.AddSharedSerilog(serviceName: "ContractingAPI");
#endregion
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Apply migrations and seed ONCE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var env = services.GetRequiredService<IWebHostEnvironment>();

    await AppDbContextSeed.SeedAsync(db, userManager, roleManager, env);
}

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contracting API v1"); c.RoutePrefix = string.Empty; });

var options = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(options.Value);
app.UseStaticFiles();
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

// Start Hangfire server and expose dashboard at /hangfire with authorization
app.UseHangfireServer();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapControllers();
#region Logger
app.UseSharedHttpLogging();
#endregion
CurrentUser.Initialize(app.Services.GetRequiredService<IHttpContextAccessor>());


app.Run();
