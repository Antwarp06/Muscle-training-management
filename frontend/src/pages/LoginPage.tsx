import { useState } from 'react';
import { login, register, saveAuth } from '../api';

interface Props {
    // ログイン成功をApp.tsxに知らせる
    onSuccess: (userName: string) => void;
}

const LoginPage: React.FC<Props> = ({ onSuccess }) => {
    // 同じ画面をログインと新規登録で使い回す
    const [isRegisterMode, setIsRegisterMode] = useState(false);

    const [userName, setUserName] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();//フォーム送信でページが再読み込みされるのを止める
        setErrorMessage('');

        if (!userName || !password) {
            setErrorMessage('ユーザー名とパスワードを入力してください。');
            return;
        }

        setIsSubmitting(true);
        try {
            const result = isRegisterMode
                ? await register(userName, password)
                : await login(userName, password);

            // トークンを保存すると、以降の通信に自動で付くようになる
            saveAuth(result.token, result.userName);
            onSuccess(result.userName);
        } catch (error) {
            setErrorMessage(error instanceof Error ? error.message : '通信に失敗しました。');
        } finally {
            setIsSubmitting(false);
        }
    };

    const inputStyle: React.CSSProperties = {
        width: '100%', padding: '12px', boxSizing: 'border-box',
        borderRadius: '4px', border: '1px solid #ddd', fontSize: '1rem'
    };

    return (
        <div style={{
            display: 'flex', justifyContent: 'center', alignItems: 'center',
            minHeight: '100vh', backgroundColor: '#f8f9fa', padding: '20px'
        }}>
            <div style={{
                width: '100%', maxWidth: '380px', backgroundColor: '#fff',
                padding: '32px', borderRadius: '8px', boxShadow: '0 2px 12px rgba(0,0,0,0.08)'
            }}>
                <h2 style={{ textAlign: 'center', marginTop: 0, marginBottom: '8px' }}>筋トレ管理</h2>
                <p style={{ textAlign: 'center', color: '#666', fontSize: '0.9rem', marginTop: 0, marginBottom: '24px' }}>
                    {isRegisterMode ? 'アカウントを作成します' : 'ログインしてください'}
                </p>

                <form onSubmit={handleSubmit}>
                    <div style={{ marginBottom: '16px' }}>
                        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '6px', fontSize: '0.9rem' }}>
                            ユーザー名
                        </label>
                        <input
                            type="text"
                            style={inputStyle}
                            value={userName}
                            autoComplete="username"
                            onChange={(e) => setUserName(e.target.value)}
                        />
                        {isRegisterMode && (
                            <p style={{ fontSize: '0.75rem', color: '#888', margin: '4px 0 0' }}>
                                半角英数字とアンダースコア、3〜30文字
                            </p>
                        )}
                    </div>

                    <div style={{ marginBottom: '20px' }}>
                        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '6px', fontSize: '0.9rem' }}>
                            パスワード
                        </label>
                        <input
                            type="password"
                            style={inputStyle}
                            value={password}
                            autoComplete={isRegisterMode ? 'new-password' : 'current-password'}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        {isRegisterMode && (
                            <p style={{ fontSize: '0.75rem', color: '#888', margin: '4px 0 0' }}>
                                13文字以上
                            </p>
                        )}
                    </div>

                    {errorMessage && (
                        <div style={{
                            backgroundColor: '#f8d7da', color: '#842029', padding: '12px',
                            borderRadius: '4px', marginBottom: '16px', fontSize: '0.85rem',
                            whiteSpace: 'pre-line'//改行入りのメッセージをそのまま表示する
                        }}>
                            {errorMessage}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={isSubmitting}
                        style={{
                            width: '100%', padding: '12px', border: 'none', borderRadius: '4px',
                            backgroundColor: isSubmitting ? '#94d3a2' : '#28a745', color: '#fff',
                            fontWeight: 'bold', fontSize: '1rem',
                            cursor: isSubmitting ? 'default' : 'pointer'
                        }}>
                        {isSubmitting ? '通信中...' : (isRegisterMode ? '登録する' : 'ログイン')}
                    </button>
                </form>

                <div style={{ textAlign: 'center', marginTop: '20px' }}>
                    <button
                        onClick={() => { setIsRegisterMode(!isRegisterMode); setErrorMessage(''); }}
                        style={{
                            background: 'none', border: 'none', color: '#0d6efd',
                            cursor: 'pointer', fontSize: '0.85rem', textDecoration: 'underline'
                        }}>
                        {isRegisterMode ? 'ログイン画面に戻る' : 'アカウントをお持ちでない方はこちら'}
                    </button>
                </div>

                <p style={{ fontSize: '0.75rem', color: '#999', textAlign: 'center', marginTop: '24px', marginBottom: 0 }}>
                    ※ パスワードを忘れた場合の再設定機能はまだありません
                </p>
            </div>
        </div>
    );
};

export default LoginPage;
