using System.ComponentModel.DataAnnotations;

namespace iTeamAPI.Models
{
    public class StudyPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? ImageLink { get; set; } // Link to an image on Google Drive
    }
}
