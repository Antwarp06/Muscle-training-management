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

// --- アプリ起動時にデータを取得 ---
  useEffect(() => {
    // ⭕ 【新機能】データが空っぽ（[]）だった場合、1秒待って指定回数やり直す関数
    const fetchWithRetry = async (url: string, maxRetries: number = 3) => {
      for (let i = 0; i < maxRetries; i++) {
        const res = await fetch(url);
        if (!res.ok) throw new Error("通信失敗");
        const data = await res.json();

        // データが1件でも入っていれば、ループを抜けて即座にデータを返す
        if (data && data.length > 0) {
          return data;
        }

        // 空っぽだった場合、コンソールに警告を出して1秒待機
        console.warn(`[リトライ中 ${i + 1}/${maxRetries}] データが空でした: ${url}`);
        await new Promise(resolve => setTimeout(resolve, 1000));
      }
      return []; // 指定回数やってもダメなら、諦めて空を返す
    };

    const fetchMasterData = async () => {
      try {
        // ⭕ 先ほど作ったリトライ関数を使って取得する
        const catData = await fetchWithRetry('https://muscle-training-management.onrender.com/api/MasterData/categories');
        const exData = await fetchWithRetry('https://muscle-training-management.onrender.com/api/MasterData/exercises');

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
      } finally {
        setIsLoading(false);
      }
    };

    fetchMasterData();
  }, []);

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