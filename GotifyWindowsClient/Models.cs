using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GotifyWindowsClient
{
    public class GotifyMessage
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("appid")]
        public int Appid { get; set; }

        [JsonPropertyName("message")]
        public string Content { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
