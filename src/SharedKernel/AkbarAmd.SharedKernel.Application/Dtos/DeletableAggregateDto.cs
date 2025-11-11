using AkbarAmd.SharedKernel.Domain.Contracts.Audits;

namespace AkbarAmd.SharedKernel.Application.Dtos;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class DeletableAggregateDto<TKey> : ModifiableAggregateDto<TKey>,ISoftDeletableAudit
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}