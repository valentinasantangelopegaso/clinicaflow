using System.Reflection;
using System.Text;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i servizi per i controller.
builder.Services.AddControllers();

// Configura autenticazione JWT.
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key non configurata.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer non configurato.");
// var jwtKey = builder.Configuration["Jwt:Key"] ?? "supersegretodev"; // sposta la chiave nel configuration file
// var issuer = builder.Configuration["Jwt:Issuer"] ?? "ClinicaFlow";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly", p => p.RequireRole("Doctor"));
    options.AddPolicy("PatientOnly", p => p.RequireRole("Patient"));
});

// Configura Swagger e abilita annotazioni e commenti XML.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

     // Aggiunge il supporto Bearer in Swagger.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Inserire il token JWT nel formato: Bearer {token}"
    });

    options.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement();

        requirement[new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>();

        return requirement;
    });
});

builder.Services.AddDbContext<ClinicaFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicaFlowDb")));

var app = builder.Build();

// Abilita Swagger in ambiente di sviluppo.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicaFlowDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}

app.Run();