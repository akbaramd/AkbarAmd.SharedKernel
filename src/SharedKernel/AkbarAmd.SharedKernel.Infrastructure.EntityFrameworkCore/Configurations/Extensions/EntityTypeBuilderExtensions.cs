/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Infrastructure - EntityTypeBuilder Extensions
 * Extension methods for simplified Entity Framework Core entity configuration
 * Year: 2025
 */

using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts;
using AkbarAmd.SharedKernel.Domain.Contracts.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkbarAmd.SharedKernel.Infrastructure.Configurations.Extensions
{
    /// <summary>
    /// Extension methods for EntityTypeBuilder to simplify entity configuration.
    /// </summary>
    public static class EntityTypeBuilderExtensions
    {
        /// <summary>
        /// Configures creation audit properties for entities implementing ICreatableAudit.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing ICreatableAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureCreationAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, ICreatableAudit
        {
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            // Add indexes for common queries
            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedAt");

            builder.HasIndex(e => e.CreatedBy)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedBy");

            return builder;
        }

        /// <summary>
        /// Configures modification audit properties for entities implementing IModifiableAudit.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IModifiableAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureModificationAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IModifiableAudit
        {
            builder.Property(e => e.LastModifiedAt)
                .HasColumnType("datetime2");

            builder.Property(e => e.LastModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            // Add indexes for common queries with filters
            builder.HasIndex(e => e.LastModifiedAt)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_LastModifiedAt")
                .HasFilter("[LastModifiedAt] IS NOT NULL");

            builder.HasIndex(e => e.LastModifiedBy)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_LastModifiedBy")
                .HasFilter("[LastModifiedBy] IS NOT NULL");

            return builder;
        }

        /// <summary>
        /// Configures soft delete audit properties for entities implementing ISoftDeletableAudit.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing ISoftDeletableAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureSoftDeleteAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, ISoftDeletableAudit
        {
            builder.Property(e => e.DeletedAt)
                .HasColumnType("datetime2");

            builder.Property(e => e.DeletedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Global query filter to exclude soft deleted entities
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Add indexes for soft delete queries
            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_IsDeleted");

            builder.HasIndex(e => e.DeletedAt)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_DeletedAt")
                .HasFilter("[DeletedAt] IS NOT NULL");

            builder.HasIndex(e => e.DeletedBy)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_DeletedBy")
                .HasFilter("[DeletedBy] IS NOT NULL");

            return builder;
        }

        /// <summary>
        /// Configures concurrent audit properties for entities implementing IConcurrentAudit.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IConcurrentAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureConcurrentAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IConcurrentAudit
        {
            builder.Property(e => e.RowVersion)
                .IsRequired()
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();

            return builder;
        }

        /// <summary>
        /// Configures full audit trail for entities implementing IConcurrentAudit (all audit properties).
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IConcurrentAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureFullAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IConcurrentAudit,ICreatableAudit,ISoftDeletableAudit,IModifiableAudit
        {
            return builder
                .ConfigureCreationAudit()
                .ConfigureModificationAudit()
                .ConfigureSoftDeleteAudit()
                .ConfigureConcurrentAudit();
        }

        /// <summary>
        /// Configures a string property with common settings.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <param name="propertyExpression">Expression representing the property.</param>
        /// <param name="maxLength">Maximum length for the string property.</param>
        /// <param name="isRequired">Whether the property is required.</param>
        /// <param name="isUnicode">Whether the property supports Unicode characters.</param>
        /// <returns>The PropertyBuilder for further configuration.</returns>
        public static PropertyBuilder<string> ConfigureStringProperty<TEntity>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TEntity, string>> propertyExpression,
            int maxLength = 255,
            bool isRequired = false,
            bool isUnicode = true)
            where TEntity : class
        {
            var propertyBuilder = builder.Property(propertyExpression);

            propertyBuilder.HasMaxLength(maxLength);

            if (isRequired)
                propertyBuilder.IsRequired();

            if (!isUnicode)
                propertyBuilder.IsUnicode(false);

            return propertyBuilder;
        }

        /// <summary>
        /// Configures a decimal property with precision and scale.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <param name="propertyExpression">Expression representing the property.</param>
        /// <param name="precision">The precision for the decimal property.</param>
        /// <param name="scale">The scale for the decimal property.</param>
        /// <param name="isRequired">Whether the property is required.</param>
        /// <returns>The PropertyBuilder for further configuration.</returns>
        public static PropertyBuilder<decimal> ConfigureDecimalProperty<TEntity>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TEntity, decimal>> propertyExpression,
            int precision = 18,
            int scale = 2,
            bool isRequired = false)
            where TEntity : class
        {
            var propertyBuilder = builder.Property(propertyExpression)
                .HasPrecision(precision, scale);

            if (isRequired)
                propertyBuilder.IsRequired();

            return propertyBuilder;
        }

        /// <summary>
        /// Configures a nullable decimal property with precision and scale.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <param name="propertyExpression">Expression representing the property.</param>
        /// <param name="precision">The precision for the decimal property.</param>
        /// <param name="scale">The scale for the decimal property.</param>
        /// <returns>The PropertyBuilder for further configuration.</returns>
        public static PropertyBuilder<decimal?> ConfigureDecimalProperty<TEntity>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TEntity, decimal?>> propertyExpression,
            int precision = 18,
            int scale = 2)
            where TEntity : class
        {
            return builder.Property(propertyExpression)
                .HasPrecision(precision, scale);
        }

        /// <summary>
        /// Configures a datetime property with appropriate column type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <param name="propertyExpression">Expression representing the property.</param>
        /// <param name="columnType">The column type (default: datetime2).</param>
        /// <param name="isRequired">Whether the property is required.</param>
        /// <returns>The PropertyBuilder for further configuration.</returns>
        public static PropertyBuilder<DateTime> ConfigureDateTimeProperty<TEntity>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TEntity, DateTime>> propertyExpression,
            string columnType = "datetime2",
            bool isRequired = false)
            where TEntity : class
        {
            var propertyBuilder = builder.Property(propertyExpression)
                .HasColumnType(columnType);

            if (isRequired)
                propertyBuilder.IsRequired();

            return propertyBuilder;
        }

        /// <summary>
        /// Configures a nullable datetime property with appropriate column type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <param name="propertyExpression">Expression representing the property.</param>
        /// <param name="columnType">The column type (default: datetime2).</param>
        /// <returns>The PropertyBuilder for further configuration.</returns>
        public static PropertyBuilder<DateTime?> ConfigureDateTimeProperty<TEntity>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TEntity, DateTime?>> propertyExpression,
            string columnType = "datetime2")
            where TEntity : class
        {
            return builder.Property(propertyExpression)
                .HasColumnType(columnType);
        }

        /// <summary>
        /// Creates a composite index on multiple properties.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="propertyNames">The names of the properties to include in the index.</param>
        /// <param name="isUnique">Whether the index should be unique.</param>
        /// <returns>The IndexBuilder for further configuration.</returns>
        public static IndexBuilder ConfigureCompositeIndex<TEntity>(
            this EntityTypeBuilder<TEntity> builder,
            string indexName,
            string[] propertyNames,
            bool isUnique = false)
            where TEntity : class
        {
            var indexBuilder = builder.HasIndex(propertyNames)
                .HasDatabaseName(indexName);

            if (isUnique)
                indexBuilder.IsUnique();

            return indexBuilder;
        }

        #region Combined Entity and Audit Configuration Extensions

        /// <summary>
        /// Configures an entity that implements IEntity and ICreatableAudit.
        /// Sets up entity configuration with creation audit properties.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity and ICreatableAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureCreatableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, ICreatableAudit
        {
            return builder.ConfigureCreationAudit();
        }

        /// <summary>
        /// Configures an entity that implements IEntity and IModifiableAudit.
        /// Sets up entity configuration with creation and modification audit properties.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity and IModifiableAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureModifiableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IModifiableAudit
        {
            return builder
                .ConfigureModificationAudit();
        }

        /// <summary>
        /// Configures an entity that implements IEntity and ISoftDeletableAudit.
        /// Sets up entity configuration with creation, modification, and soft delete audit properties.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity and ISoftDeletableAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureSoftDeletableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, ISoftDeletableAudit
        {
            return builder
                .ConfigureSoftDeleteAudit();
        }

        /// <summary>
        /// Configures an entity that implements IEntity and IConcurrentAudit.
        /// Sets up entity configuration with full audit trail (creation, modification, soft delete, and concurrency).
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity and IConcurrentAudit.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureFullAuditableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IConcurrentAudit,ICreatableAudit,IModifiableAudit,ISoftDeletableAudit
        {
            return builder
                .ConfigureCreationAudit()
                .ConfigureModificationAudit()
                .ConfigureSoftDeleteAudit()
                .ConfigureConcurrentAudit();
        }

        /// <summary>
        /// Configures an entity with typed Id that implements IEntity&lt;TId&gt; and ICreatableAudit.
        /// Sets up entity configuration with creation audit properties and typed primary key.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity&lt;TId&gt; and ICreatableAudit.</typeparam>
        /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureCreatableEntity<TEntity, TId>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IEntity<TId>, ICreatableAudit
            where TId : IEquatable<TId>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired();

            return builder.ConfigureCreationAudit();
        }

        /// <summary>
        /// Configures an entity with typed Id that implements IEntity&lt;TId&gt; and IModifiableAudit.
        /// Sets up entity configuration with creation and modification audit properties and typed primary key.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity&lt;TId&gt; and IModifiableAudit.</typeparam>
        /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureModifiableEntity<TEntity, TId>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IEntity<TId>, IModifiableAudit
            where TId : IEquatable<TId>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired();

            return builder
                .ConfigureModificationAudit();
        }

        /// <summary>
        /// Configures an entity with typed Id that implements IEntity&lt;TId&gt; and ISoftDeletableAudit.
        /// Sets up entity configuration with creation, modification, and soft delete audit properties and typed primary key.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity&lt;TId&gt; and ISoftDeletableAudit.</typeparam>
        /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureSoftDeletableEntity<TEntity, TId>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IEntity<TId>, ISoftDeletableAudit
            where TId : IEquatable<TId>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired();

            return builder
                .ConfigureSoftDeleteAudit();
        }

        /// <summary>
        /// Configures an entity with typed Id that implements IEntity&lt;TId&gt; and IConcurrentAudit.
        /// Sets up entity configuration with full audit trail (creation, modification, soft delete, and concurrency) and typed primary key.
        /// </summary>
        /// <typeparam name="TEntity">The entity type implementing IEntity&lt;TId&gt; and IConcurrentAudit.</typeparam>
        /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        /// <returns>The EntityTypeBuilder instance for method chaining.</returns>
        public static EntityTypeBuilder<TEntity> ConfigureFullAuditableEntity<TEntity, TId>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IEntity<TId>, IConcurrentAudit,ISoftDeletableAudit,ICreatableAudit,IModifiableAudit
            where TId : IEquatable<TId>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired();

            return builder
                .ConfigureCreationAudit()
                .ConfigureModificationAudit()
                .ConfigureSoftDeleteAudit()
                .ConfigureConcurrentAudit();
        }

        #endregion
    }
}