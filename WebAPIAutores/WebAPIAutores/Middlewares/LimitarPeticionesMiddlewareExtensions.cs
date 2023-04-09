using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Middlewares
{
    public static class LimitarPeticionesMiddlewareExtensions
    {
        public static IApplicationBuilder UseLimitarPeticiones(this IApplicationBuilder app) {

            return app.UseMiddleware<UseLimitarPeticiones>();
        }
    }

    public class UseLimitarPeticiones {
        private readonly RequestDelegate siguiente;
        private readonly IConfiguration configuration;

        public UseLimitarPeticiones(RequestDelegate siguiente, IConfiguration configuration)
        {
            this.siguiente = siguiente;
            this.configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext httpContext, ApplicationDbContext context) {

            var limitarPeticionesConfiguracion = new LimitarPeticionesConfiguracion();
            configuration.GetRequiredSection("limitarPeticiones").Bind(limitarPeticionesConfiguracion);

            var llaveStringValues = httpContext.Request.Headers["X-Api-Key"];
            var ruta = httpContext.Request.Path.ToString();

            var estaLaRutaEnListaBlanca = limitarPeticionesConfiguracion.ListaBlancaRutas.Any(a => ruta.Contains(a));

            if (estaLaRutaEnListaBlanca) {

                await siguiente(httpContext);
                return;
            }

            if (llaveStringValues.Count == 0) {

                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Debes proveer la llave en la cabecera X-Api-Key");
                return;
            }

            if (llaveStringValues.Count > 1)
            {

                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Solo una llave debe estar presente");
                return;
            }

            var llave = llaveStringValues[0];

            var llaveDB = await context.LlavesAPI.Include(a=> a.RestriccionDominio).Include(a=> a.RestriccionesIP).FirstOrDefaultAsync(a => a.Llave == llave);
            if (llaveDB == null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave provista no existe");
                return;
            }

            if (!llaveDB.Activa)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave se encuentra inactiva");
                return;
            }

            if (llaveDB.TipoLlave == Entidades.TipoLlave.Gratuita) {

                var hoy = DateTime.Today;
                var manana = hoy.AddDays(1);
                var cantidadPeticionesRealizadasHoy = await context.Peticiones.CountAsync(a => a.LlaveId == llaveDB.Id && a.FechaPeticion >= hoy && a.FechaPeticion < manana);
                if (cantidadPeticionesRealizadasHoy >= limitarPeticionesConfiguracion.PeticionesPorDiaGratuito) {

                    httpContext.Response.StatusCode = 429; // Too many request
                    await httpContext.Response.WriteAsync("Ha excedido el limite de peticiones por día. Si desea realizar mas peticiones," +
                        " actualice su suscripción a una cuenta profesional");
                    return;
                }


            }

            var superaRestricciones = PeticionSuperaAlgunaDeLasRestricciones(llaveDB, httpContext);
            if (!superaRestricciones)
            {

                httpContext.Response.StatusCode = 403;
                await httpContext.Response.WriteAsync("Esta llave no tiene permitido request desde este dominio o esta IP");
                return;
            }



            var peticion = new Peticion() { LlaveId = llaveDB.Id, FechaPeticion = DateTime.UtcNow };
            context.Add(peticion);
            await context.SaveChangesAsync();

            await siguiente(httpContext);
        }

        private bool PeticionSuperaAlgunaDeLasRestricciones(LlaveAPI llaveAPI, HttpContext httpContext) {

            
            var hayRestricciones = llaveAPI.RestriccionDominio == null || llaveAPI.RestriccionesIP == null;
            if (hayRestricciones) {
                return true;
            }

            var peticionSuperaRestriccionesDeDominio = PeticionSuperaRestriccionDominio(llaveAPI.RestriccionDominio.ToList(), httpContext);

            var peticionSuperaRestriccionesDeIP = PeticionSuperaRestriccionIP(llaveAPI.RestriccionesIP.ToList(), httpContext);


            return peticionSuperaRestriccionesDeDominio || peticionSuperaRestriccionesDeIP;
        }

        private bool PeticionSuperaRestriccionDominio(List<RestriccionDominio> restricciones, HttpContext httpContext) {

            if (restricciones == null || restricciones.Count == 0) {

                return false;
            }

            var referer = httpContext.Request.Headers["Referer"].ToString();

            if (referer == string.Empty) {
                return false;
            }

            Uri myURI = new Uri(referer);
            string host = myURI.Host;

            var superaRestriccion = restricciones.Any(a => a.Dominio == host);

            return superaRestriccion;


        }

        private bool PeticionSuperaRestriccionIP(List<RestriccionesIP> restricciones, HttpContext httpContext)
        {

            if (restricciones == null || restricciones.Count == 0)
            {
                return false;
            }

            var ip = httpContext.Connection.RemoteIpAddress.ToString();
            if (ip == string.Empty)
            {
                return false;
            }

            var superaRestriccion = restricciones.Any(a => a.IP == ip);
            return superaRestriccion;
        }

    }
}
