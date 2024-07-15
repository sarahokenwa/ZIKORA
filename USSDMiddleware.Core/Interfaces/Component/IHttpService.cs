using Aornis;

namespace USSDMiddleware.Core.Interfaces.Component;

public interface IHttpService
{   
     Task<Optional<string>> Get(string url, IDictionary<string, string> headers);
     Task<Optional<string>> Post(string url, IDictionary<string, string> headers, string jsonContent);
    Task<T> Post<T>(string url, HttpContent content, string token = null);
    Task<T> Get<T>(string url, string token = null);
   
}