using Bonyan.Modularity.Abstractions;
using MCA.SharedKernel.Domain;

namespace MCA.Identity.Domain;

public class McaIdentityDomainModule : BonModule
{
    public McaIdentityDomainModule()
    {
        DependOn<McaSharedKernelDomainModule>();
    }
}