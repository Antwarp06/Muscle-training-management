import { useEffect, useState } from 'react';
import { Line } from 'react-chartjs-2';
import ErrorRetry from '../components/ErrorRetry';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js';

// Chart.jsの初期設定
ChartJS.register( CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

// --- 型定義 ---
interface Category { category_Id: number; category_Name: string; }
interface Exercise { exercise_Id: number; category_Id: number; exercise_Name: string; }
interface Workout {
    record_Id: number;
    exercise_Id: number;
    exercise_Name: string;
    weight: number;
    reps: number;
    createdAt: string | null;
}

// 親（App.tsx）から受け取る「仕送り」の設計図
interface Props {
    categories: Category[];
    exercises: Exercise[];
    isLoading: boolean; // 親でマスタデータを取得中かどうかのフラグ
}

// React.FC<Props> を指定し、親からのデータを受け取ります
const ChartPage: React.FC<Props> = ({ categories, exercises, isLoading }) => {
    const [ workouts, setWorkouts ] = useState<Workout[]>([]);
    const [ isChartLoading, setIsChartLoading ] = useState(false); // グラフ用の履歴取得フラグ
    const [ chartError, setChartError ] = useState(false); // グラフデータの取得で通信エラーが起きたかどうかのフラグ

    // 選択状態管理（記録画面と同じ「部位 → 種目」の2段階選択）
    const [ selectedCatId, setSelectedCatId ] = useState<number>(0);
    const [ selectedExId, setSelectedExId ] = useState<number>(0);

    // 「再度実行」ボタンからも呼び出せるように、useEffect の外に関数として定義します
    const loadChartData = () => {
        setIsChartLoading(true);
        setChartError(false); // 再実行するときは、前回のエラー表示をいったんリセット
        fetch('https://muscle-training-management.onrender.com/api/Workouts')
            .then(res => {
                if (!res.ok) throw new Error("グラフデータの取得失敗"); // サーバーがエラーを返した場合も catch に送る
                return res.json();
            })
            .then((data: Workout[]) => setWorkouts(data))
            .catch(err => {
                console.error("グラフデータの取得エラー:", err);
                setChartError(true); // エラー表示のスイッチをON
            })
            .finally(() => setIsChartLoading(false));
    };

    useEffect(() => {
        loadChartData();
    }, []);

    // 現在選択されている部位に属する種目だけを抽出
    const filteredExercises = exercises.filter(ex => Number(ex.category_Id) === Number(selectedCatId));

    // 選択中の種目の記録だけを、古い順（左→右）に並べ替える
    const selectedExercise = exercises.find(ex => ex.exercise_Id === selectedExId);
    const targetWorkouts = workouts
        .filter(w => Number(w.exercise_Id) === Number(selectedExId))
        .sort((a, b) => new Date(a.createdAt ?? 0).getTime() - new Date(b.createdAt ?? 0).getTime());

    // グラフ用データ（種目が選ばれている時だけ作る）
    const chartData = selectedExercise ? {
        labels: targetWorkouts.map(w => w.createdAt ? new Date(w.createdAt).toLocaleDateString() : '日付なし'), // ｘ軸：日付
        datasets: [
            {
                label: `${selectedExercise.exercise_Name}の重量(kg)`, // 凡例に種目名を表示
                data: targetWorkouts.map(w => w.weight), // ｙ軸：重さ
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.5)',
            },
        ],
    } : null;

    // 親のデータ読み込み中、またはグラフデータの読み込み中のロード画面
    if (isLoading || isChartLoading) {
        return (
            <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                <div style={{ width: '50px', height: '50px', border: '5px solid #f3f3f3', borderTop: '5px solid #3498db', borderRadius: '50%', animation: 'spin 1s linear infinite', marginBottom: '20px' }}></div>
                <h2>データを読み込んでいます...</h2>
                <style>{' @keyframes spin{ 0% {transform: rotate(0deg);} 100% {transform: rotate(360deg);} } '}</style>
            </div>
        );
    }

    // グラフデータの取得に失敗した場合は、エラーメッセージと再度実行ボタンを表示
    if (chartError) {
        return (
            <div style={{ padding: '20px'}}>
                <h2>成長グラフ</h2>
                <ErrorRetry onRetry={loadChartData} />
            </div>
        );
    }

    return (
        <div style={{ padding: '20px'}}>
            <h2>成長グラフ</h2>

            {/* 部位と種目の選択エリア */}
            <div style={{ display: 'flex', gap: '10px', maxWidth: '600px', margin: '0 auto 20px' }}>
                <select
                    style={{ flex: 1, padding: '10px', borderRadius: '4px', border: '1px solid #ddd' }}
                    value={ selectedCatId }
                    onChange={(e) => { setSelectedCatId(Number(e.target.value)); setSelectedExId(0); }}>
                    <option value="0">部位を選択してください</option>
                    {categories.map(cat => (<option key={cat.category_Id} value={cat.category_Id}>{cat.category_Name}</option>))}
                </select>

                <select
                    style={{ flex: 1, padding: '10px', borderRadius: '4px', border: '1px solid #ddd' }}
                    value={ selectedExId }
                    onChange={(e) => setSelectedExId(Number(e.target.value))}>
                    <option value="0">種目を選択してください</option>
                    {filteredExercises.map(ex => (<option key={ex.exercise_Id} value={ex.exercise_Id}>{ex.exercise_Name}</option>))}
                </select>
            </div>

            <div style={{ maxWidth: '600px', margin: '0 auto' }}>
                {!selectedExercise
                    ? <p style={{ textAlign: 'center', color: '#999' }}>部位と種目を選ぶと成長グラフが表示されます</p>
                    : (chartData && targetWorkouts.length > 0
                        ? <Line data={chartData} />
                        : <p style={{ textAlign: 'center', color: '#999' }}>この種目の記録はまだありません</p>)}
            </div>
        </div>
    );
};

export default ChartPage;
