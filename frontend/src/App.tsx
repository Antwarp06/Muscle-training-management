import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import WorkoutPage from './pages/WorkoutPage';
import ChartPage from './pages/ChartPage';
import SettingPage from './pages/SettingPage';

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

function App() {
  // アプリ全体で共有するデータ（実家の金庫）
  const [categories, setCategories] = useState<Category[]>([]);
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // --- アプリ起動時に1回だけデータを取得 ---
  useEffect(() => {
    const fetchMasterData = async () => {
      try {
        // 無料DBがパンクしないよう、順番に（直列で）取得します
        const catRes = await fetch('https://muscle-training-management.onrender.com/api/MasterData/categories');
        if (!catRes.ok) throw new Error("部位の取得失敗");
        const catData = await catRes.json();

        const exRes = await fetch('https://muscle-training-management.onrender.com/api/MasterData/exercises');
        if (!exRes.ok) throw new Error("種目の取得失敗");
        const exData = await exRes.json();

        // データの型と名前を綺麗な小文字に統一
        const cleanCategories = catData.map((c: any) => ({
          category_Id: Number(c.category_Id ?? c.Category_Id) || 0,
          category_Name: String(c.category_Name ?? c.Category_Name) || ""
        }));

        const cleanExercises = exData.map((e: any) => ({
          exercise_Id: Number(e.exercise_Id ?? e.Exercise_Id) || 0,
          exercise_Name: String(e.exercise_Name ?? e.Exercise_Name) || "",
          category_Id: Number(e.category_Id ?? e.Category_Id) || 0
        }));

        setCategories(cleanCategories);
        setExercises(cleanExercises);

      } catch (error) {
        console.error("初期データの読み込みエラー:", error);
        alert("データの取得に失敗しました。画面をリロードしてください。");
      } finally {
        setIsLoading(false);
      }
    };

    fetchMasterData();
  }, []); // 空の配列 `[]` が「最初の一回だけ」を保証します

  return (
    <Router>
      <div style={{ fontFamily: 'sans-serif' }}>
        {/* ナビゲーション */}
        <nav style={{ display: 'flex', gap: '15px', padding: '10px', background: '#eee' }}>
          <Link to="/">記録入力</Link>
          <Link to="/charts">グラフ</Link>
          <Link to="/settings">種目管理</Link>
        </nav>

        {/* 画面の切り替え設定 */}
        <Routes>
          {/* 各ページに、実家で取得したデータ（仕送り）を渡す */}
          <Route path="/" element={
            <WorkoutPage 
              categories={categories} 
              exercises={exercises} 
              isLoading={isLoading} 
            />
          } />
          
          <Route path="/charts" element={
            <ChartPage 
              categories={categories} 
              exercises={exercises} 
              isLoading={isLoading} 
            />
          } />
          
          <Route path="/settings" element={
            <SettingPage 
              categories={categories} 
              setCategories={setCategories} // 追加・削除時に実家のデータを直接書き換えるためのスイッチ
              exercises={exercises} 
              setExercises={setExercises} 
              isLoading={isLoading} 
            />
          } />
        </Routes>
      </div>
    </Router>
  );
}

export default App;