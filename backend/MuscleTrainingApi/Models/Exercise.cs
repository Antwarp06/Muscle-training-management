using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MuscleTrainingApi.Models;

[Table("Exercises", Schema = "public")] 
public class Exercise 
{
    [Key]
    [Column("Exercise_Id")]
    public int Exercise_Id { get; set; }

    // 所有者。フロントから受け取らず、必ずJWTのクレームから取得した値を入れること
    [Column("User_Id")]
    [JsonIgnore]
    public int User_Id { get; set; }

    [Column("Category_Id")]
    [Required(ErrorMessage = "部位は必須です")]
    public int Category_Id { get; set; }

    [Column("Exercise_Name")] 
    [Required(ErrorMessage = "種目名は必須です。")]
    [MaxLength(50, ErrorMessage = "種目名は50文字以内で入力してください。")]
    public string Exercise_Name { get; set; } = "";
}