using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace MuscleTrainingApi.Models;

[Table("Exercises")] 
public class Exercise 
{
    [Key]
    [Column("Exercise_Id")] 
    public int Exercise_Id { get; set; }

    [Column("Category_Id")] 
    public int Category_Id { get; set; }

    [Column("Exercise_Name")] 
    public string Exercise_Name { get; set; } = "";
}