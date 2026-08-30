// --- API通信とログイン状態の管理をまとめた場所 ---
// これまで各ページに本番URLを直接書いていたが、ここ1箇所に集約している。
// ローカル開発では .env.development の VITE_API_BASE で差し替えられる。

const API_BASE =
    (import.meta.env.VITE_API_BASE as string | undefined) ??
    'https://muscle-training-management.onrender.com';

const TOKEN_KEY = 'mt_token';
const USER_KEY = 'mt_userName';

// --- トークンの保管 ---
// localStorage に置くのでブラウザを閉じても残る。
// 中身は誰でも読めるため、パスワードなど秘密の情報は入っていない（userIdと名前だけ）。
export const getToken = () => localStorage.getItem(TOKEN_KEY);
export const getUserName = () => localStorage.getItem(USER_KEY);

export const saveAuth = (token: string, userName: string) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, userName);
};

export const clearAuth = () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
};

// 401を受け取ったとき、App.tsx にログイン画面へ戻ってもらうための連絡係
let onUnauthorized: (() => void) | null = null;
export const setUnauthorizedHandler = (fn: (() => void) | null) => {
    onUnauthorized = fn;
};

/** トークン切れ・未ログインを表すエラー */
export class UnauthorizedError extends Error {
    constructor() {
        super('ログインの有効期限が切れました。もう一度ログインしてください。');
        this.name = 'UnauthorizedError';
    }
}

/**
 * ログイン中のトークンを自動で付けて通信する。
 * 各ページは fetch ではなくこちらを使うこと。付け忘れると401になる。
 */
export async function apiFetch(path: string, options: RequestInit = {}): Promise<Response> {
    const headers = new Headers(options.headers);

    const token = getToken();
    if (token) headers.set('Authorization', `Bearer ${token}`);
    if (options.body && !headers.has('Content-Type')) {
        headers.set('Content-Type', 'application/json');
    }

    const res = await fetch(`${API_BASE}${path}`, { ...options, headers });

    // トークンが無効・期限切れなら、保存済みの情報を捨ててログイン画面へ戻す。
    // 各ページで個別に判定しなくて済むよう、ここで一括して面倒を見る。
    if (res.status === 401) {
        clearAuth();
        onUnauthorized?.();
        throw new UnauthorizedError();
    }

    return res;
}

// --- ログイン・新規登録（トークン不要なのでapiFetchを通さない） ---

interface AuthResponse {
    token: string;
    userName: string;
}

async function postAuth(path: string, userName: string, password: string): Promise<AuthResponse> {
    const res = await fetch(`${API_BASE}${path}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userName, password })
    });

    const data = await res.json().catch(() => null);

    if (!res.ok) {
        // サーバーからのメッセージをそのまま画面に出せるよう整形する。
        // 入力チェックのエラーは errors の中に項目ごとに入っている。
        if (data?.errors) {
            const messages = Object.values(data.errors as Record<string, string[]>).flat();
            throw new Error(messages.join('\n'));
        }
        throw new Error(data?.message ?? '通信に失敗しました。時間をおいて再度お試しください。');
    }

    return data as AuthResponse;
}

export const login = (userName: string, password: string) =>
    postAuth('/api/Auth/login', userName, password);

export const register = (userName: string, password: string) =>
    postAuth('/api/Auth/register', userName, password);
