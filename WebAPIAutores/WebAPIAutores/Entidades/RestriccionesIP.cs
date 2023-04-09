namespace WebAPIAutores.Entidades
{
    public class RestriccionesIP
    {
        public int Id { get; set; }
        public int LlaveId { get; set; }
        public string IP { get; set; }
        public LlaveAPI Llave { get; set; }
    }
}
