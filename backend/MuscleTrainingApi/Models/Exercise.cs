using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace MuscleTrainingApi.Models;

[Table("Exercises", Schema = "public")] 
public class Exercise 
{
    [Key]
    [Column("Exercise_id")] 
    public int Exercise_Id { get; set; }

    [Column("Category_id")] 
    public int Category_Id { get; set; }

    [Column("Exercise_Name")] 
    public string Exercise_Name { get; set; } = "";
}