using System.Text.Json.Serialization;   // <-- add this

namespace WellbeingHub.Models
{
    public class User
    {
        public string id { get; set; } = System.Guid.NewGuid().ToString();

        [JsonPropertyName("Id")]        // <-- keeps the JSON name as "Id" (not camel-cased to "id")
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public string Location { get; set; } = string.Empty;
    }
}
