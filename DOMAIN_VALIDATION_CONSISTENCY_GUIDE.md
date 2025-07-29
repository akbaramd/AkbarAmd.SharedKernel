# Domain Validation Consistency Guide - User Domain

## Overview

This guide explains the consistent validation structure implemented across all files in the User domain. All validation rules, constants, and error messages are centralized and consistent, following Domain-Driven Design (DDD) principles.

## 🏗️ **Architecture Overview**

### **Centralized Constants**
- **`ValidationConstants.cs`**: Single source of truth for all validation rules
- **Consistent Values**: All files use the same constants for validation
- **Maintainable**: Changes to validation rules only require updates in one place

### **Business Rules Layer**
- **`EmailFormatRule.cs`**: Email validation using constants
- **`PasswordStrengthRule.cs`**: Password validation using constants
- **`NameFormatRule.cs`**: Name validation using constants

### **Policy Layer**
- **`PasswordPolicy.cs`**: High-level password validation using same constants
- **`UserStatusPolicy.cs`**: User status transitions using constants

### **Value Objects**
- **`Email.cs`**: Uses `EmailFormatRule` and constants
- **`Password.cs`**: Uses `PasswordStrengthRule` and constants

### **Entity Layer**
- **`User.cs`**: Uses business rules and constants for validation

## 📋 **Validation Constants Structure**

### **Email Validation Constants**
```csharp
public const int EmailMaxLength = 254; // RFC 5321
public const int EmailMinLength = 5; // a@b.c
public const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
public const string EmailFormatErrorMessage = "Email must be in a valid format (e.g., user@domain.com)";
public const string EmailTooShortErrorMessage = "Email must be at least {0} characters long";
public const string EmailTooLongErrorMessage = "Email cannot exceed {0} characters";
public const string EmailEmptyErrorMessage = "Email cannot be empty or whitespace";
```

### **Password Validation Constants**
```csharp
public const int PasswordMinLength = 8;
public const int PasswordMaxLength = 128;
public const int PasswordMinUppercase = 1;
public const int PasswordMinLowercase = 1;
public const int PasswordMinDigits = 1;
public const int PasswordMinSpecialChars = 1;
public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
// ... error messages
```

### **Name Validation Constants**
```csharp
public const int FirstNameMinLength = 2;
public const int FirstNameMaxLength = 50;
public const int LastNameMinLength = 2;
public const int LastNameMaxLength = 50;
public const string NamePattern = @"^[a-zA-Z\s\-']+$";
// ... error messages
```

### **User Status Constants**
```csharp
public const string UserNullErrorMessage = "User cannot be null";
public const string UserAlreadyDeletedErrorMessage = "User is already deleted";
public const string UserAlreadyActiveErrorMessage = "User is already active";
public const string UserAlreadySuspendedErrorMessage = "User is already suspended";
// ... more status-related constants
```

## 🔧 **Business Rules Implementation**

### **EmailFormatRule**
```csharp
public class EmailFormatRule : IBusinessRule
{
    private readonly string _email;
    private static readonly Regex EmailRegex = new(ValidationConstants.EmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool IsSatisfied()
    {
        if (string.IsNullOrWhiteSpace(_email))
            return false;

        if (_email.Length < ValidationConstants.EmailMinLength)
            return false;

        if (_email.Length > ValidationConstants.EmailMaxLength)
            return false;

        return EmailRegex.IsMatch(_email);
    }

    public string Message
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_email))
                return ValidationConstants.EmailEmptyErrorMessage;

            if (_email.Length < ValidationConstants.EmailMinLength)
                return string.Format(ValidationConstants.EmailTooShortErrorMessage, ValidationConstants.EmailMinLength);

            if (_email.Length > ValidationConstants.EmailMaxLength)
                return string.Format(ValidationConstants.EmailTooLongErrorMessage, ValidationConstants.EmailMaxLength);

            return ValidationConstants.EmailFormatErrorMessage;
        }
    }
}
```

