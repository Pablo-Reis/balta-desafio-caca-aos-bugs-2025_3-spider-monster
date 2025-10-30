using System.Text.Json.Serialization;

namespace BugStore.Responses
{
    public class Response<T>
    {

        public T? Data { get; private set; }
        public string? Message { get; private set; }

        [JsonIgnore]
        public bool IsSuccess => string.IsNullOrEmpty(Message);

        public Response(T? data, string message)
        {
            Data = data;
            Message = message;
        }
        public Response(string message)
        {
            Message = message;
        }
        public Response(T? data)
        {
            Data = data;
        }
    }
}
