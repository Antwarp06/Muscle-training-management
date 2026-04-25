import React, { useState, useEffect } from 'react';

// 型定義
interface Category {
    category_Id: number;
    category_Name: string;
}

interface Exercise {
    exercise_Id: number;
    exercise_Name: string;
    category_Id: number;
}

const SettingsPage: React.FC = () => {
    const [categories, setCategories] = useState<Category[]>([]);
    const [exercises, setExercises] = useState<Exercise[]>([]); // 全種目保持用
    const [newCategoryName, setNewCategoryName] = useState('');
    const [selectedCategoryId, setSelectedCategoryId] = useState<number>(0);
    const [newExerciseName, setNewExerciseName] = useState('');

    // --- 1. データ取得関数 ---
    const fetchCategories = async () => {
        const res = await fetch('https://muscle-training-management.onrender.com/api/MasterData/categories');
        if (res.ok) setCategories(await res.json());
    };

    const fetchExercises = async () => {
        // すべての種目を取得するAPI（MasterDataに全取得がある前提）
        const res = await fetch('https://muscle-training-management.onrender.com/api/MasterData/exercises');
        if (res.ok) setExercises(await res.json());
    };

    useEffect(() => {
        fetchCategories();
        fetchExercises();
    }, []);

    // --- 2. 登録処理 ---
    const handleAddCategory = async () => {
        if (!newCategoryName.trim()) return;
        await fetch('https://muscle-training-management.onrender.com/api/MasterData/categories', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ category_Name: newCategoryName })
        });
        setNewCategoryName('');
        fetchCategories();
    };

    const handleAddExercise = async () => {
        if (!newExerciseName.trim() || selectedCategoryId === 0) return;
        await fetch('https://muscle-training-management.onrender.com/api/MasterData/exercises', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ category_Id: selectedCategoryId, exercise_Name: newExerciseName })
        });
        setNewExerciseName('');
        fetchExercises();
    };

    // --- 3. 削除処理 (今回追加！) ---
    const handleDeleteCategory = async (id: number) => {
        if (!confirm("この部位を削除しますか？")) return;
        const res = await fetch(`https://muscle-training-management.onrender.com/api/Categories/${id}`, { method: 'DELETE' });
        if (res.ok) fetchCategories();
        else alert("紐づく種目があるため削除できません");
    };

    const handleDeleteExercise = async (id: number) => {
        if (!confirm("この種目を削除しますか？")) return;
        const res = await fetch(`https://muscle-training-management.onrender.com/api/Exercises/${id}`, { method: 'DELETE' });
        if (res.ok) fetchExercises();
        else alert("記録が存在するため削除できません");
    };

    return (
        <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
            <h1>トレーニング設定</h1>
            <p>部位と種目の登録・管理を行います。</p>
            <hr />

            {/* --- 登録セクション (既存) --- */}
            <div style={{ display: 'flex', gap: '20px', marginBottom: '30px' }}>
                <section style={{ flex: 1, backgroundColor: '#f9f9f9', padding: '15px', borderRadius: '8px' }}>
                    <h3>部位追加</h3>
                    <input type="text" value={newCategoryName} onChange={e => setNewCategoryName(e.target.value)} style={{ width: '70%', padding: '8px' }} />
                    <button onClick={handleAddCategory}>登録</button>
                </section>

                <section style={{ flex: 1, backgroundColor: '#f0f7ff', padding: '15px', borderRadius: '8px' }}>
                    <h3>種目追加</h3>
                    <select value={selectedCategoryId} onChange={e => setSelectedCategoryId(Number(e.target.value))} style={{ width: '100%', marginBottom: '10px' }}>
                        <option value="0">部位を選択</option>
                        {categories.map(cat => <option key={cat.category_Id} value={cat.category_Id}>{cat.category_Name}</option>)}
                    </select>
                    <input type="text" value={newExerciseName} onChange={e => setNewExerciseName(e.target.value)} style={{ width: '70%', padding: '8px' }} />
                    <button onClick={handleAddExercise}>登録</button>
                </section>
            </div>

            <hr />

            {/* --- 4. 管理・削除セクション (今回追加！) --- */}
            <div style={{ display: 'flex', gap: '20px' }}>
                {/* 部位一覧 */}
                <div style={{ flex: 1 }}>
                    <h4>登録済みの部位</h4>
                    <ul style={{ listStyle: 'none', padding: 0 }}>
                        {categories.map(cat => (
                            <li key={cat.category_Id} style={{ display: 'flex', justifyContent: 'space-between', padding: '8px', borderBottom: '1px solid #eee' }}>
                                {cat.category_Name}
                                <button onClick={() => handleDeleteCategory(cat.category_Id)} style={{ color: 'red', border: 'none', background: 'none', cursor: 'pointer' }}>削除</button>
                            </li>
                        ))}
                    </ul>
                </div>

                {/* 種目一覧 */}
                <div style={{ flex: 1 }}>
                    <h4>登録済みの種目</h4>
                    <div style={{ maxHeight: '300px', overflowY: 'auto' }}>
                        <ul style={{ listStyle: 'none', padding: 0 }}>
                            {exercises.map(ex => (
                                <li key={ex.exercise_Id} style={{ display: 'flex', justifyContent: 'space-between', padding: '8px', borderBottom: '1px solid #eee' }}>
                                    <span>
                                        <small style={{ color: '#666' }}>[{categories.find(c => c.category_Id === ex.category_Id)?.category_Name || "未分類"}]</small> {ex.exercise_Name}
                                    </span>
                                    <button onClick={() => handleDeleteExercise(ex.exercise_Id)} style={{ color: 'red', border: 'none', background: 'none', cursor: 'pointer' }}>削除</button>
                                </li>
                            ))}
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SettingsPage;