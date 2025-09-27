using Microsoft.AspNetCore.Identity;
using System;


namespace iTeamAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Major { get; set; }
        public string? Skills { get; set; }

        public ICollection<MyCourse> MyCourses { get; set; } = new List<MyCourse>();

    }

}
