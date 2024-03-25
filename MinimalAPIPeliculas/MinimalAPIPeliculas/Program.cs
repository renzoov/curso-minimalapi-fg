using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas;
using MinimalAPIPeliculas.Endpoints;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;


var builder = WebApplication.CreateBuilder(args);

var origenePermitidos = builder.Configuration.GetValue<string>("origenesPermitidos")!;

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
{
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

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

builder.Services.AddScoped<IRepositorioGeneros, RepositorioGeneros>();
builder.Services.AddScoped<IRepositorioActores, RepositorioActores>();
builder.Services.AddScoped<IRepositorioPeliculas, RepositorioPeliculas>();
builder.Services.AddScoped<IRepositorioComentarios, RepositorioComentarios>();
//builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();
//builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenarArchivosCloudinary>();
builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenarArchivosLocal>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseCors();
app.UseOutputCache();

app.MapGet("/", [EnableCors(policyName: "libre")]() => "Hello World!");

app.MapGroup("/generos").MapGeneros();
app.MapGroup("/actores").MapActores();
app.MapGroup("/peliculas").MapPeliculas();
app.MapGroup("/pelicula/{peliculaId:int}/comentarios").MapComentarios();

app.Run();