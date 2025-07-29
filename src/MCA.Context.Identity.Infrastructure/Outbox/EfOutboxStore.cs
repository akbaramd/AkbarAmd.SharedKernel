/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Outbox Store EF Core Implementation
 * Implements IOutboxStore to persist and manage outbox messages with EF Core.
 * Year: 2025
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MCA.SharedKernel.Domain.Contracts;
using MCA.SharedKernel.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Outbox
{
    public class EfOutboxStore<TDbContext> : IOutboxStore
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly DbSet<OutboxMessage> _dbSet;

        public EfOutboxStore(TDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<OutboxMessage>();
        }

        public async Task SaveMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            await _dbSet.AddAsync(message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveMessagesAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            await _dbSet.AddRangeAsync(messages, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));

            return await _dbSet
                .Where(m => m.Status == OutboxMessageStatus.Pending || m.Status == OutboxMessageStatus.Retrying)
                .OrderBy(m => m.OccurredOn)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task MarkMessageAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            var message = await _dbSet.FindAsync(new object[] { messageId }, cancellationToken);
            if (message == null)
                throw new InvalidOperationException($"Outbox message with ID '{messageId}' not found.");

            message.MarkAsProcessed();
            _dbSet.Update(message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkMessageAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
        {
            var message = await _dbSet.FindAsync(new object[] { messageId }, cancellationToken);
            if (message == null)
                throw new InvalidOperationException($"Outbox message with ID '{messageId}' not found.");

            message.MarkAsFailed(error);
            _dbSet.Update(message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteProcessedMessagesOlderThanAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var oldMessages = await _dbSet
                .Where(m => m.Status == OutboxMessageStatus.Processed && m.ProcessedOn < date)
                .ToListAsync(cancellationToken);

            if (oldMessages.Count == 0)
                return;

            _dbSet.RemoveRange(oldMessages);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
