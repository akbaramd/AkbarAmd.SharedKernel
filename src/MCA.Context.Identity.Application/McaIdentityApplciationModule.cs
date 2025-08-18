using Bonyan.Modularity.Abstractions;
using MCA.Context.Identity.Domain;
using MCA.SharedKernel.Application;

namespace MCA.Context.Identity.Application;

public class McaIdentityApplciationModule : BonModule
{
    public McaIdentityApplciationModule()
    {
        DependOn<McaIdentityDomainModule>();
        DependOn<McaSharedKernelApplicationModule>();
    }
}