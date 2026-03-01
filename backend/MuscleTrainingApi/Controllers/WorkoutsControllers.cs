using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Models;

[ApiController]
[Route("api/[controller]")]

public class WorkoutsController : ControllerBase{
    private readonly AppDbContext _context;

    public WorkoutsController(AppDbContext context){
        _context = context;
    }

    //データを取得する
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workout>>> GetWorkouts(){

        return await _context.Workout.ToListAsync();
    }
    [HttpPost]
    public async Task<ActionResult<Workout>> PostWorkout(Workout workout){
        _context.Workout.Add(workout);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWorkouts),new { id = workout.Record_Id }, workout);
    }
}