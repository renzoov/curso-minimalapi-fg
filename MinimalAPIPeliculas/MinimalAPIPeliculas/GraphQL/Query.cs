﻿using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.GraphQL
{
    public class Query
    {
        [Serial]
        [UsePaging]
        [UseProjection]
        [UseSorting]
        public IQueryable<Genero> ObtenerGeneros([Service] ApplicationDbContext context) => context.Generos;

        [Serial]
        [UsePaging]
        [UseProjection]
        [UseSorting]
        public IQueryable<Actor> ObtenerActores([Service] ApplicationDbContext context) => context.Actores;

        [Serial]
        [UsePaging]
        [UseProjection]
        [UseSorting]
        public IQueryable<Pelicula> ObtenerPeliculas([Service] ApplicationDbContext context) => context.Peliculas;
    }
}
