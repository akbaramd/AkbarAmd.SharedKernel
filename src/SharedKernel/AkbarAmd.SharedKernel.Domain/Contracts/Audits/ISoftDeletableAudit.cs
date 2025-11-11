/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Soft Deletable Aggregate Root Contract
 * Tracking interface for entities that require soft delete audit fields
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Domain.Contracts.Audits
{
    /// <summary>
    /// Contract for entities that require soft delete audit tracking
    /// </summary>
    public interface ISoftDeletableAudit
    {
        /// <summary>
        /// Whether the entity is soft deleted (required)
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// When the entity was deleted (nullable)
        /// </summary>
        DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Who deleted the entity (nullable)
        /// </summary>
        string? DeletedBy { get; set; }
    }
}