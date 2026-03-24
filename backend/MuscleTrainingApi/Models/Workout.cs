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
    public double Weight { get; set; }//重さ

    [Column("Reps")]
    public int Reps { get; set; }//回数

    [Column("CreatedAt")] 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;//いつ記録したかを自動で保存
}