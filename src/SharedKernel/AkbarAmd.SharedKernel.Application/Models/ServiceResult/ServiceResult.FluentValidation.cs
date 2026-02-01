// File: ServiceResult.FluentValidation.cs
#nullable enable
using FluentValidation;
using FluentValidation.Results;

namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Provides integration with FluentValidation for creating validation results.
/// </summary>
public partial class ServiceResult
{
    /// <summary>
    /// Creates a validation failed result from a FluentValidation <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result containing validation failures.</param>
    /// <param name="message">An optional custom message. Defaults to "Validation failed." if not provided.</param>
    /// <param name="exception">An optional exception associated with the validation failure.</param>
    /// <returns>
    /// A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.ValidationFailed"/>
    /// and errors derived from the validation failures.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    /// <remarks>
    /// This method converts FluentValidation <see cref="ValidationFailure"/> objects into
    /// <see cref="ServiceError"/> objects, preserving error codes, messages, property names,
    /// and attempted values.
    /// </remarks>
    public static ServiceResult Validation(ValidationResult validationResult, string? message = null, Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var failures = validationResult.Errors ?? Enumerable.Empty<ValidationFailure>();
        var errors = failures
            .Select(f => ServiceError.Validation(
                code: string.IsNullOrWhiteSpace(f.ErrorCode) ? "VALIDATION_ERROR" : f.ErrorCode,
                message: f.ErrorMessage,
                target: string.IsNullOrWhiteSpace(f.PropertyName) ? null : f.PropertyName,
                attemptedValue: f.AttemptedValue))
            .ToArray();

        return new ServiceResult(
            status: ServiceResultStatus.ValidationFailed,
            message: message ?? "Validation failed.",
            errorCode: "VALIDATION_FAILED",
            errors: Array.AsReadOnly(errors),
            exception: exception,
            traceId: System.Diagnostics.Activity.Current?.TraceId.ToString(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: ServiceResultBase.EmptyMetadata);
    }

    /// <summary>
    /// Implicitly converts a FluentValidation <see cref="ValidationResult"/> to a <see cref="ServiceResult"/>.
    /// </summary>
    /// <param name="validationResult">The validation result to convert.</param>
    /// <returns>
    /// A validation failed result if the validation result has errors; otherwise, a successful result.
    /// </returns>
    public static implicit operator ServiceResult(ValidationResult validationResult)
        => validationResult.IsValid ? Ok() : Validation(validationResult);

    /// <summary>
    /// Implicitly converts a FluentValidation <see cref="ValidationFailure"/> to a <see cref="ServiceResult"/>.
    /// </summary>
    /// <param name="failure">The validation failure to convert.</param>
    /// <returns>A validation failed result containing the single failure.</returns>
    public static implicit operator ServiceResult(ValidationFailure failure)
        => Validation(new ValidationResult(new[] { failure }));

    /// <summary>
    /// Attempts to map a custom exception type to a <see cref="ServiceResult"/>.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <param name="result">When this method returns, contains the mapped result if mapping succeeded; otherwise, the default value.</param>
    /// <returns><c>true</c> if the exception was successfully mapped; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This partial method implementation handles <see cref="ValidationException"/> from FluentValidation.
    /// </remarks>
    private static partial bool TryMapException(Exception exception, out ServiceResult result)
    {
        if (exception is ValidationException validationException)
        {
            result = Validation(
                new ValidationResult(validationException.Errors),
                message: validationException.Message,
                exception: validationException);
            return true;
        }

        result = default!;
        return false;
    }
}

/// <summary>
/// Provides integration with FluentValidation for creating validation results with typed values.
/// </summary>
public sealed partial class ServiceResult<T>
{
    /// <summary>
    /// Creates a validation failed result from a FluentValidation <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result containing validation failures.</param>
    /// <param name="message">An optional custom message. Defaults to "Validation failed." if not provided.</param>
    /// <param name="exception">An optional exception associated with the validation failure.</param>
    /// <returns>
    /// A new <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.ValidationFailed"/>
    /// and errors derived from the validation failures.
    /// </returns>
    public static ServiceResult<T> Validation(ValidationResult validationResult, string? message = null, Exception? exception = null)
        => FromNonGeneric(ServiceResult.Validation(validationResult, message, exception), value: default);

    /// <summary>
    /// Implicitly converts a FluentValidation <see cref="ValidationResult"/> to a <see cref="ServiceResult{T}"/>.
    /// </summary>
    /// <param name="validationResult">The validation result to convert.</param>
    /// <returns>
    /// A validation failed result if the validation result has errors; otherwise, a successful result with <c>default(T)</c>.
    /// </returns>
    public static implicit operator ServiceResult<T>(ValidationResult validationResult)
        => Validation(validationResult);

    /// <summary>
    /// Implicitly converts a FluentValidation <see cref="ValidationFailure"/> to a <see cref="ServiceResult{T}"/>.
    /// </summary>
    /// <param name="failure">The validation failure to convert.</param>
    /// <returns>A validation failed result containing the single failure.</returns>
    public static implicit operator ServiceResult<T>(ValidationFailure failure)
        => Validation(new ValidationResult(new[] { failure }));
}
