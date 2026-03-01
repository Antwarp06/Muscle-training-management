import { useEffect, useState } from 'react';
import { Line } from 'react-chartjs-2';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js';

//Chart.jsの初期設定
ChartJS.register( CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

interface Workout {
    event_Name: string;
    weight: number;
    createdAt: string;
}

const ChartPage = () =>{
    const [ chartData, setChartData ] = useState<any>(null);

    useEffect(() =>{
        fetch('http://localhost:5062/api/Workouts')
            .then(res => res.json())
            .then((data: Workout[]) =>{
                //グラフ用にデータ形成に整える
                setChartData({
                    labels: data.map((w) => new Date(w.createdAt).toLocaleDateString()),//ｘ軸：回転
                    datasets:[
                        {
                            label: '重量(kg)',
                            data: data.map(w =>w.weight),//ｙ軸：重さ
                            borderClor: 'rgb(75, 192, 192)',
                            backgroundColor: 'rgb(75, 192, 192, 0.5)',
                        },
                    ],
                });
            });
    }, []);

    return(
        <div style={{ padding: '20px'}}>
            <h2>成長グラフ</h2>
            <div style={{ maxWidth: '600px', margin: '0 aut' }}>
                {chartData ? <Line data={chartData} /> : <p>読み込み中...</p>}
            </div>
        </div>
    );
};

export default ChartPage;