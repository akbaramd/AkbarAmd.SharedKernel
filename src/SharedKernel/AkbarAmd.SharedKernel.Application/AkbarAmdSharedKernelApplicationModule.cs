using AkbarAmd.SharedKernel.Domain;
using Bonyan.Modularity;
using Bonyan.Modularity.Abstractions;

namespace AkbarAmd.SharedKernel.Application;

public class AkbarAmdSharedKernelApplicationModule : BonModule
{
    public AkbarAmdSharedKernelApplicationModule()
    {
        DependOn<AkbarAmdSharedKernelDomainModule>();
    }


    public override Task OnPostConfigureAsync(BonConfigurationContext context)
    {
        return base.OnPostConfigureAsync(context);
    }
}