namespace EncurtaUrl.Api.Services;

public enum ShortCodeValidationResult
{
    Valid,
    InvalidFormat,
    Reserved
}

public static class ShortCodeValidator
{
    private const int MinimumLength = 3;
    private const int MaximumLength = 32;

    public static ShortCodeValidationResult Validate(string code)
    {
        if (code.Length < MinimumLength || code.Length > MaximumLength)
            return ShortCodeValidationResult.InvalidFormat;

        for (var index = 0; index < code.Length; index++)
        {
            if (!IsAllowedCharacter(code[index]))
                return ShortCodeValidationResult.InvalidFormat;
        }

        return string.Equals(code, "urls", StringComparison.OrdinalIgnoreCase)
            ? ShortCodeValidationResult.Reserved
            : ShortCodeValidationResult.Valid;
    }

    private static bool IsAllowedCharacter(char character)
        => character is >= 'A' and <= 'Z'
            or >= 'a' and <= 'z'
            or >= '0' and <= '9'
            or '-'
            or '_';
}