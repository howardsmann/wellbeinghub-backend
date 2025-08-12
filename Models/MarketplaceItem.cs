using System.Text.Json.Serialization;   // <-- add

namespace WellbeingHub.Models
{
    public class MarketplaceItem
    {
        public string id { get; set; } = System.Guid.NewGuid().ToString();

        [JsonPropertyName("Id")]        // <-- add
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CreatedBy { get; set; }
    }
}
