using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i servizi per i controller.
builder.Services.AddControllers();

// Configura Swagger e abilita annotazioni e commenti XML.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
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
app.UseAuthorization();
app.MapControllers();

app.Run();