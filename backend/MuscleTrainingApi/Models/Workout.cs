using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace MuscleTrainingApi.Models;

[Table("Workout", Schema = "public")] 
public class Workout
{
    [Key]
    [Column("Record_Id")] 
    public int Record_Id { get; set; }//記録ID

    [Column("Exercise_Id")] 
    public int Exercise_Id { get; set; }//IDで紐づけ

    [Column("Weight")]
    [Range(1, 300, ErrorMessage = "重量は 1kg から 300kg の範囲で入力してください。")]
    public double Weight { get; set; }//重さ

    [Column("Reps")]
    [Range(1, 100, ErrorMessage = "回数は 1回 から 100回 の範囲で入力してください。")]
    public int Reps { get; set; }//回数

    [Column("CreatedAt")] 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;//いつ記録したかを自動で保存
}