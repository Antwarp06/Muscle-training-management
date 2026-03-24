using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace MuscleTrainingApi.Models;

[Table("Categories", Schema = "public")] 
public class Category
{
    [Key]
    [Column("Category_Id")] 
    public int Category_Id { get; set; }

    [Column("Category_Name")] 
    public string Category_Name { get; set; } = "";
}