import { useEffect, useState } from "react";
import { apiFetch } from '../api';
import ErrorRetry from "../components/ErrorRetry";

// --- 型定義 ---
interface CardioRecord {
    cardio_Id: number;
    exercise_Name: string;
    duration_Min: number;
    distance_Km: number | null; // 距離が未入力の場合はnullが入る
}

// 有酸素運動の種目リスト(マスタテーブルを使わない固定リスト)
const CARDIO_TYPES = ["ランニング", "ウォーキング", "サイクリング", "水泳", "エアロバイク"];

const CardioPage = () => {
    const [history, setHistory] = useState<CardioRecord[]>([]);
    const [isHistoryLoading, setIsHistoryLoading] = useState(false);
    const [historyError, setHistoryError] = useState(false);

    // 入力状態管理
    const [selectedType, setSelectedType] = useState<string>("");
    const [duration, setDuration] = useState<number | string>('');
    const [distance, setDistance] = useState<number | string>('');

    // --- 1.履歴データの取得 ---
    const loadHistoryData = async () => {
        setIsHistoryLoading(true);
        setHistoryError(false);
        try {
            const res = await apiFetch('/api/Cardio');
            if (!res.ok) throw new Error("履歴の取得失敗");
            setHistory(await res.json());
        } catch (error) {
            console.error("履歴の取得失敗:", error);
            setHistoryError(true);
        } finally {
            setIsHistoryLoading(false);
        }
    };

    // ページが開いた時に履歴を読み込む
    useEffect(() => {
        loadHistoryData();
    }, []);

    // --- 2.保存処理 ---
    const handleSave = async () => {
        if (!selectedType || !duration) {
            alert("種目と時間を入力してください");
            return;
        }

        const numDuration = Number(duration);
        if (numDuration < 1 || numDuration > 600) {
            alert("時間は 1分から 600分 の範囲で入力してください!");
            return;
        }
        // 距離は任意入力。ただし入力された場合だけ範囲チェックする
        if (distance !== '' && (Number(distance) < 0.1 || Number(distance) > 300)) {
            alert("距離は 0.1km から 300km の範囲で入力してください!");
            return;
        }

        const body = {
            exercise_Name: selectedType,
            duration_Min: numDuration,
            distance_Km: distance === '' ? null : Number(distance) // 未入力ならnullを送る
        };

        try {
            const res = await apiFetch('/api/Cardio', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!res.ok) {
                alert("保存に失敗しました。時間をおいて再度お試しください。");
                return;
            }
            setDuration('');
            setDistance('');
            loadHistoryData();
        } catch (error) {
            console.error("保存の通信エラー:", error);
            alert("通信エラーが発生しました。サーバーが起動中の可能性があるので、少し待ってから再度お試しください。");
        }
    };

    // --- 3.削除処理 ---
    const handleDelete = async (cardioId: number) => {
        if (!confirm("本当に削除しますか？")) return;
        try {
            const res = await apiFetch(`/api/Cardio/${cardioId}`, { method: 'DELETE' });
            if (!res.ok) {
                alert("削除に失敗しました。");
                return;
            }
            loadHistoryData();
        } catch (error) {
            console.error("削除の通信エラー:", error);
            alert("通信エラーが発生しました。");
        }
    };

    // 履歴の読み込み中のロード画面
    if (isHistoryLoading && history.length === 0) {
        return (
            <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                <div style={{ width: '50px', height: '50px', border: '5px solid #f3f3f3', borderTop: '5px solid #3498db', borderRadius: '50%', animation: 'spin 1s linear infinite', marginBottom: '20px' }}></div>
                <h2>データを読み込んでいます...</h2>
                <style>{' @keyframes spin{ 0% {transform: rotate(0deg);} 100% {transform: rotate(360deg);} } '}</style>
            </div>
        );
    }

    return (
        <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
            <h2 style={{ textAlign: 'center', marginBottom: '30px', borderBottom: '2px solid #eee', paddingBottom: '10px' }}>有酸素運動記録</h2>

            <div style={{ display: 'flex', gap: '40px', alignItems: 'flex-start' }}>
                {/* 左：入力エリア */}
                <div style={{ flex: '0 0 350px', backgroundColor: '#f8f9fa', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0.1)' }}>
                    <h3 style={{ marginTop: 0, fontSize: '1.2rem', color: '#333' }}>新規記録入力</h3>

                    <div style={{ marginBottom: '15px' }}>
                        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '5px' }}>種目</label>
                        <select
                            style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ddd' }}
                            value={selectedType}
                            onChange={(e) => setSelectedType(e.target.value)}>
                            <option value="">種目を選択してください</option>
                            {CARDIO_TYPES.map(type => (<option key={type} value={type}>{type}</option>))}
                        </select>
                    </div>

                    <div style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
                        <div style={{ flex: 1 }}>
                            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 'bold' }}>時間(分)</label>
                            <input type="number" min="1" max="600" style={{ width: '100%', padding: '10px', boxSizing: 'border-box' }} value={duration} onChange={e => setDuration(e.target.value)} />
                        </div>
                        <div style={{ flex: 1 }}>
                            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 'bold' }}>距離(km)※任意</label>
                            <input type="number" min="0.1" max="300" step="0.1" style={{ width: '100%', padding: '10px', boxSizing: 'border-box' }} value={distance} onChange={e => setDistance(e.target.value)} />
                        </div>
                    </div>

                    <button onClick={handleSave} style={{ width: '100%', padding: '12px', backgroundColor: '#28a745', color: '#fff', border: 'none', borderRadius: '4px', fontWeight: 'bold', cursor: 'pointer' }}>
                        保存
                    </button>
                </div>

                {/* 右：履歴エリア */}
                <div style={{ flex: '1' }}>
                    <h3 style={{ marginTop: 0, fontSize: '1.2rem', color: '#333' }}>本日の記録</h3>
                    {historyError ? (
                        <div style={{ border: '1px solid #eee', borderRadius: '8px' }}>
                            <ErrorRetry onRetry={loadHistoryData} />
                        </div>
                    ) : (
                        <>
                            <div style={{ maxHeight: '600px', overflowY: 'auto', border: '1px solid #eee', borderRadius: '8px' }}>
                                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                                    <thead>
                                        <tr style={{ backgroundColor: '#f2f2f2', borderBottom: '2px solid #ddd' }}>
                                            <th style={{ padding: '12px' }}>種目</th>
                                            <th style={{ padding: '12px' }}>時間</th>
                                            <th style={{ padding: '12px' }}>距離</th>
                                            <th style={{ padding: '12px', textAlign: 'center' }}>操作</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {history.map((h) => (
                                            <tr key={h.cardio_Id} style={{ borderBottom: '1px solid #eee' }}>
                                                <td style={{ padding: '12px', fontWeight: '500' }}>{h.exercise_Name}</td>
                                                <td style={{ padding: '12px' }}>{h.duration_Min}分</td>
                                                <td style={{ padding: '12px' }}>{h.distance_Km != null ? `${h.distance_Km}km` : '－'}</td>
                                                <td style={{ padding: '12px', textAlign: 'center' }}>
                                                    <button
                                                        onClick={() => handleDelete(h.cardio_Id)}
                                                        style={{ backgroundColor: 'transparent', color: '#dc3545', border: '1px solid #dc3545', borderRadius: '4px', cursor: 'pointer', padding: '5px 10px', transition: '0.2s' }}
                                                        onMouseOver={(e) => (e.currentTarget.style.backgroundColor = '#dc3545', e.currentTarget.style.color = 'white')}
                                                        onMouseOut={(e) => (e.currentTarget.style.backgroundColor = 'transparent', e.currentTarget.style.color = '#dc3545')}>
                                                        削除
                                                    </button>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                            {history.length === 0 && <p style={{ textAlign: 'center', color: '#999', marginTop: '20px' }}>今日の記録はまだありません</p>}
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

export default CardioPage;