### **PasswordStrengthRule**
```csharp
public class PasswordStrengthRule : IBusinessRule
{
    private readonly string _password;

    public bool IsSatisfied()
    {
        if (string.IsNullOrWhiteSpace(_password))
            return false;

        if (_password.Length < ValidationConstants.PasswordMinLength)
            return false;

        if (_password.Length > ValidationConstants.PasswordMaxLength)
            return false;

        // Check for required character types using constants
        if (!HasRequiredUppercase()) return false;
        if (!HasRequiredLowercase()) return false;
        if (!HasRequiredDigits()) return false;
        if (!HasRequiredSpecialCharacters()) return false;

        return true;
    }

    private bool HasRequiredUppercase()
    {
        var uppercaseCount = _password.Count(char.IsUpper);
        return uppercaseCount >= ValidationConstants.PasswordMinUppercase;
    }

    // ... other validation methods using constants
}
```

### **NameFormatRule**
```csharp
public class NameFormatRule : IBusinessRule
{
    private readonly string _name;
    private readonly string _fieldName;
    private readonly int _minLength;
    private readonly int _maxLength;
    private static readonly Regex NameRegex = new(ValidationConstants.NamePattern, RegexOptions.Compiled);

    public static NameFormatRule ForFirstName(string firstName)
    {
        return new NameFormatRule(
            firstName, 
            "First name", 
            ValidationConstants.FirstNameMinLength, 
            ValidationConstants.FirstNameMaxLength);
    }

    public static NameFormatRule ForLastName(string lastName)
    {
        return new NameFormatRule(
            lastName, 
            "Last name", 
            ValidationConstants.LastNameMinLength, 
            ValidationConstants.LastNameMaxLength);
    }

    // ... validation logic using constants
}
```

## 🎯 **Policy Layer Implementation**

### **PasswordPolicy**
```csharp
public static class PasswordPolicy
{
    public static PasswordValidationResult Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordValidationResult.Failure(ValidationConstants.PasswordEmptyErrorMessage);

        if (password.Length < ValidationConstants.PasswordMinLength)
            return PasswordValidationResult.Failure(
                string.Format(ValidationConstants.PasswordTooShortErrorMessage, ValidationConstants.PasswordMinLength));

        // ... all validation using constants
    }

    public static int GetStrengthScore(string password)
    {
        // ... scoring logic using constants
        if (password.Length >= ValidationConstants.PasswordMinLength)
            score += Math.Min(25, password.Length - ValidationConstants.PasswordMinLength + 1);
    }
}
```

### **UserStatusPolicy**
```csharp
public static class UserStatusPolicy
{
    public static string GetDeactivationErrorMessage(UserEntity user)
    {
        if (user == null)
            return ValidationConstants.UserNullErrorMessage;

        if (user.Status == UserStatus.Deleted)
            return ValidationConstants.CannotDeactivateDeletedUserErrorMessage;

        if (user.Status != UserStatus.Active)
            return string.Format(ValidationConstants.CannotDeactivateUserWithStatusErrorMessage, user.Status.Name);

        return string.Empty;
    }

    // ... all error messages using constants
}
```

## 💎 **Value Objects Implementation**

### **Email Value Object**
```csharp
public sealed class Email : ValueObject
{
    public static Email Create(string email)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        var trimmed = email.Trim();
        var instance = new Email(trimmed.ToLowerInvariant());
        
        // Validate using business rule with constants
        ValueObject.CheckRule(new EmailFormatRule(instance.Value));
        
        return instance;
    }

    public static bool IsValidFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var rule = new EmailFormatRule(email);
        return rule.IsSatisfied();
    }

    public static string? GetValidationErrorMessage(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ValidationConstants.EmailEmptyErrorMessage;

        var rule = new EmailFormatRule(email);
        return rule.IsSatisfied() ? null : rule.Message;
    }
}
```

### **Password Value Object**
```csharp
public sealed class Password : ValueObject
{
    public static Password Create(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        var trimmed = password.Trim();
        
        // Validate password strength before hashing using constants
        ValueObject.CheckRule(new PasswordStrengthRule(trimmed));
        
        var hashedPassword = HashPassword(trimmed);
        return new Password(hashedPassword);
    }

    public static bool IsValidStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var rule = new PasswordStrengthRule(password);
        return rule.IsSatisfied();
    }

    public static string? GetValidationErrorMessage(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return ValidationConstants.PasswordEmptyErrorMessage;

        var rule = new PasswordStrengthRule(password);
        return rule.IsSatisfied() ? null : rule.Message;
    }
}
```

