using Microsoft.AspNetCore.Cors;
using MinimalAPIPeliculas.Entidades;


var builder = WebApplication.CreateBuilder(args);

var origenePermitidos = builder.Configuration.GetValue<string>("origenesPermitidos")!;

builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(configuracion =>
    {
        configuracion.WithOrigins(origenePermitidos).AllowAnyHeader().AllowAnyMethod();
    });

    opciones.AddPolicy("libre", configuracion =>
    {
        configuracion.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddOutputCache();
builder.Services.AddEndpointsApiExplorer(); //habilita que swagger explore nuestros endpoints
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseOutputCache();

app.MapGet("/", [EnableCors(policyName: "libre")]() => "Hello World!");

app.MapGet("/generos", () =>
{
    var generos = new List<Genero>
    {
        new Genero
        {
            Id = 1,
            Nombre = "Drama"
        },
        new Genero
        {
            Id = 2,
            Nombre = "Acción"
        },
        new Genero
        {
            Id = 3,
            Nombre = "Comedia"
        }
    };

    return generos;
}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(15)));

app.Run();
