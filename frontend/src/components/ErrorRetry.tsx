// 通信エラー時に「エラーメッセージ＋再度実行ボタン」を表示する共通部品
// どの画面からでも使えるように、押されたときの処理（onRetry）は親から受け取ります

interface Props {
    onRetry: () => void; // 「再度実行」ボタンが押されたときに動かす関数
}

const ErrorRetry: React.FC<Props> = ({ onRetry }) => {
    return (
        <div style={{ textAlign: 'center', padding: '40px 20px' }}>
            <p style={{ color: '#dc3545', fontWeight: 'bold', fontSize: '1.1rem', marginBottom: '5px' }}>
                通信エラーが発生しました
            </p>
            <p style={{ color: '#666', fontSize: '0.9rem', marginBottom: '20px' }}>
                サーバーが起動中の可能性があります。少し待ってからもう一度お試しください。
            </p>
            <button
                onClick={onRetry}
                style={{ padding: '10px 30px', backgroundColor: '#3498db', color: '#fff', border: 'none', borderRadius: '4px', fontWeight: 'bold', cursor: 'pointer' }}>
                再度実行
            </button>
        </div>
    );
};

export default ErrorRetry;
