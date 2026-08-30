using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MuscleTrainingApi.Models;

[Table("Cardio", Schema = "public")]
public class Cardio
{
    [Key]
    [Column("Cardio_Id")]
    public int Cardio_Id { get; set; } //記録ID

    // 所有者。フロントから受け取らず、必ずJWTのクレームから取得した値を入れること
    [Column("User_Id")]
    [JsonIgnore]
    public int User_Id { get; set; }

    [Column("Exercise_Name")]
    [Required(ErrorMessage = "種目名は必須です。")]
    [MaxLength(50, ErrorMessage = "種目名は50文字以内で入力してください。")]
    public string Exercise_Name { get; set; } = "";//種目名

    [Column("Duration_Min")]
    [Range(1, 600, ErrorMessage = "時間は 1分 から 600分 の範囲で入力してください。")]
    public int Duration_Min { get; set; } //時間

    [Column("Distance_Km")]
    [Range(0.1, 300, ErrorMessage = "距離は 0.1km から 300km の範囲で入力してください。")]
    public double? Distance_Km { get; set; } //距離(km)＊未入力可
        //?をつけることでnull許容型になる

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
