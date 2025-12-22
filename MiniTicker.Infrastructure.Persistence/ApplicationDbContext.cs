using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Infrastructure.Persistence
{
    /// <summary>
    /// DbContext de persistencia para MiniTicker.
    /// Mapea las entidades del dominio y configura relaciones y columnas mediante Fluent API.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets por entidad del dominio
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Area> Areas { get; set; } = null!;
        public DbSet<TipoSolicitud> TiposSolicitud { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<Comentario> Comentarios { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración por entidad usando Fluent API
            ConfigureArea(modelBuilder);
            ConfigureTipoSolicitud(modelBuilder);
            ConfigureUsuario(modelBuilder);
            ConfigureTicket(modelBuilder);
            ConfigureComentario(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        #region Entity configurations

        private static void ConfigureArea(ModelBuilder builder)
        {
            var entity = builder.Entity<Area>();
            entity.ToTable("Areas");

            // Clave primaria
            entity.HasKey(e => e.Id);

            // Propiedades
            entity.Property(e => e.Nombre)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.Activo)
                  .IsRequired();

            // Auditable properties (inhered) - asegurar columna si es necesario
            //entity.Property<DateTime>("FechaCreacion").IsRequired();
            //entity.Property<DateTime?>("FechaModificacion");
        }

        private static void ConfigureTipoSolicitud(ModelBuilder builder)
        {
            var entity = builder.Entity<TipoSolicitud>();
            entity.ToTable("TipoSolicitudes");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.Activo)
                  .IsRequired();

            // Relacion con Area
            entity.HasOne<Area>()
                  .WithMany()
                  .HasForeignKey(e => e.AreaId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            //entity.Property<DateTime>("FechaCreacion").IsRequired();
            //entity.Property<DateTime?>("FechaModificacion");
        }

        private static void ConfigureUsuario(ModelBuilder builder)
        {
            var entity = builder.Entity<Usuario>();
            entity.ToTable("Usuarios");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(256);

            // Guardar el enum Rol como string para mayor legibilidad en DB
            entity.Property(e => e.Rol)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.Activo)
                  .IsRequired();

            entity.Property(e => e.FotoPerfilUrl)
                  .HasMaxLength(500);

            //entity.Property<DateTime>("FechaCreacion").IsRequired();
            //entity.Property<DateTime?>("FechaModificacion");
        }

        private static void ConfigureTicket(ModelBuilder builder)
        {
            var entity = builder.Entity<Ticket>();
            entity.ToTable("Tickets");

            entity.HasKey(e => e.Id);

            // Número único por ticket
            entity.Property(e => e.Numero)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.HasIndex(e => e.Numero).IsUnique();

            entity.Property(e => e.Asunto)
                  .IsRequired()
                  .HasMaxLength(250);

            entity.Property(e => e.Descripcion)
                  .IsRequired()
                  .HasMaxLength(4000);

            // Enums como strings
            entity.Property(e => e.Prioridad)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.Estado)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.ArchivoAdjuntoUrl)
                  .HasMaxLength(500);

            entity.Property(e => e.FechaActualizacion);

            // Relaciones
            entity.HasOne<Area>()
                  .WithMany()
                  .HasForeignKey(e => e.AreaId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne<TipoSolicitud>()
                  .WithMany()
                  .HasForeignKey(e => e.TipoSolicitudId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            // Solicitante (usuario) - requerido
            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(e => e.SolicitanteId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            // Gestor asignado - opcional, al eliminar usuario dejar null el gestor
            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(e => e.GestorAsignadoId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);

            //entity.Property<DateTime>("FechaCreacion").IsRequired();
            //entity.Property<DateTime?>("FechaModificacion");
        }

        private static void ConfigureComentario(ModelBuilder builder)
        {
            var entity = builder.Entity<Comentario>();
            entity.ToTable("Comentarios");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Texto)
                  .IsRequired()
                  .HasMaxLength(2000);

            entity.Property(e => e.Fecha)
                  .IsRequired();

            // Relaciones
            entity.HasOne<Ticket>()
                  .WithMany()
                  .HasForeignKey(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();
        }

        #endregion
    }
}