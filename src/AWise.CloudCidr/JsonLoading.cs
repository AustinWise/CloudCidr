using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Austin.CloudCidr
{
    public partial class JsonLoading
    {
        public static async Task<List<(IPAddress, int, string)>> LoadGoogleData()
        {
            using var gdataStream = typeof(JsonLoading).Assembly.GetManifestResourceStream(typeof(JsonLoading), "google-cloud.json");
            Debug.Assert(gdataStream != null);
            return await LoadGoogleData(gdataStream);
        }

        public static async Task<List<(IPAddress, int, string)>> LoadGoogleData(Stream gdataStream)
        {
            var gdata = await JsonSerializer.DeserializeAsync(gdataStream, MyJsonContext.Default.GoogleCloudData);

            if (gdata is null || gdata.Prefixes is null)
                throw new Exception();

            var ret = new List<(IPAddress, int, string)>(gdata.Prefixes.Count);

            foreach (var prefix in gdata.Prefixes)
            {
                if (prefix.Service != "Google Cloud")
                    continue;

                if (prefix.Scope is null)
                    throw new Exception();

                string? ipPrefix = prefix.Ipv4Prefix ?? prefix.Ipv6Prefix;
                if (ipPrefix is null)
                    throw new Exception();

                int ndx = ipPrefix.IndexOf("/");
                if (ndx < 0)
                    throw new Exception();

                var ip = IPAddress.Parse(ipPrefix.AsSpan(0, ndx));
                int cidrSize = int.Parse(ipPrefix.AsSpan(ndx + 1), CultureInfo.InvariantCulture);

                ret.Add((ip, cidrSize, prefix.Scope));
            }

            return ret;
        }


        [JsonSerializable(typeof(GoogleCloudData))]
        partial class MyJsonContext : JsonSerializerContext
        {
        }

        class GoogleCloudData
        {
            [JsonPropertyName("syncToken")]
            public string? SyncToken { get; set; }

            [JsonPropertyName("creationTime")]
            public string? CreationTime { get; set; }

            [JsonPropertyName("prefixes")]
            public List<GoogleCloudPrefix>? Prefixes { get; set; }
        }

        class GoogleCloudPrefix
        {
            [JsonPropertyName("ipv4Prefix")]
            public string? Ipv4Prefix { get; set; }

            [JsonPropertyName("ipv6Prefix")]
            public string? Ipv6Prefix { get; set; }

            [JsonPropertyName("service")]
            public string? Service { get; set; }

            [JsonPropertyName("scope")]
            public string? Scope { get; set; }
        }
    }
}
