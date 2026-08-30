using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MuscleTrainingApi.Models;

[Table("Users", Schema = "public")]
public class User
{
    [Key]
    [Column("User_Id")]
    public int User_Id { get; set; }//ユーザーID。JWTに入れて本人確認に使う

    [Column("User_Name")]
    [Required(ErrorMessage = "ユーザー名は必須です。")]
    [MaxLength(30, ErrorMessage = "ユーザー名は30文字以内で入力してください。")]
    public string User_Name { get; set; } = "";//ログインID。全ユーザーで一意

    // JsonIgnore を付けると、このプロパティはJSONに変換されなくなる。
    // 付け忘れると、ユーザー情報を返すAPIを作ったときにハッシュ値まで外部に出てしまう。
    [Column("Password_Hash")]
    [MaxLength(100)]
    [JsonIgnore]
    public string Password_Hash { get; set; } = "";//BCryptのハッシュ値。出力は60文字だが余裕を持たせる

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;//登録日時

    // ? を付けると「値が入っていない状態(NULL)」を持てるようになる。
    // 一度も更新していないユーザーは NULL のまま。
    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }//最終更新日時
}
