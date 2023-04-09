using Microsoft.AspNetCore.Identity;

namespace WebAPIAutores.Entidades
{
    public class Usuario:IdentityUser
    {
        public bool MalaPaga { get; set; }
        public List<Factura> Factura { get; set; }
    }
}
