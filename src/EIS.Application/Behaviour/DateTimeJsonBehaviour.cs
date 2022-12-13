using System.Buffers.Text;
using System.Globalization;
namespace EIS.Application.Behaviour
{
    public class DateTimeJsonBehaviour : JsonConverter<DateTime>
    {
        private readonly string dateFormat = "dd-MM-yyyy hh:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
            => DateTime.ParseExact(reader.GetString(), dateFormat, CultureInfo.InvariantCulture);

        public override void Write(Utf8Formatter writter, DateTime value, JsonSeializationOption options) 
            => writter.WriteStringValue(value.ToString(dateFormat, CultureInfo.InstalledUICulture));
    }
}