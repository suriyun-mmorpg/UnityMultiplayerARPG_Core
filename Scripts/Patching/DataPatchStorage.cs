using System.Globalization;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiplayerARPG
{
    /// <summary>Compact rollback storage and fingerprints without retaining JSON object graphs.</summary>
    internal static class DataPatchStorage
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public static byte[] Compress(JObject data)
        {
            using (var bytes = new MemoryStream())
            {
                using (var gzip = new GZipStream(bytes, CompressionLevel.Fastest, true))
                using (var text = new StreamWriter(gzip, Utf8))
                using (var json = new JsonTextWriter(text))
                {
                    json.WriteStartObject();
                    json.WritePropertyName("data");
                    data.WriteTo(json);
                    // A double-only JSON reader would round exact decimal defaults on rollback.
                    json.WritePropertyName("decimals");
                    json.WriteStartArray();
                    foreach (JValue value in data.Descendants().OfType<JValue>())
                    {
                        if (!(value.Value is decimal number))
                            continue;
                        json.WriteStartArray();
                        json.WriteValue(value.Path);
                        json.WriteValue(number.ToString(CultureInfo.InvariantCulture));
                        json.WriteEndArray();
                    }
                    json.WriteEndArray();
                    json.WriteEndObject();
                }
                return bytes.ToArray();
            }
        }

        public static JObject Read(byte[] data)
        {
            using (var bytes = new MemoryStream(data, false))
            using (var gzip = new GZipStream(bytes, CompressionMode.Decompress))
            using (var text = new StreamReader(gzip, Utf8))
            using (var json = new JsonTextReader(text) { DateParseHandling = DateParseHandling.None })
            {
                JObject envelope = JObject.Load(json);
                JObject result = (JObject)envelope["data"];
                foreach (JArray number in (JArray)envelope["decimals"])
                    ((JValue)result.SelectToken((string)number[0])).Value = decimal.Parse((string)number[1], CultureInfo.InvariantCulture);
                // Detach the data so its parent cannot keep the envelope metadata alive.
                envelope["data"] = null;
                return result;
            }
        }

        public static string Fingerprint(JObject data)
        {
            using (var hash = SHA256.Create())
            using (var stream = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write))
            {
                using (var text = new StreamWriter(stream, Utf8, 1024, true))
                using (var json = new JsonTextWriter(text))
                    data.WriteTo(json);
                stream.FlushFinalBlock();
                return System.BitConverter.ToString(hash.Hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
