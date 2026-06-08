namespace RaidersVault.Services;

public static class InputSanitizer
{
    public static string Clean(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    public static string? CleanOptional(string? value)
    {
        var cleanedValue = (value ?? string.Empty).Trim();

        return string.IsNullOrWhiteSpace(cleanedValue)
            ? null
            : cleanedValue;
    }
}