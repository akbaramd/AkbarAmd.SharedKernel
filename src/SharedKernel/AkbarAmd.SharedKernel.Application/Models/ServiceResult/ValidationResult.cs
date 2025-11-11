namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Result for validation operations with detailed error information
/// </summary>
public class ValidationResult : ServiceResult
{
    public IReadOnlyList<ValidationError> Errors { get; protected set; }
    public bool HasErrors => Errors?.Any() == true;

    protected ValidationResult() : base()
    {
        Errors = new List<ValidationError>();
    }

    protected ValidationResult(bool isSuccess, string? errorMessage = null, string? errorCode = null, 
        IEnumerable<ValidationError>? errors = null) : base(isSuccess, errorMessage, errorCode)
    {
        Errors = errors?.ToList() ?? new List<ValidationError>();
    }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static new ValidationResult Success() => new(true);

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => 
        new(false, "Validation failed", "VALIDATION_ERROR", errors);

    /// <summary>
    /// Creates a failed validation result with error message
    /// </summary>
    public static new ValidationResult Failure(string errorMessage, string? errorCode = null) => 
        new(false, errorMessage, errorCode);

    /// <summary>
    /// Creates a failed validation result with error code
    /// </summary>
    public static new ValidationResult FailureWithCode(string errorCode, string errorMessage) => 
        new(false, errorMessage, errorCode);

    /// <summary>
    /// Adds a validation error
    /// </summary>
    public ValidationResult AddError(string propertyName, string errorMessage, string? errorCode = null)
    {
        var errors = Errors.ToList();
        errors.Add(new ValidationError(propertyName, errorMessage, errorCode));
        return new ValidationResult(IsSuccess, ErrorMessage, ErrorCode, errors);
    }

    /// <summary>
    /// Adds multiple validation errors
    /// </summary>
    public ValidationResult AddErrors(IEnumerable<ValidationError> errors)
    {
        var allErrors = Errors.ToList();
        allErrors.AddRange(errors);
        return new ValidationResult(IsSuccess, ErrorMessage, ErrorCode, allErrors);
    }

    /// <summary>
    /// Implicit conversion from bool to ValidationResult
    /// </summary>
    public static implicit operator ValidationResult(bool isSuccess) => 
        isSuccess ? Success() : Failure("Validation failed");

    /// <summary>
    /// Implicit conversion from string to ValidationResult (treats as error message)
    /// </summary>
    public static implicit operator ValidationResult(string errorMessage) => 
        Failure(errorMessage);

    /// <summary>
    /// Implicit conversion from Exception to ValidationResult
    /// </summary>
    public static implicit operator ValidationResult(Exception exception) => 
        Failure(exception.Message, "EXCEPTION");

    /// <summary>
    /// Implicit conversion from ValidationError to ValidationResult
    /// </summary>
    public static implicit operator ValidationResult(ValidationError error) => 
        Failure(new[] { error });
}

/// <summary>
/// Represents a validation error for a specific property
/// </summary>
public record ValidationError
{
    public string PropertyName { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }

    public ValidationError(string propertyName, string errorMessage, string? errorCode = null)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }
}
