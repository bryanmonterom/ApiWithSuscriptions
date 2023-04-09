using System.ComponentModel.DataAnnotations;

namespace WebAPIAutores.DTOs
{
    public class ActualizarIPRestriccionDTO
    {
        [Required]
        public string IP { get; set; }
    }
}
