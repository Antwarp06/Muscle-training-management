using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
using System.ComponentModel.DataAnnotations;

namespace MuscleTrainingApi.Models;

[Table("Categories", Schema = "public")] 
public class Category
{
    [Key]
    [Column("Category_Id")] 
    public int Category_Id { get; set; }

    [Column("Category_Name")] 
    [Required(ErrorMessage = "部位名は必須です")]
    [MaxLength(30, ErrorMessage = "部位名は30文字以内で入力してください")]
    public string Category_Name { get; set; } = "";
}