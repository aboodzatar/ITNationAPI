using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTeamAPI.Models
{
    public class MyCourse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign Key to Users table

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public int CourseId { get; set; } // Foreign Key to Courses table

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public bool IsFavorite { get; set; } = false;

        public bool NotifyMe { get; set; } = false;
    }
}