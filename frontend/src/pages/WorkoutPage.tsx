import { useEffect, useState } from "react";

// 型定義（DBの構造に合わせる）
interface Category { category_Id: number; category_Name: string; }
interface Exercise { exercise_Id: number; category_Id: number; exercise_Name: string; }
interface WorkoutRecord { exercise_Name: string; weight: number; reps: number; }

const WorkoutPage = () => {
    // マスターデータ用
    const [categories, setCategories] = useState<Category[]>([]);
    const [exercises, setExercises] = useState<Exercise[]>([]);
    const [history, setHistory] = useState<WorkoutRecord[]>([]);

    // 選択状態管理
    const [selectedCatId, setSelectedCatId] = useState<number>(0);
    const [selectedExId, setSelectedExId] = useState<number>(0);
    const [weight, setWeight] = useState<number | string>('');
    const [reps, setReps] = useState<number | string>('');

    // 1. データの取得（マスターデータと履歴）
    const loadAllData = async () => {
        const [catRes, exRes, historyRes] = await Promise.all([
            fetch('http://localhost:5062/api/MasterData/categories'),
            fetch('http://localhost:5062/api/MasterData/exercises'),
            fetch('http://localhost:5062/api/Workouts') // 履歴取得用
        ]);

        setCategories(await catRes.json());
        setExercises(await exRes.json());
        setHistory(await historyRes.json());
    };

    useEffect(() => { loadAllData(); }, []);

    // 2. 保存処理（IDを送信する）
    const handleSave = async () => {
        if (selectedExId === 0 || !weight || !reps) {
            alert("種目、重量、回数を入力してください");
            return;
        }

        const body = {
            exercise_Id: selectedExId, // 数字のIDを送る
            weight: Number(weight),
            reps: Number(reps)
        };

        await fetch('http://localhost:5062/api/Workouts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        setWeight('');
        setReps('');
        loadAllData(); // リストを更新
    };

    // 現在選択されている部位に属する種目だけを抽出
    const filteredExercises = exercises.filter(ex => ex.category_Id === selectedCatId);

    return (
        <div style={{ padding: '20px' }}>
            <h2>トレーニング記録</h2>

            {/* 部位の選択 */}
            <div style={{ marginBottom: '10px' }}>
                <label style={{ display: 'block', fontWeight: 'bold' }}>部位</label>
                <select 
                    style={{ width: '100%', padding: '8px' }} 
                    value={selectedCatId}
                    onChange={(e) => {
                        setSelectedCatId(Number(e.target.value));
                        setSelectedExId(0); // 部位を変えたら種目をリセット
                    }}
                >
                    <option value="0">部位を選択してください</option>
                    {categories.map(cat => (
                        <option key={cat.category_Id} value={cat.category_Id}>{cat.category_Name}</option>
                    ))}
                </select>
            </div>

            {/* 種目の選択 */}
            <div style={{ marginBottom: '10px' }}>
                <label style={{ display: 'block', fontWeight: 'bold' }}>種目</label>
                <select 
                    style={{ width: '100%', padding: '8px' }} 
                    value={selectedExId} 
                    onChange={(e) => setSelectedExId(Number(e.target.value))}
                >
                    <option value="0">種目を選択してください</option>
                    {filteredExercises.map(ex => (
                        <option key={ex.exercise_Id} value={ex.exercise_Id}>{ex.exercise_Name}</option>
                    ))}
                </select>
            </div>

            <div style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
                <input type="number" step="0.1" placeholder="重量(kg)" value={weight} onChange={e => setWeight(e.target.value)} />
                <input type="number" placeholder="回数(回)" value={reps} onChange={e => setReps(e.target.value)} />
            </div>
            
            <button onClick={handleSave} style={{ width: '80%', padding: '10px', backgroundColor: '#28a745', color: '#fff', border: 'none', borderRadius: '4px' }}>
                保存
            </button>

            <hr />

            <h3>本日の記録</h3>
            <table border={1} style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'center' }}>
                <thead>
                    <tr style={{ backgroundColor: '#eee' }}>
                        <th>種目</th>
                        <th>重量</th>
                        <th>回数</th>
                    </tr>
                </thead>
                <tbody>
                    {history.map((h, index) => (
                        <tr key={index}>
                            <td>{h.exercise_Name}</td>
                            <td>{h.weight}kg</td>
                            <td>{h.reps}回</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default WorkoutPage;