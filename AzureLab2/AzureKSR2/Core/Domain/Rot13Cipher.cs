namespace Core.Domain;

public static class Rot13Cipher
{
    public static string Encode(string input)
    {
        return new string(input.Select(Rotate).ToArray());
    }

    private static char Rotate(char c)
    {
        if (c >= 'a' && c <= 'z')
            return (char)('a' + (c - 'a' + 13) % 26);
        if (c >= 'A' && c <= 'Z')
            return (char)('A' + (c - 'A' + 13) % 26);
        return c;
    }
}