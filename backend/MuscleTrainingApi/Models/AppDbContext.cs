using Microsoft.EntityFrameworkCore;

namespace MuscleTrainingApi.Models;

public class AppDbContext : DbContext{
    public AppDbContext( DbContextOptions<AppDbContext>options ) : base( options ){

    }

    public DbSet<Workout> Workout { get; set; } = null!;
}