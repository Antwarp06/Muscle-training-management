import React, { useState } from 'react';

// --- 型定義 ---
interface Category {
    category_Id: number;
    category_Name: string;
}

interface Exercise {
    exercise_Id: number;
    exercise_Name: string;
    category_Id: number;
}

// 親（App.tsx）から受け取る「仕送り」の設計図（Props）
interface Props {
    categories: Category[];
    setCategories: React.Dispatch<React.SetStateAction<Category[]>>;
    exercises: Exercise[];
    setExercises: React.Dispatch<React.SetStateAction<Exercise[]>>;
    isLoading: boolean;
}

// () の中に Props を書いて、親からのデータを受け入れます
const SettingsPage: React.FC<Props> = ({ categories, setCategories, exercises, setExercises, isLoading }) => {
    const [newCategoryName, setNewCategoryName] = useState('');
    const [selectedCategoryId, setSelectedCategoryId] = useState<number>(0);
    const [newExerciseName, setNewExerciseName] = useState('');
    // 通信自体が失敗したときに、リロードボタン付きのお知らせを出すためのスイッチ
    const [commError, setCommError] = useState(false);

    // --- ロード画面表示（親の isLoading をそのまま使います） ---
    if (isLoading) {
        return (
            <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', height: '100vh', textAlign: 'center' }}>
                <div className="spinner" style={{ width: '50px', height: '50px', border: '5px solid #f3f3f3', borderTop: '5px solid #3498db', borderRadius: '50%', animation: 'spin 1s linear infinite', marginBottom: '20px' }}></div>
                <h2>サーバを起動しています...</h2>
                <p>無料サーバーを使用しているため、起動に最大1分ほどかかる場合があります。</p>
                <style>{' @keyframes spin{ 0% {transform: rotate(0deg);} 100% {transform: rotate(360deg);} } '}</style>
            </div>
        );
    }

    // --- 最新データの取り直し（登録成功後にサーバーと同期するため） ---
    const refreshCategories = async () => {
        const res = await fetch('https://muscle-training-management.onrender.com/api/MasterData/categories');
        const data = await res.json();
        setCategories(data.map((c: any) => ({
            category_Id: Number(c.category_Id ?? c.Category_Id) || 0,
            category_Name: String(c.category_Name ?? c.Category_Name) || ""
        })));
    };

    const refreshExercises = async () => {
        const res = await fetch('https://muscle-training-management.onrender.com/api/MasterData/exercises');
        const data = await res.json();
        setExercises(data.map((e: any) => ({
            exercise_Id: Number(e.exercise_Id ?? e.Exercise_Id) || 0,
            exercise_Name: String(e.exercise_Name ?? e.Exercise_Name) || "",
            category_Id: Number(e.category_Id ?? e.Category_Id) || 0
        })));
    };

    // --- 2. 登録処理 ---
    const handleAddCategory = async () => {
        if (!newCategoryName.trim()) {
            alert("部位名を入力してください");
            return;
        }
        const isDuplicate = categories.some(cat => cat.category_Name === newCategoryName.trim());
        if (isDuplicate) {
            alert("その部位はすでに登録されています！");
            return;
        }
        // 通信自体の失敗（ネット切断・サーバー停止など）は catch で受け止める
        try {
            const res = await fetch('https://muscle-training-management.onrender.com/api/MasterData/categories', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ category_Name: newCategoryName })
            });

            // 【同期】保存に成功したら、サーバーから最新の一覧を取り直して画面へ反映する
            if (res.ok) {
                await refreshCategories();
                setNewCategoryName('');
            } else {
                alert("部位の登録に失敗しました。時間をおいて再度お試しください。");
            }
        } catch {
            setCommError(true); // 画面上部にリロードボタン付きのお知らせを表示
        }
    };

    const handleAddExercise = async () => {
        if (!newExerciseName.trim() || selectedCategoryId === 0) {
            alert("部位を選択し、種目名を入力してください");
            return;
        }

        const isDuplicate = exercises.some( ex => ex.category_Id === selectedCategoryId && ex.exercise_Name === newExerciseName.trim() );
        if (isDuplicate) {
        alert("その種目はすでに登録されています！");
        return;
        }
        
        // 通信自体の失敗（ネット切断・サーバー停止など）は catch で受け止める
        try {
            const res = await fetch('https://muscle-training-management.onrender.com/api/MasterData/exercises', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ category_Id: selectedCategoryId, exercise_Name: newExerciseName })
            });

            // 【同期】保存に成功したら、サーバーから最新の一覧を取り直して画面へ反映する
            if (res.ok) {
                await refreshExercises();
                setNewExerciseName('');
            } else {
                alert("種目の登録に失敗しました。時間をおいて再度お試しください。");
            }
        } catch {
            setCommError(true); // 画面上部にリロードボタン付きのお知らせを表示
        }
    };

    // --- 3. 削除処理 ---
    const handleDeleteCategory = async (id: number) => {
        if (!confirm("この部位を削除しますか？")) return;
        try {
            const res = await fetch(`https://muscle-training-management.onrender.com/api/Categories/${id}`, { method: 'DELETE' });

            if (res.ok) {
                // 【通信削減】削除されたIDだけを手元のリストから省く
                setCategories(categories.filter(c => c.category_Id !== id));
            } else {
                alert("紐づく種目があるため削除できません");
            }
        } catch {
            setCommError(true); // 画面上部にリロードボタン付きのお知らせを表示
        }
    };

    const handleDeleteExercise = async (id: number) => {
        if (!confirm("この種目を削除しますか？")) return;
        try {
            const res = await fetch(`https://muscle-training-management.onrender.com/api/Exercises/${id}`, { method: 'DELETE' });

            if (res.ok) {
                // 【通信削減】削除されたIDだけを手元のリストから省く
                setExercises(exercises.filter(e => e.exercise_Id !== id));
            } else {
                alert("記録が存在するため削除できません");
            }
        } catch {
            setCommError(true); // 画面上部にリロードボタン付きのお知らせを表示
        }
    };

    return (
        <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
            <h1>トレーニング設定</h1>
            <p>部位と種目の登録・管理を行います。</p>
            <hr />

            {/* --- 通信失敗のお知らせ（リロードボタン付き） --- */}
            {commError && (
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '10px', backgroundColor: '#fdecea', border: '1px solid #dc3545', borderRadius: '8px', padding: '10px 15px', marginBottom: '20px' }}>
                    <span style={{ color: '#dc3545', fontWeight: 'bold' }}>
                        通信に失敗しました。ネットワーク接続を確認するか、ページをリロードしてください。
                    </span>
                    <span style={{ display: 'flex', gap: '8px', flexShrink: 0 }}>
                        <button onClick={() => window.location.reload()} style={{ padding: '8px 20px', backgroundColor: '#3498db', color: '#fff', border: 'none', borderRadius: '4px', fontWeight: 'bold', cursor: 'pointer' }}>
                            リロード
                        </button>
                        <button onClick={() => setCommError(false)} style={{ padding: '8px 12px', backgroundColor: 'transparent', color: '#666', border: '1px solid #ccc', borderRadius: '4px', cursor: 'pointer' }}>
                            閉じる
                        </button>
                    </span>
                </div>
            )}

            {/* --- 登録セクション --- */}
            <div style={{ display: 'flex', gap: '20px', marginBottom: '30px' }}>
                <section style={{ flex: 1, backgroundColor: '#f9f9f9', padding: '15px', borderRadius: '8px' }}>
                    <h3>部位追加</h3>
                    <input type="text" value={newCategoryName} onChange={e => setNewCategoryName(e.target.value)} maxLength={30} required placeholder="部位名を入力（30文字以内）" style={{ width: '70%', padding: '8px' }} />
                    <button onClick={handleAddCategory}>登録</button>
                </section>

                <section style={{ flex: 1, backgroundColor: '#f0f7ff', padding: '15px', borderRadius: '8px' }}>
                    <h3>種目追加</h3>
                    <select value={selectedCategoryId} onChange={e => setSelectedCategoryId(Number(e.target.value))} style={{ width: '100%', marginBottom: '10px' }}>
                        <option value="0">部位を選択</option>
                        {categories.map(cat => (
                            <option key={cat.category_Id} value={cat.category_Id}>{cat.category_Name}</option>
                        ))}
                    </select>
                    <input type="text" value={newExerciseName} onChange={e => setNewExerciseName(e.target.value)} maxLength={50} required placeholder="種目名を入力（50文字以内）" style={{ width: '70%', padding: '8px' }} />
                    <button onClick={handleAddExercise}>登録</button>
                </section>
            </div>

            <hr />

            {/* --- 4. 管理・削除セクション --- */}
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
                            {exercises.map(ex => {
                                // データが綺麗に統一されたため、大文字小文字の複雑な判定（??）は不要になりました
                                const parentCategory = categories.find(c => c.category_Id === ex.category_Id);
                                const parentName = parentCategory?.category_Name || "未分類";

                                return (
                                    <li key={ex.exercise_Id} style={{ display: 'flex', justifyContent: 'space-between', padding: '8px', borderBottom: '1px solid #eee' }}>
                                        <span>
                                            <small style={{ color: '#666' }}>[{parentName}]</small> {ex.exercise_Name}
                                        </span>
                                        <button onClick={() => handleDeleteExercise(ex.exercise_Id)} style={{ color: 'red', border: 'none', background: 'none', cursor: 'pointer' }}>削除</button>
                                    </li>
                                );
                            })}
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SettingsPage;