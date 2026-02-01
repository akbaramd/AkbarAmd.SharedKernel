using Bonyan.Modularity;
using Bonyan.Modularity.Abstractions;

namespace AkbarAmd.SharedKernel.Domain;

/// <summary>
/// 
/// </summary>
public class AkbarAmdSharedKernelDomainModule : BonModule
{
    public override Task OnPostConfigureAsync(BonConfigurationContext context)
    {
        return base.OnPostConfigureAsync(context);
    }
}