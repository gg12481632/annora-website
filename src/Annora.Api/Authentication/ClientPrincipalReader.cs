using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace api.Authentication;

public static class ClientPrincipalReader
{
    private const string HeaderName =
        "x-ms-client-principal";

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public static ClientPrincipal? Read(
        HttpRequest request)
    {
        if (!request.Headers.TryGetValue(
                HeaderName,
                out var values))
        {
            return null;
        }

        var encodedPrincipal =
            values.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(
            encodedPrincipal))
        {
            return null;
        }

        try
        {
            var bytes =
                Convert.FromBase64String(
                    encodedPrincipal);

            var json =
                Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<
                ClientPrincipal>(
                json,
                JsonOptions);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}