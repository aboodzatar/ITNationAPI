using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTeamAPI.Models
{
    public class ITFactoryRequest
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }

        [Required]
        public string RequestType { get; set; } // "ExchangeCourses", "ProjectTeam", etc.

        [Required]
        public string Title { get; set; }

        [Required]
        public string Details { get; set; }

        public string? WhatsAppLink { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AdminNotes { get; set; }

        // Type-specific fields (can be null depending on RequestType)
        public bool? IsLost { get; set; }
        public string? MaterialLink { get; set; }
    }
}