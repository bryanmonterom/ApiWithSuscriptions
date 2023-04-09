using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/restriccionesip")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestriccionesIpController:CustomBaseController
    {
        private readonly ApplicationDbContext context;

        public RestriccionesIpController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearRestriccionesIPDTO crearRestriccionesIPDTO) {

            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(a => a.Id == crearRestriccionesIPDTO.LlaveId);
            if (llaveDB == null)
            {
                return NotFound();
            }

            var usuarioId = ObtenerUsuarioId();

            if (llaveDB.UsuarioId != usuarioId) {

                return Forbid();
            }

            var restriccionIp = new RestriccionesIP() { LlaveId = llaveDB.Id, IP = crearRestriccionesIPDTO.IP };
            context.Add(restriccionIp);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ActualizarIPRestriccionDTO actualizarRestriccionIPDTO)
        {

            var restriccion = await context.RestriccionesIP.Include(a => a.Llave).FirstOrDefaultAsync(a => a.Id == id);

            if (restriccion == null)
            {
                return NotFound();
            }

            var usuarioId = ObtenerUsuarioId();
            if (usuarioId != restriccion.Llave.UsuarioId)
            {
                return Forbid();
            }

            restriccion.IP = actualizarRestriccionIPDTO.IP;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {

            var restriccion = await context.RestriccionesIP.Include(a => a.Llave).FirstOrDefaultAsync(a => a.Id == id);

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
