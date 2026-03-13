namespace SavaloApp.Persistance.External.Apple;

public class AppleJwksResponse
{
    public List<AppleJwk> Keys { get; set; } = new();
}

public class AppleJwk
{
    public string Kty { get; set; } = null!;
    public string Kid { get; set; } = null!;
    public string Use { get; set; } = null!;
    public string Alg { get; set; } = null!;
    public string N { get; set; } = null!;
    public string E { get; set; } = null!;
}