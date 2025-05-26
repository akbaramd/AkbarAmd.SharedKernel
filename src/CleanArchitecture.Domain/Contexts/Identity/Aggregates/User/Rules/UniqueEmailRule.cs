using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;
using CleanArchitecture.Domain.Contexts.Identity.Interfaces;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Rules;

public class UniqueEmailRule
{
    private readonly Email _email;
    private readonly IUserRepository _userRepository;

    public UniqueEmailRule(Email email, IUserRepository userRepository)
    {
        _email = email;
        _userRepository = userRepository;
    }

    public bool IsSatisfied()
    {
        return !_userRepository.ExistsByEmail(_email);
    }

    public string Message => $"Email {_email.Value} is already in use";
} 