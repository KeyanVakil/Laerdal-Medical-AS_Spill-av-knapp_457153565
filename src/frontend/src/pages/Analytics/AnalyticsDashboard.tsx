import { useState, useEffect } from 'react';
import { api } from '../../api/client';
import type { Learner, TrainingSession, LearnerAnalytics, SessionAnalytics } from '../../types';
import SessionList from './SessionList';
import PerformanceTrend from './PerformanceTrend';
import CompressionDetail from './CompressionDetail';
import Suggestions from './Suggestions';
import styles from './Analytics.module.css';

export default function AnalyticsDashboard() {
  const [learners, setLearners] = useState<Learner[]>([]);
  const [selectedLearnerId, setSelectedLearnerId] = useState('');
  const [sessions, setSessions] = useState<TrainingSession[]>([]);
  const [selectedSessionId, setSelectedSessionId] = useState<string | null>(null);
  const [learnerAnalytics, setLearnerAnalytics] = useState<LearnerAnalytics | null>(null);
  const [sessionAnalytics, setSessionAnalytics] = useState<SessionAnalytics | null>(null);

  useEffect(() => {
    api.learners.list().then(setLearners).catch(console.error);
  }, []);

  useEffect(() => {
    if (!selectedLearnerId) {
      setSessions([]);
      setLearnerAnalytics(null);
      return;
    }

    api.sessions.list({ learnerId: selectedLearnerId, type: 'CprTraining' })
      .then((s) => setSessions(s.filter(sess => sess.endedAt)))
      .catch(console.error);

    api.analytics.learner(selectedLearnerId)
      .then(setLearnerAnalytics)
      .catch(console.error);
  }, [selectedLearnerId]);

  useEffect(() => {
    if (!selectedSessionId) {
      setSessionAnalytics(null);
      return;
    }

    api.analytics.session(selectedSessionId)
      .then(setSessionAnalytics)
      .catch(console.error);
  }, [selectedSessionId]);

  return (
    <div className={styles.container}>
      <div className="page-header">
        <h1>Training Analytics</h1>
        <p>Performance trends, session details, and AI-driven improvement suggestions</p>
      </div>

      <div className={styles.filters}>
        <select
          value={selectedLearnerId}
          onChange={(e) => {
            setSelectedLearnerId(e.target.value);
            setSelectedSessionId(null);
          }}
        >
          <option value="">Select a learner...</option>
          {learners.map((l) => (
            <option key={l.id} value={l.id}>{l.name} ({l.role})</option>
          ))}
        </select>
      </div>

      {learnerAnalytics && <PerformanceTrend analytics={learnerAnalytics} />}

      {sessions.length > 0 && (
        <div className={styles.detailGrid}>
          <div className="card">
            <h3>Completed Sessions</h3>
            <div style={{ marginTop: 12 }}>
              <SessionList
                sessions={sessions}
                selectedId={selectedSessionId}
                onSelect={setSelectedSessionId}
              />
            </div>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
            {sessionAnalytics && <CompressionDetail analytics={sessionAnalytics} />}
          </div>
        </div>
      )}

      {(learnerAnalytics?.suggestions || sessionAnalytics?.suggestions) && (
        <Suggestions suggestions={sessionAnalytics?.suggestions ?? learnerAnalytics?.suggestions ?? []} />
      )}
    </div>
  );
}
