using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MuscleTrainingApi.Models;

[Table("Workout", Schema = "public")] 
public class Workout
{
    [Key]
    [Column("Record_Id")]
    public int Record_Id { get; set; }//記録ID

    // 所有者。フロントから受け取らず、必ずJWTのクレームから取得した値を入れること。
    // JsonIgnore を付けているので、リクエストのJSONに User_Id を混ぜられても無視される。
    [Column("User_Id")]
    [JsonIgnore]
    public int User_Id { get; set; }//記録した人

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