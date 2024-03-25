
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MinimalAPIPeliculas.Servicios
{
    public class AlmacenarArchivosCloudinary : IAlmacenadorArchivos
    {
        private readonly IConfiguration _configuration;

        public AlmacenarArchivosCloudinary(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> Almacenar(string contenedor, IFormFile archivo)
        {
            var cloudinary = new Cloudinary(new Account(
                cloud: _configuration.GetSection("CloudinarySettings:CloudName").Value,
                apiKey: _configuration.GetSection("CloudinarySettings:ApiKey").Value,
                apiSecret: _configuration.GetSection("CloudinarySettings:ApiSecret").Value
            ));

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(archivo.FileName, archivo.OpenReadStream())
            };

            var uploadResult = cloudinary.Upload(uploadParams);

            return uploadResult.Url.ToString();
        }

        public Task Borrar(string? ruta, string contenedor)
        {
            throw new NotImplementedException();
        }
    }
}
