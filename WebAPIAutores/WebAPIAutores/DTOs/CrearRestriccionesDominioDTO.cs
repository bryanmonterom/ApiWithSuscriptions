using System.ComponentModel.DataAnnotations;

namespace WebAPIAutores.DTOs
{
    public class CrearRestriccionesDominioDTO
    {
        public int LlaveID { get; set; }

        [Required]
        public string Dominio { get; set; }
    }
}
