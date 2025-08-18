namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Contract for aggregate roots that support temporal operations.
/// </summary>
public interface ITemporalAggregateRoot
{
    /// <summary>
    /// Gets the effective date when the aggregate becomes active.
    /// </summary>
    DateTime? EffectiveFrom { get; }

    /// <summary>
    /// Gets the effective date when the aggregate expires.
    /// </summary>
    DateTime? EffectiveTo { get; }

    /// <summary>
    /// Indicates whether the aggregate is currently effective.
    /// </summary>
    bool IsEffective { get; }

    /// <summary>
    /// Sets the effective period for the aggregate.
    /// </summary>
    /// <param name="effectiveFrom">Start date of effectiveness.</param>
    /// <param name="effectiveTo">End date of effectiveness.</param>
    void SetEffectivePeriod(DateTime effectiveFrom, DateTime? effectiveTo = null);

    /// <summary>
    /// Activates the aggregate from a specific date.
    /// </summary>
    /// <param name="effectiveFrom">Date from which the aggregate becomes active.</param>
    void Activate(DateTime effectiveFrom);

    /// <summary>
    /// Deactivates the aggregate from a specific date.
    /// </summary>
    /// <param name="effectiveTo">Date from which the aggregate becomes inactive.</param>
    void Deactivate(DateTime effectiveTo);
}