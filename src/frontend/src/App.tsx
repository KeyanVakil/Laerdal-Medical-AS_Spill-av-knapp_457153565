import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import CprTrainingPage from './pages/CprTraining/CprTrainingPage';
import PatientMonitorPage from './pages/PatientMonitor/PatientMonitorPage';
import AnalyticsDashboard from './pages/Analytics/AnalyticsDashboard';
import LearnersPage from './pages/Learners/LearnersPage';

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/cpr" replace />} />
        <Route path="/cpr" element={<CprTrainingPage />} />
        <Route path="/monitor" element={<PatientMonitorPage />} />
        <Route path="/analytics" element={<AnalyticsDashboard />} />
        <Route path="/learners" element={<LearnersPage />} />
      </Routes>
    </Layout>
  );
}

export default App;
