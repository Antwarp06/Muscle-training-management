using System.ComponentModel.DataAnnotations;

namespace MuscleTrainingApi.Models;

// DTO（受け取り専用の入れ物）。
// User クラスと違い、こちらは「平文パスワード」を一時的に持つ。
// DBに保存する User とは役割が違うため、意図的に別のクラスに分けている。

/// <summary>新規登録で受け取るデータ</summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "ユーザー名は必須です。")]
    [MinLength(3, ErrorMessage = "ユーザー名は3文字以上で入力してください。")]
    [MaxLength(30, ErrorMessage = "ユーザー名は30文字以内で入力してください。")]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "ユーザー名は半角英数字とアンダースコアのみ使用できます。")]
    public string UserName { get; set; } = "";

    // BCrypt は73バイト目以降を無視するため、上限を72にして
    // 「長くしたのに実は効いていない」状態を防いでいる。
    [Required(ErrorMessage = "パスワードは必須です。")]
    [MinLength(13, ErrorMessage = "パスワードは13文字以上で入力してください。")]
    [MaxLength(72, ErrorMessage = "パスワードは72文字以内で入力してください。")]
    public string Password { get; set; } = "";
}

/// <summary>ログインで受け取るデータ</summary>
public class LoginRequest
{
    [Required(ErrorMessage = "ユーザー名は必須です。")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "パスワードは必須です。")]
    public string Password { get; set; } = "";
}
