using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;
using CleanArchitecture.Domain.Contexts.Identity.Interfaces;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Rules;

public class UniqueEmailRule(Email email, IUserRepository userRepository)
{
    public bool IsSatisfied()
    {
        return !userRepository.ExistsAsync(x=>x.Email == email && x.IsActive).GetAwaiter().GetResult();
    }

    public string Message => $"Email {email.Value} is already in use";
} 