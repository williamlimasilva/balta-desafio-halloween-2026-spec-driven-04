using System.Security.Cryptography;

namespace EncurtaUrl.Api.Services;

public interface IShortCodeGenerator
{
    string Generate();
}

public class ShortCodeGenerator : IShortCodeGenerator
{
    private const int CodeLength = 7;

    private static readonly char[] Alphabet =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    public string Generate()
        => new(RandomNumberGenerator.GetItems<char>(Alphabet, CodeLength));
}
