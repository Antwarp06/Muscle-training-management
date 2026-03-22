using System.ComponentModel.DataAnnotations;

namespace MuscleTrainingApi.Models;

public class Exercise {
    [Key]
    public int Exercise_Id { get; set; }
    public int Category_Id { get; set; }
    public string Exercise_Name { get; set; } = "";
}