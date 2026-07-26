using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuscleTrainingApi.Models;

[Table("Cardio", Schema = "public")]
public class Cardio
{
    [Key]
    [Column("Cardio_Id")]
    public int Cardio_Id { get; set; } //記録ID

    [Column("Exercise_Name")]
    public string Exercise_Name { get; set; } = "";//種目名

    [Column("Duration_Min")]
    [Range(1, 600, ErrorMessage = "時間は 1分 から 600分 の範囲で入力してください。")]
    public int Duration_Min { get; set; } //時間

    [Column("Distance_Km")]
    [Range(0.1, 300, ErrorMessage = "距離は 0.1km から 300km の範囲で入力してください。")]
    public double? Distance_Km { get; set; } //距離(km)＊未入力可
        //?をつけることでnull許容型になる0
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}