using FluentValidation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIPeliculas;
using MinimalAPIPeliculas.Endpoints;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;
using MinimalAPIPeliculas.Utilidades;


var builder = WebApplication.CreateBuilder(args);

var origenePermitidos = builder.Configuration.GetValue<string>("origenesPermitidos")!;

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
{
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentityCore<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<UserManager<IdentityUser>>();
builder.Services.AddScoped<SignInManager<IdentityUser>>();

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
builder.Services.AddScoped<IRepositorioErrores, RepositorioErrores>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
//builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();
//builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenarArchivosCloudinary>();
builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenarArchivosLocal>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false;
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //IssuerSigningKey = Llaves.ObtenerLlave(builder.Configuration),
        IssuerSigningKeys = Llaves.ObtenerTodasLasLlave(builder.Configuration),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MinimalAPIPeliculas");
    c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
});

app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error!;

    var error = new Error();
    error.Fecha = DateTime.UtcNow;
    error.MensajeDeError = exception.Message;
    error.StackTrace = exception.StackTrace;

    var repositorio = context.RequestServices.GetRequiredService<IRepositorioErrores>();
    await repositorio.Crear(error);

    await TypedResults.BadRequest(new { tipo = "error", mensaje = "ha ocurrido un mensaje de error inesperado", estatus = 500 })
        .ExecuteAsync(context);
}));
app.UseStatusCodePages();

app.UseStaticFiles();

app.UseCors();
app.UseOutputCache();

app.UseAuthorization();

app.MapGet("/", [EnableCors(policyName: "libre")]() => "Hello World!");
app.MapGet("/error", () =>
{
    throw new InvalidOperationException("error de ejemplo");
});

app.MapGroup("/generos").MapGeneros();
app.MapGroup("/actores").MapActores();
app.MapGroup("/peliculas").MapPeliculas();
app.MapGroup("/pelicula/{peliculaId:int}/comentarios").MapComentarios();
app.MapGroup("/usuarios").MapUsuarios();

app.Run();