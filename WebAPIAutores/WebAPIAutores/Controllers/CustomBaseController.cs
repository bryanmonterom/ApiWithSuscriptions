using Microsoft.AspNetCore.Mvc;

namespace WebAPIAutores.Controllers
{
    public class CustomBaseController:ControllerBase
    {
        protected string ObtenerUsuarioId() { 
        
            var usuarioClaim = HttpContext.User.Claims.Where(a=> a.Type =="id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value.ToString();

            return usuarioId;
        }
    }
}
