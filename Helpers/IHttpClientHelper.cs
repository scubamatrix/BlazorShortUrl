namespace BlazorShortUrl.Helpers;

public interface IHttpClientHelper
{
    Task<T?> GetFromJsonAsync<T>(string uri, HttpClient httpClient);
    Task<T?> GetJsonFromContent<T>(string uri, HttpClient httpClient);
    Task PostJsonAsync<T>(string uri, HttpClient httpClient);
}