## 🏗️ **Entity Implementation**

### **User Entity**
```csharp
public sealed class UserEntity : AggregateRoot<Guid>
{
    private UserEntity(Guid id, Email email, Password password, string firstName, string lastName, string createdBy = "system")
        : base(id, createdBy)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        
        // Validate names using business rules with constants
        CheckRule(NameFormatRule.ForFirstName(firstName));
        CheckRule(NameFormatRule.ForLastName(lastName));
        
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Active;

        RaiseEvent(new UserCreatedEvent(Id, Email.Value), _ => { });
    }

    public void UpdateName(string firstName, string lastName)
    {
        // Validate names using business rules with constants
        CheckRule(NameFormatRule.ForFirstName(firstName));
        CheckRule(NameFormatRule.ForLastName(lastName));

        // ... update logic
    }

    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(ValidationConstants.SuspensionReasonEmptyErrorMessage, nameof(reason));
        
        if (!UserStatusPolicy.CanSuspend(this))
            throw new InvalidOperationException(UserStatusPolicy.GetSuspensionErrorMessage(this));

        RaiseEvent(new UserSuspendedEvent(Id, reason), _ => Status = UserStatus.Suspended);
    }
}
```

## 🔄 **Consistency Benefits**

### **1. Single Source of Truth**
- All validation rules defined in `ValidationConstants.cs`
- No duplicate constants across files
- Easy to maintain and update

### **2. Consistent Error Messages**
- All error messages use the same format
- Localization-friendly with string formatting
- Professional and user-friendly messages

### **3. Reusable Business Rules**
- Business rules can be used in multiple contexts
- Value objects, entities, and policies all use the same rules
- Consistent validation behavior

### **4. Easy Testing**
- Constants can be easily mocked or overridden
- Business rules can be tested independently
- Validation logic is isolated and testable

### **5. Domain-Driven Design Compliance**
- Business rules encapsulate domain logic
- No infrastructure dependencies in domain
- Clear separation of concerns

## 📊 **Usage Examples**

### **Creating a User with Validation**
```csharp
// All validation uses the same constants
var email = Email.Create("user@example.com"); // Uses EmailFormatRule
var password = Password.Create("StrongPass123!"); // Uses PasswordStrengthRule
var user = UserEntity.Create(id, email, password, "John", "Doe"); // Uses NameFormatRule
```

### **Validating Password Strength**
```csharp
// Using PasswordPolicy (high-level)
var result = PasswordPolicy.Validate("weak");
if (!result.IsValid)
    Console.WriteLine(result.ErrorMessage); // Uses constants

// Using PasswordStrengthRule (low-level)
var rule = new PasswordStrengthRule("weak");
if (!rule.IsSatisfied())
    Console.WriteLine(rule.Message); // Uses constants
```

### **Checking User Status Transitions**
```csharp
// Using UserStatusPolicy with constants
if (!UserStatusPolicy.CanDeactivate(user))
{
    var error = UserStatusPolicy.GetDeactivationErrorMessage(user);
    Console.WriteLine(error); // Uses constants
}
```

## 🚀 **Best Practices**

### **1. Always Use Constants**
- Never hardcode validation values
- Always reference `ValidationConstants`
- Use string formatting for dynamic messages

### **2. Use Business Rules**
- Implement validation logic in business rules
- Use `CheckRule()` method in entities
- Keep validation logic in domain layer

### **3. Consistent Error Handling**
- Use the same error message format
- Provide meaningful error messages
- Use constants for all error messages

### **4. Test Validation Logic**
- Test business rules independently
- Test value object validation
- Test entity validation

### **5. Document Changes**
- Update constants when business rules change
- Document validation requirements
- Keep constants synchronized across all files

## Conclusion

The consistent validation structure ensures that:

- **All validation rules are centralized** in `ValidationConstants.cs`
- **Business rules use the same constants** across all files
- **Error messages are consistent** and professional
- **Validation logic is reusable** and maintainable
- **Domain-driven design principles** are followed
- **Testing is simplified** with isolated validation logic

This structure provides a solid foundation for maintaining consistent validation across the entire User domain while following DDD best practices. 