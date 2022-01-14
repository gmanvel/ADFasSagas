using Newtonsoft.Json;
using System.IO;

namespace DiscountCalculation
{
    public static class StreamExtensions
    {
        public static T Deserialize<T>(this Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}
