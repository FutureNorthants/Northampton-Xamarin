using Amazon.Lambda.LexEvents;

namespace norbot
{
    public class ValidationResult
    {
        public static readonly ValidationResult VALID_RESULT = new ValidationResult(true, null, null);

        public ValidationResult(bool isValid, string violationSlot, string message)
        {
            this.IsValid = isValid;
            this.ViolationSlot = violationSlot;

            if (!string.IsNullOrEmpty(message))
            {
                this.Message = new LexResponse.LexMessage { ContentType = "PlainText", Content = message };
            }
        }

        public bool IsValid { get; }
        public string ViolationSlot { get; }
        public LexResponse.LexMessage Message { get; }
    }
}
