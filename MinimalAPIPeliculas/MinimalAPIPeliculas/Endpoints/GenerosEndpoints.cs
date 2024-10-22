﻿using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class GenerosEndpoints
    {
        public static RouteGroupBuilder MapGeneros(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerGeneros).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("generos-get"));
            group.MapGet("/{id:int}", ObtenerGeneroPorId);

            group.MapPost("/", CrearGenero).AddEndpointFilter<FiltroValidaciones<CrearGeneroDTO>>().RequireAuthorization("esadmin");
            group.MapPut("/{id:int}", ActualizarGenero)
                .AddEndpointFilter<FiltroValidaciones<CrearGeneroDTO>>()
                .RequireAuthorization("esadmin")
                .WithOpenApi(opciones =>
                {
                    opciones.Summary = "Actualizar un género";
                    opciones.Description = "Con este endpoint podemos actualizar un género";
                    opciones.Parameters[0].Description = "El id del género a actualizar";
                    opciones.RequestBody.Description = "El género que se desea actualizar";

                    return opciones;
                });
            group.MapDelete("/{id:int}", BorrarGenero).RequireAuthorization("esadmin");

            return group;
        }

        static async Task<Ok<List<GeneroDTO>>> ObtenerGeneros(IRepositorioGeneros repositorio, IMapper mapper, ILoggerFactory loggerFactory)
        {
            var tipo = typeof(GenerosEndpoints);
            var logger = loggerFactory.CreateLogger(tipo.FullName!);
            logger.LogInformation("Obteniendo el listado de géneros");

            var generos = await repositorio.ObtenerTodos();
            var generosDTO = mapper.Map<List<GeneroDTO>>(generos);
            return TypedResults.Ok(generosDTO);
        }

        static async Task<Results<Ok<GeneroDTO>, NotFound>> ObtenerGeneroPorId([AsParameters] ObtenerGeneroPorIdPeticionDTO modelo)
        {
            var genero = await modelo.Repositorio.ObtenerPorId(modelo.Id);
            if (genero is null) return TypedResults.NotFound();

            var generoDTO = modelo.Mapper.Map<GeneroDTO>(genero);

            return TypedResults.Ok(generoDTO);
        }

        static async Task<Results<Created<GeneroDTO>, ValidationProblem>> CrearGenero(CrearGeneroDTO crearGeneroDTO, IRepositorioGeneros repositorio, 
            IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var genero = mapper.Map<Genero>(crearGeneroDTO);
            var id = await repositorio.Crear(genero);
            await outputCacheStore.EvictByTagAsync("generos-get", default);

            var generoDTO = mapper.Map<GeneroDTO>(genero);

            return TypedResults.Created($"/generos/{id}", generoDTO);
        }

        static async Task<Results<NoContent, NotFound, ValidationProblem>> ActualizarGenero(int id, CrearGeneroDTO crearGeneroDTO, IRepositorioGeneros repositorio,
            IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var existe = await repositorio.Existe(id);
            if (!existe) return TypedResults.NotFound();

            var genero = mapper.Map<Genero>(crearGeneroDTO);
            genero.Id = id;

            await repositorio.Actualizar(genero);
            await outputCacheStore.EvictByTagAsync("generos-get", default);

            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> BorrarGenero(int id, IRepositorioGeneros repositorio, IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);
            if (!existe) return TypedResults.NotFound();

            await repositorio.Borrar(id);
            await outputCacheStore.EvictByTagAsync("generos-get", default);

            return TypedResults.NoContent();
        }
    }
}
