using AkbarAmd.SharedKernel.Domain.Contracts.Audits;

namespace AkbarAmd.SharedKernel.Application.Dtos;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class ModifiableAggregateDto<TKey> : CreatableAggregateDto<TKey>,IModifiableAudit
{
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}