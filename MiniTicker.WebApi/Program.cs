using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiniTicker.Core.Application;
using MiniTicker.Infrastructure;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.WebApi.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using MiniTicker.Infrastructure.Persistence.Seeds;
using Microsoft.Extensions.FileProviders; // Necesario para PhysicalFileProvider
using System.IO; // Necesario para Path y Directory

var builder = WebApplication.CreateBuilder(args);

// =======================================================================
// 1. CONFIGURACIÓN DE SERVICIOS (BUILDER)
// =======================================================================

// Limpia el mapeo por defecto de claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

// CORS: Permitir acceso desde cualquier origen (útil para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controllers con configuración para ignorar ciclos en JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MiniTicker API", Version = "v1" });

    // Definición de seguridad JWT para Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT."
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
            new string[] {}
        }
    });
});

// Configuración de Autenticación JWT
var signingKey = builder.Configuration.GetValue<string>("Jwt:Key");
if (string.IsNullOrWhiteSpace(signingKey))
    throw new InvalidOperationException("Falta la configuración 'Jwt:Key' en appsettings.");

builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "nombre",
            RoleClaimType = ClaimTypes.Role
        };
    });

// Capas de Aplicación e Infraestructura
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor(); // Importante para generar URLs completas

// =======================================================================
// 2. CONSTRUCCIÓN DE LA APP
// =======================================================================

var app = builder.Build();

// =======================================================================
// 3. CONFIGURACIÓN DE CARPETA UPLOADS (ARCHIVOS ESTÁTICOS)
// =======================================================================

// Usamos ContentRootPath para asegurar la ruta correcta
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");

// Crear la carpeta si no existe
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Habilitar acceso web a la carpeta uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Forzar descarga agregando header Content-Disposition
        ctx.Context.Response.Headers.Append(
            "Content-Disposition", $"attachment; filename=\"{ctx.File.Name}\"");
    }
});

// Habilitar archivos estáticos por defecto (wwwroot) si los usas
app.UseStaticFiles(); 

// =======================================================================
// 4. SEEDER (DATOS INICIALES)
// =======================================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al ejecutar el Seed de datos.");
    }
}

// =======================================================================
// 5. PIPELINE DE MIDDLEWARE (ORDEN IMPORTANTE)
// =======================================================================

app.UseMiddleware<ErrorHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS debe ir antes de Auth
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();