# Advanced User Factory - Domain-Driven Design Implementation

## Overview

The `UserFactory` is an advanced factory implementation that follows Domain-Driven Design (DDD) patterns and handles complex object creation scenarios. It encapsulates the complexity of user creation, provides multiple creation strategies, and ensures business rule validation.

## Key Features

### 🔧 **Multiple Creation Strategies**

#### 1. Basic User Creation
```csharp
// Standard user creation with default settings
var user = UserFactory.CreateNewUser(
    email: "john.doe@example.com",
    password: "SecurePassword123!",
    firstName: "John",
    lastName: "Doe",
    createdBy: "admin"
);
```

#### 2. User with Specific ID
```csharp
// Useful for testing, data migration, or external system integration
var user = UserFactory.CreateUserWithId(
    id: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    email: "jane.smith@example.com",
    password: "SecurePassword123!",
    firstName: "Jane",
    lastName: "Smith"
);
```

#### 3. User with Specific Status
```csharp
// Create user with non-default status
var inactiveUser = UserFactory.CreateUserWithStatus(
    email: "temp.user@example.com",
    password: "SecurePassword123!",
    firstName: "Temporary",
    lastName: "User",
    status: UserStatus.Inactive
);
```

#### 4. System User Creation
```csharp
// Create system user with enhanced validation
var systemUser = UserFactory.CreateSystemUser(
    email: "system@company.com",
    password: "VerySecurePassword123!",
    firstName: "System",
    lastName: "Administrator"
);
```

#### 5. Temporary User Creation
```csharp
// Create temporary user with relaxed validation
var tempUser = UserFactory.CreateTemporaryUser(
    email: "temp@example.com",
    password: "TempPass123",
    firstName: "Temporary",
    lastName: "User"
);
```

### 📦 **Bulk User Creation**

```csharp
// Create multiple users from data collection
var userData = new List<UserCreationData>
{
    new("user1@example.com", "Password123!", "John", "Doe"),
    new("user2@example.com", "Password123!", "Jane", "Smith"),
    new("user3@example.com", "Password123!", "Bob", "Johnson")
};

var users = UserFactory.CreateUsers(userData, createdBy: "bulk-import");
```

## Domain-Driven Design Patterns

### 🏗️ **Factory Pattern Types**

#### 1. Static Factory Method
- **Purpose**: Simple object creation with default values
- **Use Case**: Standard user registration
- **Example**: `CreateNewUser()`

#### 2. Factory Class
- **Purpose**: Complex object creation with multiple strategies
- **Use Case**: Different user types with specific requirements
- **Example**: `CreateSystemUser()`, `CreateTemporaryUser()`

#### 3. Abstract Factory (Interface-based)
- **Purpose**: Dependency injection and testability
- **Use Case**: When factory behavior needs to be mocked
- **Implementation**: Can be extended with `IUserFactory` interface

### 🎯 **Encapsulation Benefits**

#### 1. Hidden Complexity
```csharp
// Before: Complex creation logic exposed
var email = Email.Create(emailString);
var password = Password.Create(passwordString);
var user = new UserEntity(id, email, password, firstName, lastName);

// After: Encapsulated creation logic
var user = UserFactory.CreateNewUser(emailString, passwordString, firstName, lastName);
```

#### 2. Business Rule Validation
```csharp
// Factory handles all validation internally
ValidateBasicUserInput(email, password, firstName, lastName, createdBy);
var passwordValidation = PasswordPolicy.Validate(password);
```

#### 3. Default Value Management
```csharp
// Factory manages default values and computed properties
Status = UserStatus.Active;  // Default status
Id = Guid.NewGuid();        // Auto-generated ID
```

## Advanced Features

### 🔍 **Validation Strategies**

#### 1. Basic Validation
```csharp
private static void ValidateBasicUserInput(
    string email, string password, string firstName, 
    string lastName, string createdBy)
{
    // Comprehensive input validation
    // Business rule enforcement
    // Password policy validation
}
```

#### 2. Specialized Validation
```csharp
// System user validation with enhanced requirements
private static void ValidateSystemUserInput(...)
{
    ValidateBasicUserInput(...);
    // Additional system-specific validation
    if (!email.Contains("@"))
        throw new ArgumentException("System user email must be valid.");
}

// Temporary user validation with relaxed requirements
private static void ValidateTemporaryUserInput(...)
{
    // Relaxed password validation for temporary users
    if (password.Length < 6)
        throw new ArgumentException("Temporary user password must be at least 6 characters.");
}
```

### 📊 **Data Structures**

