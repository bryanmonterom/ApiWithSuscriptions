namespace WebAPIAutores.DTOs
{
    public class LimitarPeticionesConfiguracion
    {
        public int PeticionesPorDiaGratuito { get; set; }
        public List<string> ListaBlancaRutas { get; set; }

    }
}
