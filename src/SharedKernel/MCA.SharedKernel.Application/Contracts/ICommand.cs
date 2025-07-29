using MediatR;

namespace MCA.SharedKernel.Application.Contracts;

public class ICommand : IRequest
{
    
}

public class ICommand<TResult> : IRequest<TResult>
{
    
}