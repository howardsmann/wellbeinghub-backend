using System.Collections.Generic;
using System.Text.Json.Serialization;   // <-- add

namespace WellbeingHub.Models
{
    public class Group
    {
        public string id { get; set; } = System.Guid.NewGuid().ToString();

        [JsonPropertyName("Id")]        // <-- add
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<int> MemberIds { get; set; } = new();
    }
}
