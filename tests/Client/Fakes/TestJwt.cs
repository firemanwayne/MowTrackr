using System.Text;
using System.Text.Json;

namespace MowTrackr.Client.UnitTests.Fakes;

/// <summary>
/// Builds unsigned JWTs for tests (the client reads claims without verifying; the API validates the signature).
/// </summary>
internal static class TestJwt
{
    public static string Create(DateTimeOffset expires, string name = "Pat Lee", params string[] roles)
    {
        var payload = new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString(),
            ["name"] = name,
            ["exp"] = expires.ToUnixTimeSeconds(),
            ["role"] = roles,
        };

        return $"{Encode("{\"alg\":\"none\"}")}.{Encode(JsonSerializer.Serialize(payload))}.signature";
    }

    private static string Encode(string json)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}