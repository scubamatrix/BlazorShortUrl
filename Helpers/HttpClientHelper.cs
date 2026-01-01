using System.Text.Json;

namespace BlazorShortUrl.Helpers;

/// <summary>
/// TODO: Refactor HttpClient use in application??
/// Utility class for HttpClient error handling.
/// </summary>
public class HttpClientHelper : IHttpClientHelper
{
    public async Task<T?>  GetFromJsonAsync<T>(string uri, HttpClient httpClient)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T>(uri);
        }
        catch (HttpRequestException) // Non success
        {
            Console.WriteLine("An error occurred.");
        }
        catch (NotSupportedException) // When content type is not valid
        {
            Console.WriteLine("The content type is not supported.");
        }
        catch (JsonException) // Invalid JSON
        {
            Console.WriteLine("Invalid JSON.");
        }

        return default(T);
    }

    // Handle JSON from HttpContent
    // Send custom headers on the request. Or perhaps you want to inspect the response headers before deserialisation.
    public async Task<T?> GetJsonFromContent<T>(string uri, HttpClient httpClient)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.TryAddWithoutValidation("some-header", "some-value");

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // With a valid response, we can access the response body using the Content property.
        if (response.IsSuccessStatusCode)
        {
            // perhaps check some headers before deserializing

            try
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (NotSupportedException) // When content type is not valid
            {
                Console.WriteLine("The content type is not supported.");
            }
            catch (JsonException) // Invalid JSON
            {
                Console.WriteLine("Invalid JSON.");
            }
        }

        return default(T);
    }

    // Send JSON data as part of a POST request
    public async Task PostJsonAsync<T>(string uri, HttpClient httpClient)
    {
        var postResponse = await httpClient.PostAsJsonAsync<T>(uri, default(T));
        postResponse.EnsureSuccessStatusCode();
    }

    // Manually create HttpRequestMessage, perhaps to include custom headers.
    async Task PostJsonContent<T>(string uri, HttpClient httpClient)
    {
        var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(default(T))
        };

        var postResponse = await httpClient.SendAsync(postRequest);

        postResponse.EnsureSuccessStatusCode();
    }
}