namespace PulsePanel.Core.Services
{
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string Message { get; private set; }
        public ValidationSeverity Severity { get; private set; }

        private ValidationResult(bool isValid, string message, ValidationSeverity severity)
        {
            IsValid = isValid;
            Message = message;
            Severity = severity;
        }

        public static ValidationResult Success() =>
            new ValidationResult(true, "Valid", ValidationSeverity.Info);

        public static ValidationResult Warning(string message) =>
            new ValidationResult(true, message, ValidationSeverity.Warning);

        public static ValidationResult Error(string message) =>
            new ValidationResult(false, message, ValidationSeverity.Error);
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
}
