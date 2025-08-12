using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WellbeingHub.Models
{
    public class Group
    {
        public string id { get; set; } = System.Guid.NewGuid().ToString();

        [JsonPropertyName("numericId")]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<int> MemberIds { get; set; } = new();
    }
}
