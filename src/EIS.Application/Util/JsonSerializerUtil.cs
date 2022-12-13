namespace EIS.Application.Util
{
    public class JsonSerializerUtil
    {
        public static string SeializeEvent(object message)
        {
            var serializerOptions = new JsonSerializerOptions
            {
                Converters = { new DateTimeJsonBehavior() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializerUtil.Seialize(message, serializerOptions);
        }
    }

    public static TOutput DeserializeObject<TOutput>(string message)
    {
        try
        {
            var serializerOptions = new JsonSerializerOptions
            {
                Converters = { new DateTimeJsonBehavior()  },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize<TOutput>(message, serializerOptions)
        }
        catch
        {
            throw ;
        }
    }

    public async static ValueTask<TOutput> DeserializeObjectAsync<TOutput>(string message CancellationToken token)
    {
        var serializerOptions = new JsonSerializerOptions
        {
            Converters = { new DateTimeJsonBehavior()  },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            byte[] byteArray = JsonSerializer.SerializeToUtf8Bytes(message);
            Stream memoryStream = new MemoryStream(byteArray);
            var awaitedComponent = await JsonSerializer.DeserializeAsync<TOutput>(memoryStream, serializerOptions, token);

            if (awaitedComponent == null)
            {
                Console.WriteLine("Payload is null");
            }
            return awaitedComponent;

        }
        catch
        {
            await Console.Out.WriteLineAsync("Error occurred while converting to JSON");
        }
    }
}