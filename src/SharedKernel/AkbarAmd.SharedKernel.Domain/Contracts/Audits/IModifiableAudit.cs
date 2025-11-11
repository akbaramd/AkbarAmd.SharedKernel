/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Modifiable Aggregate Root Contract
 * Tracking interface for entities that require modification audit fields
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Domain.Contracts.Audits
{
    /// <summary>
    /// Contract for entities that require modification audit tracking
    /// </summary>
    public interface IModifiableAudit
    {
        /// <summary>
        /// When the entity was last modified (nullable)
        /// </summary>
        DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// Who last modified the entity (nullable)
        /// </summary>
        string? LastModifiedBy { get; set; }
    }
}