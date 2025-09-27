namespace iTeamAPI.DTOs
{
    public class UpdateStatusDto
    {
        public string Status { get; set; } = "Pending"; // Default
        public string? AdminNotes { get; set; }

    }
}
