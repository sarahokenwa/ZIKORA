namespace USSDMiddleware.Core.Interfaces.Component;

public interface IHttpService
{
    Task<T> Get<T>(string url, IDictionary<string, string>? headers);
    Task<object> Get(string url, IDictionary<string, string>? headers);
   // Task<T> Get<T>(string url, IDictionary<string, string>? headers = null, string? token = null);
     Task<T> Post<T>(string url, IDictionary<string, string>? headers, string jsonContent);
    Task<T> Post<T>(string url, HttpContent content, string token = null);
    Task<T> Get<T>(string url, string token = null);
   
}