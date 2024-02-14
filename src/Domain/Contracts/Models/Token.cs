namespace Domain.Contracts.Models;
public readonly struct Token
{
    public string Value { get; }
    public DateTime Expires { get; }

    public Token(string value, DateTime expires)
    {
        Value = value;
        Expires = expires;
    }
}
