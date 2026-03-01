import { useEffect, useState } from "react";

//型定義
interface Workout {
    record_Id?: number;
    event_Name: string;
    weight: number;
    reps: number;
}

const WorkoutPage = () =>{
    const [ workouts, setWorkouts ] = useState<Workout[]>([]);
    const [eventName, setEventName ] = useState('');
    const [weight, setWeight ] = useState<number | string>('');
    const [reps, setReps ] = useState<number | string>('');

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
        <input placeholder="種目" value={eventName} onChange={e => setEventName(e.target.value)}/>
        <input type="number" step="0.1" min ="0" placeholder="kg" value={weight} onChange={e => setWeight(e.target.value)}/>
        <input type="number" min ="0" placeholder="回" value={reps} onChange={e => setReps(e.target.value)}/>
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