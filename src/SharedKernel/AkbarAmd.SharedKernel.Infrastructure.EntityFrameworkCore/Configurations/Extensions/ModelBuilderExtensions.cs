using System.Reflection;
using AkbarAmd.SharedKernel.Domain.Contracts;
using AkbarAmd.SharedKernel.Domain.Contracts.Audits;
using Microsoft.EntityFrameworkCore;

namespace AkbarAmd.SharedKernel.Infrastructure.Configurations.Extensions
{
    /// <summary>
    /// Extension methods for ModelBuilder to simplify entity configuration.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Applies all entity configurations from the specified assembly.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <param name="assembly">The assembly containing the configurations.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder ApplyConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            return modelBuilder;
        }


       
        
        /// <summary>
        /// Configures audit properties for all entities implementing ICreatableAudit.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder ConfigureAuditProperties(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(ICreatableAudit).IsAssignableFrom(clrType))
                {
                    ConfigureCreatableAudit(modelBuilder, clrType);
                }

                if (typeof(IModifiableAudit).IsAssignableFrom(clrType))
                {
                    ConfigureModifiableAudit(modelBuilder, clrType);
                }

                if (typeof(ISoftDeletableAudit).IsAssignableFrom(clrType))
                {
                    ConfigureSoftDeletableAudit(modelBuilder, clrType);
                }

                if (typeof(IConcurrentAudit).IsAssignableFrom(clrType))
                {
                    ConfigureConcurrentAudit(modelBuilder, clrType);
                }
            }

            return modelBuilder;
        }

        /// <summary>
        /// Configures aggregate root properties for all entities implementing IAggregateRoot.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder ConfigureAggregateRoots(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(IAggregateRoot).IsAssignableFrom(clrType))
                {
                    // Ignore domain events properties
                    modelBuilder.Entity(clrType).Ignore("DomainEvents");
                    modelBuilder.Entity(clrType).Ignore("HasPendingEvents");

                    // Configure version property if it implements IAggregateRoot<TId>
                    var genericInterface = clrType.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAggregateRoot<>));

                    if (genericInterface != null)
                    {
                        modelBuilder.Entity(clrType)
                            .Property("Version")
                            .IsRequired()
                            .IsConcurrencyToken()
                            .HasDefaultValue(0L);
                    }
                }
            }

            return modelBuilder;
        }

        /// <summary>
        /// Configures global query filters for soft deletable entities.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder ConfigureGlobalFilters(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(ISoftDeletableAudit).IsAssignableFrom(clrType))
                {
                    var method = typeof(ModelBuilderExtensions)
                        .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(clrType);

                    method.Invoke(null, new object[] { modelBuilder });
                }
            }

            return modelBuilder;
        }

        /// <summary>
        /// Configures decimal properties with appropriate precision and scale.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <param name="precision">The precision for decimal properties.</param>
        /// <param name="scale">The scale for decimal properties.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder ConfigureDecimalProperties(this ModelBuilder modelBuilder, int precision = 18, int scale = 2)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(precision);
                        property.SetScale(scale);
                    }
                }
            }

            return modelBuilder;
        }

        /// <summary>
        /// Configures string properties with maximum length.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <param name="maxLength">The maximum length for string properties.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder ConfigureStringProperties(this ModelBuilder modelBuilder, int maxLength = 255)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) && !property.GetMaxLength().HasValue)
                    {
                        property.SetMaxLength(maxLength);
                    }
                }
            }

            return modelBuilder;
        }

        #region Private Helper Methods

        private static void ConfigureCreatableAudit(ModelBuilder modelBuilder, Type entityType)
        {
            modelBuilder.Entity(entityType)
                .Property("CreatedAt")
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity(entityType)
                .Property("CreatedBy")
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            // Add indexes
            modelBuilder.Entity(entityType)
                .HasIndex("CreatedAt")
                .HasDatabaseName($"IX_{entityType.Name}_CreatedAt");

            modelBuilder.Entity(entityType)
                .HasIndex("CreatedBy")
                .HasDatabaseName($"IX_{entityType.Name}_CreatedBy");
        }

        private static void ConfigureModifiableAudit(ModelBuilder modelBuilder, Type entityType)
        {
            modelBuilder.Entity(entityType)
                .Property("ModifiedAt")
                .HasColumnType("datetime2");

            modelBuilder.Entity(entityType)
                .Property("ModifiedBy")
                .HasMaxLength(100)
                .IsUnicode(false);

            // Add indexes with filters
            modelBuilder.Entity(entityType)
                .HasIndex("ModifiedAt")
                .HasDatabaseName($"IX_{entityType.Name}_ModifiedAt")
                .HasFilter("[ModifiedAt] IS NOT NULL");

            modelBuilder.Entity(entityType)
                .HasIndex("ModifiedBy")
                .HasDatabaseName($"IX_{entityType.Name}_ModifiedBy")
                .HasFilter("[ModifiedBy] IS NOT NULL");
        }

        private static void ConfigureSoftDeletableAudit(ModelBuilder modelBuilder, Type entityType)
        {
            modelBuilder.Entity(entityType)
                .Property("DeletedAt")
                .HasColumnType("datetime2");

            modelBuilder.Entity(entityType)
                .Property("DeletedBy")
                .HasMaxLength(100)
                .IsUnicode(false);

            modelBuilder.Entity(entityType)
                .Property("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);

            // Add indexes
            modelBuilder.Entity(entityType)
                .HasIndex("IsDeleted")
                .HasDatabaseName($"IX_{entityType.Name}_IsDeleted");

            modelBuilder.Entity(entityType)
                .HasIndex("DeletedAt")
                .HasDatabaseName($"IX_{entityType.Name}_DeletedAt")
                .HasFilter("[DeletedAt] IS NOT NULL");

            modelBuilder.Entity(entityType)
                .HasIndex("DeletedBy")
                .HasDatabaseName($"IX_{entityType.Name}_DeletedBy")
                .HasFilter("[DeletedBy] IS NOT NULL");
        }

        private static void ConfigureConcurrentAudit(ModelBuilder modelBuilder, Type entityType)
        {
            modelBuilder.Entity(entityType)
                .Property("ConcurrencyStamp")
                .IsRequired()
                .HasMaxLength(36)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity(entityType)
                .HasIndex("ConcurrencyStamp")
                .HasDatabaseName($"IX_{entityType.Name}_ConcurrencyStamp");
        }

        private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
            where TEntity : class, ISoftDeletableAudit
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        #endregion
    }
}