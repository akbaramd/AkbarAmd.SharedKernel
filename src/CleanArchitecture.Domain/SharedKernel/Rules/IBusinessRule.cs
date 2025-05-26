namespace CleanArchitecture.Domain.SharedKernel.Rules;

public interface IBusinessRule
{
    bool IsSatisfied();
    string Message { get; }
} 