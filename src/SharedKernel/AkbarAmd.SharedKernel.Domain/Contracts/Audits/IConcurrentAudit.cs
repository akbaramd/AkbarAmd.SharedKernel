/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Concurrent Aggregate Root Contract
 * Tracking interface for entities that require concurrency control
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Domain.Contracts.Audits
{
    /// <summary>
    /// Contract for entities that require concurrency audit control
    /// </summary>
    public interface IConcurrentAudit
    {
        /// <summary>
        /// Row version for optimistic concurrency control (required)
        /// </summary>
        byte[] RowVersion { get; set; }
    }
}