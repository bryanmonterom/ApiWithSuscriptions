using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Servicios;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/llavesapi")]
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class LLavesController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ServicioLlaves servicioLlaves;

        public LLavesController(ApplicationDbContext context, IMapper mapper, ServicioLlaves servicioLlaves)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioLlaves = servicioLlaves;
        }

        [HttpGet]
        public async Task<List<LlaveDTO>> MisLlaves()
        {

            var usuarioId = ObtenerUsuarioId();
            var llaves = await context.LlavesAPI.Where(a => a.UsuarioId == usuarioId).ToListAsync();
            return mapper.Map<List<LlaveDTO>>(llaves);
        }

        [HttpPost]
        public async Task<ActionResult> CrearLlave(CrearLlaveDTO crearLlaveDTO)
        {
            var usuarioId = ObtenerUsuarioId();
            if (crearLlaveDTO.TipoLlave == Entidades.TipoLlave.Gratuita)
            {
                var theUserHasAFreeKey = await context.LlavesAPI.AnyAsync(a => a.Usuario.Id == usuarioId && a.TipoLlave == Entidades.TipoLlave.Gratuita);
                return BadRequest("El usuario ya tiene una llave gratuita");
            }

            await servicioLlaves.CrearLlave(usuarioId, crearLlaveDTO.TipoLlave);
            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> ActualizarLlave(ActualizarLlaveDTO actualizarLlaveDTO) {

            var usuarioId = ObtenerUsuarioId();
            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(a => a.Id == actualizarLlaveDTO.LlaveId);

            if (llaveDB is null) {

                return NotFound();
            }

            if (usuarioId != llaveDB.UsuarioId) {

                return Forbid();
            }

            if (actualizarLlaveDTO.ActualizarLlave) {

                llaveDB.Llave = servicioLlaves.GenerarLlave();
            }

            llaveDB.Activa = actualizarLlaveDTO.Activa;

            await context.SaveChangesAsync();
            return NoContent();


        }

    }
}
