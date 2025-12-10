import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Layout } from './components/layout';
import { Dashboard, MembersPage, MemberDetailPage, DebatesPage, DebateDetailPage } from './pages';
import { DebateReplayPage } from './pages/debates/DebateReplayPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="members" element={<MembersPage />} />
          <Route path="members/:id" element={<MemberDetailPage />} />
          <Route path="debates" element={<DebatesPage />} />
          <Route path="debates/:id" element={<DebateDetailPage />} />
          <Route path="debates/:id/replay" element={<DebateReplayPage />} />
        </Route>
        <Route path="dashboard" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
