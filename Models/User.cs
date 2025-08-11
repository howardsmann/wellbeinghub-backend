namespace WellbeingHub.Models
{
    public class User
    {
        public string id { get; set; } = System.Guid.NewGuid().ToString(); // Cosmos-required string id
        public int Id { get; set; }                                        // your numeric id (optional)
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public string Location { get; set; } = string.Empty;
    }
}
