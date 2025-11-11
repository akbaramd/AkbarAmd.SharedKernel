/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Creatable Aggregate Root Contract
 * Tracking interface for entities that require creation audit fields
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Domain.Contracts.Audits
{
    /// <summary>
    /// Contract for entities that require creation audit tracking
    /// </summary>
    public interface ICreatableAudit
    {
        /// <summary>
        /// When the entity was created (required)
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Who created the entity (required)
        /// </summary>
        string CreatedBy { get; set; }
    }
}