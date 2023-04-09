using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/restriccionesdominio")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestriccionesDominioController:CustomBaseController
    {
        private readonly ApplicationDbContext context;

        public RestriccionesDominioController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearRestriccionesDominioDTO crearRestriccionesDominioDTO) {

            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(a => a.Id == crearRestriccionesDominioDTO.LlaveID);
            if (llaveDB == null)
            {
                return NotFound();
            }

            var usuarioId = ObtenerUsuarioId();
            if(usuarioId != llaveDB.UsuarioId) {
                return Forbid();
            }

            var restriccionesDominio = new RestriccionDominio() { 
            
                Dominio = crearRestriccionesDominioDTO.Dominio,
                LlaveId = crearRestriccionesDominioDTO.LlaveID
            };

            context.Add(restriccionesDominio);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ActualizarRestriccionDominioDTO actualizarRestriccionDominioDTO)
        {

            var restriccion = await context.RestriccionDominio.Include(a=> a.Llave).FirstOrDefaultAsync(a => a.Id == id);
            
            if (restriccion == null)
            {
                return NotFound();
            }

            var usuarioId = ObtenerUsuarioId();
            if (usuarioId != restriccion.Llave.UsuarioId)
            {
                return Forbid();
            }

            restriccion.Dominio = actualizarRestriccionDominioDTO.Dominio;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id) {

            var restriccion = await context.RestriccionDominio.Include(a => a.Llave).FirstOrDefaultAsync(a => a.Id == id);

            if (restriccion == null)
            {
                return NotFound();
            }

            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == restriccion.Llave.UsuarioId)
            {
                return Forbid();
            }

            context.Remove(restriccion);
           await context.SaveChangesAsync();
            return NoContent();

        }
    }
}