#### UserCreationData Record
```csharp
public sealed record UserCreationData
{
    public string Email { get; }
    public string Password { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public UserStatus? Status { get; }

    // Built-in validation
    public ValidationResult Validate() { ... }
}
```

#### ValidationResult Record
```csharp
public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public string ErrorMessage => string.Join("; ", Errors);
}
```

### 🔄 **Status Management**

#### Dynamic Status Application
```csharp
private static void ApplyUserStatus(UserEntity user, UserStatus status)
{
    if (status == UserStatus.Inactive)
        user.Deactivate("Factory: Initial inactive status");
    else if (status == UserStatus.Suspended)
        user.Suspend("Factory: Initial suspended status");
    else if (status == UserStatus.Deleted)
        user.Delete();
    // ... handle other statuses
}
```

## Error Handling

### 🚨 **Comprehensive Error Management**

#### 1. Input Validation Errors
```csharp
if (string.IsNullOrWhiteSpace(email))
    throw new ArgumentException("Email cannot be empty.", nameof(email));
```

#### 2. Business Rule Violations
```csharp
var passwordValidation = PasswordPolicy.Validate(password);
if (!passwordValidation.IsValid)
    throw new ArgumentException(passwordValidation.ErrorMessage, nameof(password));
```

#### 3. Bulk Operation Error Aggregation
```csharp
var errors = new List<string>();
foreach (var data in userData)
{
    try
    {
        var user = CreateNewUser(...);
        users.Add(user);
    }
    catch (Exception ex)
    {
        errors.Add($"Failed to create user with email '{data.Email}': {ex.Message}");
    }
}

if (errors.Any())
    throw new ArgumentException($"Failed to create some users: {string.Join("; ", errors)}");
```

## Usage Examples

### 🎯 **Real-World Scenarios**

#### 1. User Registration
```csharp
public class UserRegistrationService
{
    public UserEntity RegisterNewUser(RegisterUserRequest request)
    {
        return UserFactory.CreateNewUser(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            createdBy: "registration-service"
        );
    }
}
```

#### 2. System Administration
```csharp
public class SystemUserService
{
    public UserEntity CreateSystemAdministrator(CreateSystemUserRequest request)
    {
        return UserFactory.CreateSystemUser(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            createdBy: "system-admin"
        );
    }
}
```

#### 3. Data Migration
```csharp
public class UserMigrationService
{
    public UserEntity MigrateUser(LegacyUserData legacyData)
    {
        return UserFactory.CreateUserWithId(
            id: legacyData.ExternalId,
            email: legacyData.Email,
            password: legacyData.HashedPassword,
            firstName: legacyData.FirstName,
            lastName: legacyData.LastName,
            createdBy: "migration-service"
        );
    }
}
```

#### 4. Bulk Import
```csharp
public class BulkUserImportService
{
    public IEnumerable<UserEntity> ImportUsers(IEnumerable<ImportUserData> importData)
    {
        var userData = importData.Select(d => new UserCreationData(
            d.Email, d.Password, d.FirstName, d.LastName, d.Status));

        return UserFactory.CreateUsers(userData, createdBy: "bulk-import-service");
    }
}
```

## Best Practices

### ✅ **Design Principles**

1. **Single Responsibility**: Each factory method has a clear, focused purpose
2. **Open/Closed**: Easy to extend with new creation strategies
3. **Dependency Inversion**: Depends on abstractions (policies, value objects)
4. **Encapsulation**: Hides complex creation logic from clients
5. **Validation**: Comprehensive input validation and business rule enforcement

### 🔧 **Implementation Guidelines**

1. **Consistent Naming**: Clear, descriptive method names
2. **Comprehensive Documentation**: XML comments for all public methods
3. **Error Handling**: Meaningful error messages with context
4. **Thread Safety**: Factory methods are stateless and thread-safe
5. **Performance**: Efficient validation and object creation

### 🧪 **Testing Considerations**

1. **Unit Testing**: Each factory method can be tested independently
2. **Mocking**: Factory can be easily mocked for testing
3. **Validation Testing**: Test various validation scenarios
4. **Error Testing**: Test error conditions and edge cases

## Conclusion

The advanced `UserFactory` implementation provides:

- **Flexibility**: Multiple creation strategies for different scenarios
- **Reliability**: Comprehensive validation and error handling
- **Maintainability**: Clear separation of concerns and organized code
- **Extensibility**: Easy to add new creation strategies
- **Professional Quality**: Follows enterprise-grade patterns and practices

This factory serves as a foundation for building robust, scalable user management systems that can handle complex business requirements while maintaining clean, maintainable code. 