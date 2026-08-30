import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import WorkoutPage from './pages/WorkoutPage';
import ChartPage from './pages/ChartPage';
import SettingPage from './pages/SettingPage';
import CardioPage from './pages/CardioPage';
import LoginPage from './pages/LoginPage';
import ErrorRetry from './components/ErrorRetry';
import { apiFetch, getUserName, clearAuth, setUnauthorizedHandler } from './api';

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
  // ログイン状態。起動時に localStorage を見て、前回のログインを引き継ぐ
  const [userName, setUserName] = useState<string | null>(getUserName());

  // アプリ全体で共有するデータ（実家の金庫）
  const [categories, setCategories] = useState<Category[]>([]);
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState(false); // 通信エラーが起きたかどうかのフラグ

  // トークンが切れて401が返ったら、どのページにいてもログイン画面へ戻す
  useEffect(() => {
    setUnauthorizedHandler(() => {
      setUserName(null);
      setCategories([]);
      setExercises([]);
    });
    return () => setUnauthorizedHandler(null);
  }, []);

  // 「再度実行」ボタンからも呼び出せるように、useEffect の外に定義します
  const fetchMasterData = async () => {
    setIsLoading(true);
    setLoadError(false); // 再実行するときは、前回のエラー表示をいったんリセット
    try {
      // 以前は「空だったら1秒待って再取得」を3回繰り返していたが、
      // 登録直後のユーザーは部位も種目も0件が正常なため、待たせるだけになる。
      // 空かどうかではなく、通信が成功したかどうかで判断する形に変更した。
      const [catRes, exRes] = await Promise.all([
        apiFetch('/api/MasterData/categories'),
        apiFetch('/api/MasterData/exercises')
      ]);
      if (!catRes.ok || !exRes.ok) throw new Error('通信失敗');

      const catData = await catRes.json();
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
      // 401の場合は上の setUnauthorizedHandler がログイン画面へ戻すので、
      // ここでエラー画面を出す必要はない
      console.error("初期データの読み込みエラー:", error);
      setLoadError(true); // エラー画面を表示するためのスイッチをON
    } finally {
      setIsLoading(false);
    }
  };

  // --- ログイン後にデータを取得 ---
  useEffect(() => {
    if (userName) fetchMasterData();
  }, [userName]);

  const handleLogout = () => {
    clearAuth();
    setUserName(null);
    setCategories([]);
    setExercises([]);
  };

  // 未ログインならログイン画面だけを表示する。
  // 以降のページはすべてこの下にあるので、ログインしないと到達できない。
  if (!userName) {
    return <LoginPage onSuccess={(name) => setUserName(name)} />;
  }

  // 通信エラー時は、アプリ全体の代わりにエラー画面と「再度実行」ボタンを表示
  if (loadError) {
    return (
      <div style={{ fontFamily: 'sans-serif', display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <ErrorRetry onRetry={fetchMasterData} />
      </div>
    );
  }

  return (
    <Router>
      <div style={{ fontFamily: 'sans-serif' }}>
        {/* ナビゲーション */}
        <nav style={{ display: 'flex', gap: '15px', padding: '10px', background: '#eee', alignItems: 'center' }}>
          <Link to="/">記録入力</Link>
          <Link to="/cardio">有酸素</Link>
          <Link to="/charts">グラフ</Link>
          <Link to="/settings">種目管理</Link>

          {/* 右端にログイン中のユーザー名とログアウト */}
          <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: '12px' }}>
            <span style={{ fontSize: '0.85rem', color: '#555' }}>{userName} さん</span>
            <button
              onClick={handleLogout}
              style={{
                padding: '4px 12px', fontSize: '0.8rem', cursor: 'pointer',
                border: '1px solid #aaa', borderRadius: '4px', background: '#fff'
              }}>
              ログアウト
            </button>
          </div>
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

          <Route path="/cardio" element={<CardioPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
