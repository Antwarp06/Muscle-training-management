import { useEffect, useState } from 'react';
import { Line } from 'react-chartjs-2';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js';

// Chart.jsの初期設定
ChartJS.register( CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

// --- 型定義 ---
interface Category { category_Id: number; category_Name: string; }
interface Exercise { exercise_Id: number; category_Id: number; exercise_Name: string; }
interface Workout {
    event_Name: string;
    weight: number;
    createdAt: string;
}

// 親（App.tsx）から受け取る「仕送り」の設計図
interface Props {
    categories: Category[];
    exercises: Exercise[];
    isLoading: boolean; // 親でマスタデータを取得中かどうかのフラグ
}

// React.FC<Props> を指定し、親からのデータを受け取ります
const ChartPage: React.FC<Props> = ({ categories, exercises, isLoading }) => {
    const [ chartData, setChartData ] = useState<any>(null);
    const [ isChartLoading, setIsChartLoading ] = useState(false); // グラフ用の履歴取得フラグ

    useEffect(() => {
        setIsChartLoading(true);
        fetch('https://muscle-training-management.onrender.com/api/Workouts')
            .then(res => res.json())
            .then((data: Workout[]) => {
                // グラフ用にデータ形成に整える
                setChartData({
                    labels: data.map((w) => new Date(w.createdAt).toLocaleDateString()), // ｘ軸：時間
                    datasets:[
                        {
                            label: '重量(kg)',
                            data: data.map(w => w.weight), // ｙ軸：重さ
                            borderColor: 'rgb(75, 192, 192)', // 💡タイポ修正(borderClor -> borderColor)
                            backgroundColor: 'rgba(75, 192, 192, 0.5)', // 💡アルファ値の指定をrgbaに修正
                        },
                    ],
                });
            })
            .catch(err => console.error("グラフデータの取得エラー:", err))
            .finally(() => setIsChartLoading(false));
    }, []);

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

    return (
        <div style={{ padding: '20px'}}>
            <h2>成長グラフ</h2>
            <div style={{ maxWidth: '600px', margin: '0 auto' }}> {/* 💡タイポ修正('0 aut' -> '0 auto') */}
                {chartData ? <Line data={chartData} /> : <p>データがありません</p>}
            </div>
        </div>
    );
};

export default ChartPage;