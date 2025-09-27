using System.ComponentModel.DataAnnotations;
using iTeamAPI.Models;
public class Course
{
    [Key]
    public int CourseId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Department { get; set; }

    public string Description { get; set; }

    // Relationships
    public ICollection<CourseResource> Resources { get; set; } = new List<CourseResource>();

    public string? WhatsAppGroupLink { get; set; }
    public string? AdvSection { get; set; }
    public DateTime? ReminderDate { get; set; } = DateTime.UtcNow;
}