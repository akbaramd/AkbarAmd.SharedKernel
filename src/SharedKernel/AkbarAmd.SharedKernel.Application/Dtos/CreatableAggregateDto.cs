using AkbarAmd.SharedKernel.Domain.Contracts.Audits;

namespace AkbarAmd.SharedKernel.Application.Dtos;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class CreatableAggregateDto<TKey> : AggregateDto<TKey>,ICreatableAudit
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}