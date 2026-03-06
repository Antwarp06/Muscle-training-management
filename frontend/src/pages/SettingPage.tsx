import React, { useState, useEffect } from 'react';

// APIから返ってくるデータの型定義
interface Category {
    category_Id: number;
    category_Name: string;
}

const SettingsPage: React.FC = () => {
    const [categories, setCategories] = useState<Category[]>([]);
    const [newCategoryName, setNewCategoryName] = useState('');

    const [selectedCategoryId, setSelectedCategoryId] = useState<number>(0);
    const [newExerciseName, setNewExerciseName] = useState('');

// 1. 既存の部位を取得する関数
    const fetchCategories = async () => {
        try {
        const res = await fetch('http://localhost:5062/api/MasterData/categories');
        if (res.ok) {
            const data = await res.json();
            setCategories(data);
        }
    } catch (error) {
        console.error("部位の取得に失敗しました", error);
    }
};

    useEffect(() => {
        fetchCategories();
    }, []);

  // 2. 新しい部位を登録する処理
    const handleAddCategory = async () => {
        if (!newCategoryName.trim()) return;
        await fetch('http://localhost:5062/api/MasterData/categories', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ category_Name: newCategoryName })
    });
    
    setNewCategoryName(''); // 入力欄を空にする
    fetchCategories();      // 一覧を再取得
    alert('部位を追加しました！');
    };

  // 3. 新しい種目を登録する処理
    const handleAddExercise = async () => {
    if (!newExerciseName.trim() || selectedCategoryId === 0) {
        alert('部位を選択し、種目名を入力してください');
        return;
    }

    await fetch('http://localhost:5062/api/MasterData/exercises', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
            category_Id: selectedCategoryId, 
            exercise_Name: newExerciseName 
        })
    });

    setNewExerciseName('');
    alert('種目を登録しました！');
    };

    return (
        <div style={{ padding: '20px', maxWidth: '600px', margin: '0 auto' }}>
        <h1>トレーニング設定</h1>
        <p>自分が行う部位と種目を設定します。</p>
        <hr style={{ margin: '20px 0' }} />

      {/* --- 部位登録セクション --- */}
    <section style={{ backgroundColor: '#f9f9f9', padding: '15px', borderRadius: '8px', marginBottom: '20px' }}>
        <h3>1. 新しい部位を追加する</h3>
        <div style={{ display: 'flex', gap: '10px' }}>
            <input 
            type="text" 
            placeholder="例：胸、脚、背中" 
            value={newCategoryName}
            onChange={(e) => setNewCategoryName(e.target.value)}
            style={{ flex: 1, padding: '8px' }}
            />
            <button onClick={handleAddCategory} style={{ padding: '8px 16px', cursor: 'pointer' }}>登録</button>
        </div>
        </section>

      {/* --- 種目登録セクション --- */}
        <section style={{ backgroundColor: '#f0f7ff', padding: '15px', borderRadius: '8px' }}>
        <h3>2. 部位に種目（ワークアウト）を追加する</h3>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
            <select 
            value={selectedCategoryId} 
            onChange={(e) => setSelectedCategoryId(Number(e.target.value))}
            style={{ padding: '8px' }}>
            <option value="0">▼ 追加先の部位を選択してください</option>
            {categories.map(cat => (
                <option key={cat.category_Id} value={cat.category_Id}>{cat.category_Name}</option>
            ))}
        </select>
        <div style={{ display: 'flex', gap: '10px' }}>
            <input 
            type="text" 
            placeholder="例：ベンチプレス、スクワット" 
            value={newExerciseName}
            onChange={(e) => setNewExerciseName(e.target.value)}
            style={{ flex: 1, padding: '8px' }}/>
            <button onClick={handleAddExercise} style={{ padding: '8px 16px', cursor: 'pointer', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px' }}>
            種目を追加
            </button>
        </div>
        </div>
    </section>
    </div>
);
};

export default SettingsPage;