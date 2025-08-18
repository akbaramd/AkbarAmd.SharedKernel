using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain.User.Enumerations;

/// <summary>
/// Represents the type of a token in the identity system.
/// </summary>
public sealed class TokenType : Enumeration
{
    /// <summary>
    /// Access token for API authentication.
    /// </summary>
    public static readonly TokenType AccessToken = new(1, nameof(AccessToken), "Access token for API authentication.");

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public static readonly TokenType RefreshToken = new(2, nameof(RefreshToken), "Refresh token for obtaining new access tokens.");

    /// <summary>
    /// Email confirmation token.
    /// </summary>
    public static readonly TokenType EmailConfirmation = new(3, nameof(EmailConfirmation), "Email confirmation token.");

    /// <summary>
    /// Password reset token.
    /// </summary>
    public static readonly TokenType PasswordReset = new(4, nameof(PasswordReset), "Password reset token.");

    /// <summary>
    /// Phone number confirmation token.
    /// </summary>
    public static readonly TokenType PhoneConfirmation = new(5, nameof(PhoneConfirmation), "Phone number confirmation token.");

    /// <summary>
    /// Two-factor authentication token.
    /// </summary>
    public static readonly TokenType TwoFactor = new(6, nameof(TwoFactor), "Two-factor authentication token.");

    /// <summary>
    /// Change email token.
    /// </summary>
    public static readonly TokenType ChangeEmail = new(7, nameof(ChangeEmail), "Change email token.");

    /// <summary>
    /// Change phone token.
    /// </summary>
    public static readonly TokenType ChangePhone = new(8, nameof(ChangePhone), "Change phone token.");

    /// <summary>
    /// Account lockout token.
    /// </summary>
    public static readonly TokenType AccountLockout = new(9, nameof(AccountLockout), "Account lockout token.");

    /// <summary>
    /// Custom token for specific purposes.
    /// </summary>
    public static readonly TokenType Custom = new(10, nameof(Custom), "Custom token for specific purposes.");

    /// <summary>
    /// Initializes a new instance of the TokenType enumeration.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    private TokenType(int id, string name, string? description = null) : base(id, name, description)
    {
    }
} 