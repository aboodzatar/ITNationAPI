namespace iTeamAPI.DTOs
{
    public class MyCourseDto
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }
        public bool IsFavorite { get; set; }
        public bool NotifyMe { get; set; }

        public Course Course { get; set; } // Include full Course object
    }
}
