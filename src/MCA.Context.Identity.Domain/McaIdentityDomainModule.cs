using Bonyan.Modularity;
using Bonyan.Modularity.Abstractions;
using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain;

public class McaIdentityDomainModule : BonModule
{
    public McaIdentityDomainModule()
    {
        DependOn<McaSharedKernelDomainModule>();
    }

    public override Task OnConfigureAsync(BonConfigurationContext context)
    {
        return base.OnConfigureAsync(context);
    }
}