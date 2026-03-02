import { useEffect, useState } from "react";

//型定義
interface Workout {
    record_Id?: number;
    event_Name: string;
    weight: number;
    reps: number;
}

const CATEGORY_MASTER = {
    "胸": ["ベンチプレス", "ダンベルフライ","ペックフライ"],
    "背中": ["ラットプルダウン", "ベントオーバーロー", "チンニング"],
    "脚": ["スクワット", "レックプレス", "レッグエクステンション"],
    "肩": ["サイドレイズ", "ショルダープレス", "フロントレイズ"],
    "腕": ["アームカール","ダンベルカール","ケーブルカール"]
};

type CategoryKey = keyof typeof CATEGORY_MASTER;

const WorkoutPage = () =>{
    const [ workouts, setWorkouts ] = useState<Workout[]>([]);
    const [ selectedCategory, setSlectedCategory ] = useState<CategoryKey>("胸");
    const [ selectedEvent, setSelectedEvent ] = useState(CATEGORY_MASTER["胸"][0]);
    const [ weight, setWeight ] = useState<number | string>('');
    const [ reps, setReps ] = useState<number | string>('');

//データの取得
const loadData = async () => {
    const res = await fetch('http://localhost:5062/api/Workouts');
    const data = await res.json();
    setWorkouts(data);
    };

useEffect(() =>{
    loadData();
},[]);

//保存処理
const handleSave = async () => {
    const body: Workout = {
        event_Name: eventName,
        weight: Number(weight),
        reps: Number(reps)
        };

    await fetch('http://localhost:5062/api/Workouts',{
        method: 'POST',
        headers: { 'Content-Type':'application/json' },
        body: JSON.stringify(body)
        });

    setEventName('');
    setWeight('');
    setReps('');
    loadData();//再度読込
    }

return(
    <div style={{padding:'20px'}}>
        <h2>トレーニング記録</h2>
        {/* 部位の選択 */}
        <div>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>部位</label>
            <select style={{ width: '100%', padding: '8px' }} value={selectedCategory}
            onChange={(e) => {const cat = e.target.value as CategoryKey; setSlectedCategory(cat); setSelectedEvent(CATEGORY_MASTER[cat][0]);}}>
            {Object.keys(CATEGORY_MASTER).map(cat => <option key={cat} value={cat}>{cat}</option>)}
            </select>
        </div>

        {/* 種目の選択 */}
        <div>
            <label style={{display: 'block', marginBottom: '5px', fontWeight: 'bold'}}>種目</label>
            <select style={{ width: '100%', padding: '8px' }} value={selectedEvent} 
            onChange={(e) => setSelectedEvent(e.target.value)}>{CATEGORY_MASTER[selectedCategory].map(event => 
            (<option key={event} value={event}>{event}</option>))}
            </select>
        </div>
        <input type="number" step="0.1" min ="0" placeholder="重量(kg)" value={weight} onChange={e => setWeight(e.target.value)}/>
        <input type="number" min ="0" placeholder="回数(回)" value={reps} onChange={e => setReps(e.target.value)}/>
        <button onClick={handleSave}>保存</button>

        <hr />
        
        {/*　一覧表示エリア　*/}
        <h3></h3>
        <table border={1} style={{ width: '100%', borderCollapse: 'collapse',textAlign: 'center'}}>
            <thead>
                <tr style={{ backgroundColor: '#eee' }}>
                    <th>種目</th>
                    <th>重量</th>
                    <th>回数</th>
                </tr>
            </thead>
            <tbody>
                {workouts.map((w, index) => ( 
                    <tr key={index}>
                        <td>{w.event_Name}</td>
                        <td>{w.weight}</td>
                        <td>{w.reps}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    </div>
);
};

export default WorkoutPage;