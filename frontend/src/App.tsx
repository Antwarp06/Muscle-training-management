import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import WorkoutPage from './pages/WorkoutPage';
import ChartPage from './pages/ChartPage';
import SettingPage from './pages/SettingPage';

function App() {
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
          <Route path="/" element={<WorkoutPage />} />
          <Route path="/charts" element={<ChartPage />} />
          <Route path="/settings" element={<SettingPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;