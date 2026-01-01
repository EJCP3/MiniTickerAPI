using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Domain.Domain;
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
        public DbSet<TicketEvent> TicketEvents { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            // Configuración por entidad usando Fluent API
            ConfigureArea(modelBuilder);
            ConfigureTipoSolicitud(modelBuilder);
            ConfigureUsuario(modelBuilder);
            ConfigureTicket(modelBuilder);
            ConfigureComentario(modelBuilder);
            ConfigureTicketEvent(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(
      CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.FechaCreacion = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
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

            entity.HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
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

            entity.Property(e => e.PasswordHash)
             .IsRequired()
                 .HasMaxLength(200);

            // Guardar el enum Rol como string para mayor legibilidad en DB
            entity.Property(e => e.Rol)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.Activo)
                  .IsRequired();

            entity.Property(e => e.FotoPerfilUrl)
                  .HasMaxLength(500);

            entity.HasIndex(e => e.Email).IsUnique();
            //entity.Property<DateTime>("FechaCreacion").IsRequired();
            //entity.Property<DateTime?>("FechaModificacion");
        }

        private static void ConfigureTicket(ModelBuilder builder)
        {
            var entity = builder.Entity<Ticket>();
            entity.ToTable("Tickets");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Numero).IsUnique();

            entity.Property(e => e.Asunto).IsRequired().HasMaxLength(250);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.Prioridad);

            entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(4000);

            entity.Property(e => e.Prioridad).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Estado).IsRequired().HasConversion<string>().HasMaxLength(50);

            entity.Property(e => e.ArchivoAdjuntoUrl).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion);


            entity.HasOne(e => e.Area)         
                  .WithMany()
                  .HasForeignKey(e => e.AreaId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne(e => e.TipoSolicitud) 
                  .WithMany()
                  .HasForeignKey(e => e.TipoSolicitudId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne(e => e.Solicitante)   
                  .WithMany()
                  .HasForeignKey(e => e.SolicitanteId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne(e => e.GestorAsignado) 
                  .WithMany()
                  .HasForeignKey(e => e.GestorAsignadoId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
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


        private static void ConfigureTicketEvent(ModelBuilder builder)
        {
            var entity = builder.Entity<TicketEvent>();
            entity.ToTable("TicketEvents");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.TipoEvento)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.Texto)
                  .HasMaxLength(1000); 

            entity.Property(e => e.EstadoAnterior)
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.EstadoNuevo)
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.Fecha)
                  .IsRequired();

            // Relación con Ticket (Borrado en cascada suele ser correcto para historial)
            entity.HasOne(e => e.Ticket)
                  .WithMany() // O WithMany(t => t.Events) si agregaste la colección en Ticket
                  .HasForeignKey(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();

            // Relación con Usuario
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict) // No borrar historial si se borra usuario
                  .IsRequired();
        }
        #endregion
    }


}