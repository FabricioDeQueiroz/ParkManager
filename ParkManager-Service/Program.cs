using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using ParkManager_Service.Data;
using ParkManager_Service.Services;
using ParkManager_Service.Services.Interfaces;
using ParkManager_Service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

var supportedCultures = new[] { new CultureInfo("pt-BR") };

builder.Configuration.AddEnvironmentVariables();

// Limita os logs durante o Teste para não ficar poluído:
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
    builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    builder.Logging.AddFilter("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", LogLevel.None);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ParkManager - Service", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT:"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowMultipleOrigins", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IEstacionamento, EstacionamentoService>();
builder.Services.AddScoped<IUsuario, UsuarioService>();
builder.Services.AddScoped<IEvento, EventoService>();
builder.Services.AddScoped<IAcesso, AcessoService>();
builder.Services.AddScoped<IEmail, EmailService>();

builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Configuração do Hangfire para usar PostgreSQL
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseDefaultTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));
});

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 10 });

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddHangfireServer();
}

var app = builder.Build();

// Migrations direto no Program.cs
if (!app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParkManager v1");
        c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
    });
}
else
{
    app.UsePathBase("/parkmanager-api");
    app.UseStaticFiles();

    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/parkmanager-api/swagger/v1/swagger.json", "ParkManager v1");
        c.RoutePrefix = "swagger";
        c.InjectStylesheet("/parkmanager-api/swagger-ui/SwaggerDark.css");
    });
}

app.UseCors("AllowMultipleOrigins");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    ApplyCurrentCultureToResponseHeaders = true
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Aplicando migrações automaticamente em produção:
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    if (!app.Environment.IsEnvironment("Test"))
    {
        app.UseHangfireDashboard("/hangfire",
            new DashboardOptions
            {
                Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
            }
        );
    }
}
else
{
    if (!app.Environment.IsEnvironment("Test"))
    {
        app.UseHangfireDashboard("/parkmanager-api/hangfire",
            new DashboardOptions
            {
                Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
            }
        );
    }
}

// Envio de e-mail todos os dias as 20h (UTC), para fins de demonstração
if (!builder.Environment.IsEnvironment("Test"))
{
    using (var scope = app.Services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<IEmail>(
            "Envio-Relatorio-Mensal",
            service => service.SendMailAsync(),
            "0 20 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
        );
    }
}

app.Run();

// Hangfire Authorization Filter para Gerentes
internal sealed class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}

// Para ser visível nos Testes
public abstract partial class Program { }
