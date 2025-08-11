using System.Collections.Generic;

namespace WellbeingHub.Models
{
    public class Group
    {
        public string id { get; set; } = System.Guid.NewGuid().ToString();
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<int> MemberIds { get; set; } = new();
    }
}
