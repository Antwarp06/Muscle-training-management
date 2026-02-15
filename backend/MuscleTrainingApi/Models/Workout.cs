using System.ComponentModel.DataAnnotations;

namespace MuscleTrainingApi.Models;

public class Workout{
    [Key]
    public int Record_Id { get; set; } //記録のID
    public string Event_Name { get; set; } = string.Empty; //種目名
    public double Weight { get; set; } //重さ
    public int Reps { get; set; } //回数
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //いつ記録したかを自動で保存
}