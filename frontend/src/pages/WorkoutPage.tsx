import { useEffect, useState } from "react";

//型定義
interface Workout {
    record_Id?: number;
    event_Name: string;
    weight: number;
    reps: number;
}

const WorkoutPage = () =>{
    const [ Workouts, setWorkouts ] = useState<Workout[]>([]);
    const [eventName, setEventName ] = useState('');
    const [weight, setWeight ] = useState<number | string>('');
    const [reps, setReps ] = useState<number | string>('');

//データの取得
const loadData = async () => {
    const res = await fetch('http://localhost:5062/api/Workouts');
    const date = await res.json();
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
        <ul>
            {Workouts.map((w,i)=> (
                <li key={i}>{w.event_Name}: {w.weight}kg * {w.reps}回</li>
            ))}
        </ul>
    </div>
);
};

export default WorkoutPage;