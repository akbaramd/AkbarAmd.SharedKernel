/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - OutboxMessageStatus Smart Enumeration
 * Represents the various statuses of an OutboxMessage using rich enumeration pattern.
 * Year: 2025
 */

using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.SharedKernel.Outbox
{
    /// <summary>
    /// Represents the status of an Outbox message using rich enumeration pattern.
    /// Enables strong typing, descriptions, and easy comparison.
    /// </summary>
    public sealed class OutboxMessageStatus : Enumeration
    {
        /// <summary>
        /// The message is pending to be processed.
        /// </summary>
        public static readonly OutboxMessageStatus Pending = new(1, nameof(Pending), "The message is pending to be processed.");

        /// <summary>
        /// The message is currently being processed.
        /// </summary>
        public static readonly OutboxMessageStatus Processing = new(2, nameof(Processing), "The message is being processed.");

        /// <summary>
        /// The message has been processed successfully.
        /// </summary>
        public static readonly OutboxMessageStatus Processed = new(3, nameof(Processed), "The message has been processed successfully.");

        /// <summary>
        /// The message processing has failed.
        /// </summary>
        public static readonly OutboxMessageStatus Failed = new(4, nameof(Failed), "The message processing failed.");

        /// <summary>
        /// The message is currently being retried after a failure.
        /// </summary>
        public static readonly OutboxMessageStatus Retrying = new(5, nameof(Retrying), "The message is being retried after a failure.");

        /// <summary>
        /// Private constructor to restrict instantiation.
        /// </summary>
        private OutboxMessageStatus(int id, string name, string? description = null) 
            : base(id, name, description)
        {
        }

        /// <summary>
        /// Retrieves all possible statuses.
        /// </summary>
        public static IReadOnlyList<OutboxMessageStatus> List() => GetAll<OutboxMessageStatus>();
    }
}