namespace WebAPIPreparation.Utility
{
    /// <summary>Thrown when a record with the same key or unique value already exists. Becomes HTTP 409.</summary>
    public sealed class ConflictException(string message) : Exception(message);

    /// <summary>Thrown when a related record (for example a Country for a State) does not exist. Becomes HTTP 400.</summary>
    public sealed class ValidationException(string message) : Exception(message);

    /// <summary>Bad input, e.g. unknown role → HTTP 400</summary>
    public class BadRequestException(string message) : Exception(message);

    /// <summary>Record not found → HTTP 404</summary>
    public class NotFoundException(string message) : Exception(message);


}
