import { useEffect, useState } from "react";

// 型定義（DBの構造に合わせる）
interface Category { category_Id: number; category_Name: string; }
interface Exercise { exercise_Id: number; category_Id: number; exercise_Name: string; }
interface WorkoutRecord { record_Id: number; exercise_Name: string; weight: number; reps: number; }

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
            fetch('https://muscle-training-management.onrender.com/api/MasterData/categories'),
            fetch('https://muscle-training-management.onrender.com/api/MasterData/exercises'),
            fetch('https://muscle-training-management.onrender.com/api/workouts') // 履歴取得用
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

        await fetch('https://muscle-training-management.onrender.com/api/workouts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        setWeight('');
        setReps('');
        loadAllData(); // リストを更新
    };

    const handkeDelete = async ( recordId: number ) => {
        if(!confirm("本当に削除しますか？")) return;
        await fetch(`https://muscle-training-management.onrender.com/api/Workouts/${recordId}`,{ method: 'DELETE'});

        loadAllData();
    };

    // 現在選択されている部位に属する種目だけを抽出
    const filteredExercises = exercises.filter(ex => ex.category_Id === selectedCatId);

    return (
        <div style={{ padding: '20px',maxWidth: '1200px',margin: '0 auto' }}>
            <h2 style={{ textAlign: 'center', marginBottom: '30px', borderBottom: '2px solid #eee',paddingBottom: '10px' }}>トレーニング記録</h2>

            {/* 分割コンテナ*/}
            <div style={{ display: 'flex', gap: '40px', alignItems: 'flex-start'}}>
                {/*入力エリア*/}
                <div style={{ flex: '0 0 350px', backgroundColor: '#f8f9fa', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0.1)' }}>
                    <h3 style={{ marginTop: 0, fontSize: '1.2rem', color: '#333' }}>新規記録入力</h3>
                    {/*部位選択*/}
                    <div style={{ marginBottom: '15px'}}>
                        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '5px'}}>部位</label>
                        <select
                        style={{ width: '100%', padding: '10px', borderRadius: '4px', border:'1px solid #ddd' }}
                        value={ selectedCatId }
                        onChange={(e) => {setSelectedCatId(Number(e.target.value)); setSelectedExId(0);}}>
                            <option value="0">部位を選択してください</option>
                            {categories.map(cat => (<option key={cat.category_Id} value={cat.category_Id}>{cat.category_Name}</option>))}
                        </select>
                    </div>
                    {/*種目の選択 */}
                    <div style={{ marginBottom: '15px'}}>
                        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '5px'}}>種目</label>
                        <select
                        style={{ width: '100%', padding: '10px', borderRadius: '4px', border:'1px solid #ddd' }}
                        value={ selectedExId }
                        onChange={(e)=> setSelectedExId(Number(e.target.value))}>
                            <option value="0">種目を選択してください</option>
                            {filteredExercises.map(ex => (<option key={ex.exercise_Id}>{ex.exercise_Name}</option>))}
                        </select>
                    </div>
                    {/* 重量・回数 */}
                    <div style={{ display: 'flex', gap: '10px', marginBottom: '20px'}}>
                        <div style={{ flex: 1}}>
                            <label style={{ display: 'block',fontSize: '0.8rem', fontWeight: 'bold'}}>重量(kg)</label>
                            <input type="number" step="0.1" style={{ width: '100%', padding: '10px', boxSizing: 'border-box' }} value={weight} onChange={e => setWeight(e.target.value)} />
                        </div>
                        <div style={{flex: 1}}>
                            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 'bold'}}>回数(回)</label>
                            <input type="number" style={{ width: '100%', padding: '10px', boxSizing: 'border-box'}} value={reps} onChange={e => setReps(e.target.value)}/>
                        </div>
                    </div>
                    <button onClick={handleSave} style={{width: '100%', padding: '12px', backgroundColor: '#28a745', color: '#fff', border: 'none', 
                    borderRadius: '4px', fontWeight: 'bold', cursor: 'pointer'}}>
                        保存
                    </button>
                </div>
                {/* 右側:履歴エリア */}
                <div style={{ flex: '1'}}>
                    <h3 style={{ marginTop: 0, fontSize: '1.2rem', color: '#333'}}>本日の記録</h3>
                    <div style={{ maxHeight: '600px', overflowY: 'auto', border: '1px solid #eee', borderRadius: '8px'}}>
                        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                            <thead>
                                <tr style={{ backgroundColor: '#f2f2f2', borderBottom: '2px solid #ddd'}}>
                                    <th style={{ padding: '12px' }}>種目</th>
                                    <th style={{ padding: '12px' }}>重量</th>
                                    <th style={{ padding: '12px' }}>回数</th>
                                    <th style={{ padding: '12px', textAlign: 'center' }}>操作</th>
                                </tr>
                            </thead>
                            <tbody>
                                {history.map((h, index) => (
                                    <tr key={index} style={{ borderBottom: '1px solid #eee' }}>
                                        <td style={{ padding: '12px', fontWeight: '500'}}>{h.exercise_Name}</td>
                                        <td style={{ padding: '12px' }}>{h.weight}kg</td>
                                        <td style={{ padding: '12px' }}>{h.reps}回</td>
                                        <td style={{ padding: '12px', textAlign: 'center'}}>
                                            <button
                                            onClick={() => handkeDelete(h.record_Id)}
                                            style={{ backgroundColor: 'transparent', color: '#dc3545', border: '1px solid #dc3545', borderRadius: '4px', 
                                            cursor: 'pointer', padding: '5px 10px', transition: '0.2s'}}
                                            onMouseOver={(e) => (e.currentTarget.style.backgroundColor = '#dc3545', e.currentTarget.style.color = 'white')}
                                            onMouseOut={(e) => (e.currentTarget.style.backgroundColor = 'transparent', e.currentTarget.style.color = '#dc3545')}>
                                            削除</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                    {history.length === 0 && <p style={{ textAlign: 'center', color: '#999', marginTop: '20px'}}>今日の記録はまだありません</p>}
                </div>
            </div>
        </div>
    );
};

export default WorkoutPage;