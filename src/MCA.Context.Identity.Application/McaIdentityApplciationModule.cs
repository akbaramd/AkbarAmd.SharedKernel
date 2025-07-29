using Bonyan.Modularity.Abstractions;
using MCA.Identity.Domain;
using MCA.SharedKernel.Application;
using MCA.SharedKernel.Domain;

namespace CleanArchitecture.Application;

public class McaIdentityApplciationModule : BonModule
{
    public McaIdentityApplciationModule()
    {
        DependOn<McaIdentityDomainModule>();
        DependOn<McaSharedKernelApplicationModule>();
    }
}