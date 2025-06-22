using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SignPuddle.API.E2ETests.Helpers
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetJsonAsync<T>(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<T> PostJsonAsync<T>(this HttpClient client, string url, T data)
        {
            var response = await client.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public static async Task PutJsonAsync<T>(this HttpClient client, string url, T data)
        {
            var response = await client.PutAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
        }

        public static async Task DeleteAsync(this HttpClient client, string url)
        {
            var response = await client.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
    }
}