namespace CleanArchitecture.Domain.SharedKernel.Outbox;

public interface IOutboxStore
{
    Task SaveMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task SaveMessagesAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default);
    Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkMessageAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkMessageAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
    Task DeleteProcessedMessagesOlderThanAsync(DateTime date, CancellationToken cancellationToken = default);
} 