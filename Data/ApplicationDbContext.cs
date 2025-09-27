using iTeamAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Course> Courses { get; set; }
    public DbSet<MyCourse> MyCourses { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<ITFactoryRequest> ITFactoryRequests { get; set; }
    public DbSet<StudyPlan> StudyPlans { get; set; }



}
