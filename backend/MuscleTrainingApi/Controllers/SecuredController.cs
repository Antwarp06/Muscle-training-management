using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MuscleTrainingApi.Controllers;

/// <summary>
/// ログインが必要なAPIの共通土台。
/// [Authorize] をこのクラスに付けているので、継承したコントローラーは
/// 付け忘れようがない（1箇所の付け忘れが情報漏れに直結するため、仕組みで防ぐ）。
/// </summary>
[Authorize]
public abstract class SecuredController : ControllerBase
{
    /// <summary>
    /// ログイン中のユーザーID。
    /// リクエストのJSONからではなく、署名を検証済みのトークンから取り出すため、
    /// 利用者が書き換えることはできない。DBを触る処理は必ずこの値を使うこと。
    /// </summary>
    protected int CurrentUserId
    {
        get
        {
            var value = User.FindFirst("userId")?.Value;

            // [Authorize] を通過していれば必ず入っている。
            // ここに来るのは、トークンの発行側と読み取り側の設定がズレているとき。
            if (!int.TryParse(value, out var userId))
            {
                throw new InvalidOperationException("トークンから userId を取得できませんでした。");
            }

            return userId;
        }
    }
}
