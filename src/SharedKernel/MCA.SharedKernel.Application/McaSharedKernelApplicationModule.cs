using Bonyan.Modularity.Abstractions;
using MCA.SharedKernel.Domain;

namespace MCA.SharedKernel.Application;

public class McaSharedKernelApplicationModule : BonModule
{
    public McaSharedKernelApplicationModule()
    {
        DependOn<McaSharedKernelDomainModule>();
    }
}