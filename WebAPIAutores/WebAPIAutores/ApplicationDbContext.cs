using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.Entidades;

namespace WebAPIAutores
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AutorLibro>()
                .HasKey(al => new { al.AutorId, al.LibroId });

            modelBuilder.Entity<Factura>().HasOne(a => a.Usuario).WithMany(a => a.Factura).HasForeignKey(a => a.UsuarioId);

            modelBuilder.Entity<Factura>().Property(a => a.Monto).HasColumnType("decimal(18,2)");

        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<FacturaEmitida> FacturasEmitidas { get; set; }

        public DbSet<LlaveAPI> LlavesAPI { get; set; }
        public DbSet<RestriccionesIP> RestriccionesIP { get; set; }
        public DbSet<RestriccionDominio> RestriccionDominio { get; set; }
        public DbSet<Peticion> Peticiones { get; set; }
        public DbSet<AutorLibro> AutoresLibros { get; set; }

    }
}
