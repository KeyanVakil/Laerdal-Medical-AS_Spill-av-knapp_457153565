import { useState, useEffect, useCallback } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { api } from '../../api/client';
import { cprConnection, startConnection } from '../../api/signalr';
import type { CompressionEvent, Learner } from '../../types';
import CompressionFeedback from './CompressionFeedback';
import QualityScoreBar from './QualityScoreBar';
import SessionSummary from './SessionSummary';
import styles from './CprTraining.module.css';

export default function CprTrainingPage() {
  const [learners, setLearners] = useState<Learner[]>([]);
  const [selectedLearnerId, setSelectedLearnerId] = useState('');
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [isRunning, setIsRunning] = useState(false);
  const [compressions, setCompressions] = useState<CompressionEvent[]>([]);
  const [latestCompression, setLatestCompression] = useState<CompressionEvent | null>(null);
  const [overallScore, setOverallScore] = useState(0);

  useEffect(() => {
    api.learners.list().then(setLearners).catch(console.error);
  }, []);

  const handleCompression = useCallback((compression: CompressionEvent) => {
    setLatestCompression(compression);
    setCompressions(prev => [...prev.slice(-99), compression]);
  }, []);

  const handleScore = useCallback((score: number) => {
    setOverallScore(score);
  }, []);

  useEffect(() => {
    cprConnection.on('ReceiveCompression', handleCompression);
    cprConnection.on('SessionScore', handleScore);

    return () => {
      cprConnection.off('ReceiveCompression', handleCompression);
      cprConnection.off('SessionScore', handleScore);
    };
  }, [handleCompression, handleScore]);

  const startSession = async () => {
    if (!selectedLearnerId) return;

    const session = await api.sessions.start({
      learnerId: selectedLearnerId,
      sessionType: 'CprTraining',
    });

    setSessionId(session.id);
    setCompressions([]);
    setLatestCompression(null);
    setOverallScore(0);
    setIsRunning(true);

    await startConnection(cprConnection);
    await cprConnection.invoke('StartSession', session.id);
  };

  const stopSession = async () => {
    if (!sessionId) return;

    await cprConnection.invoke('StopSession', sessionId);
    await api.sessions.end(sessionId);
    setIsRunning(false);
  };

  const chartData = compressions.map((c, i) => ({
    index: i + 1,
    depth: c.depthCm,
    rate: c.rateBpm,
    quality: c.qualityScore,
  }));

  return (
    <div className={styles.container}>
      <div className="page-header">
        <h1>CPR Training</h1>
        <p>Real-time compression quality feedback against AHA guidelines</p>
      </div>

      <div className={styles.controls}>
        <select
          value={selectedLearnerId}
          onChange={(e) => setSelectedLearnerId(e.target.value)}
          disabled={isRunning}
        >
          <option value="">Select a learner...</option>
          {learners.map((l) => (
            <option key={l.id} value={l.id}>{l.name} ({l.role})</option>
          ))}
        </select>
        {!isRunning ? (
          <button className="btn-primary" onClick={startSession} disabled={!selectedLearnerId}>
            Start Session
          </button>
        ) : (
          <button className="btn-danger" onClick={stopSession}>
            Stop Session
          </button>
        )}
        {isRunning && (
          <span style={{ color: 'var(--success)', fontSize: '14px' }}>
            Session active — compressions: {compressions.length}
          </span>
        )}
      </div>

      <div className={styles.mainGrid}>
        <CompressionFeedback compression={latestCompression} />
        <div className="card">
          <h3>Quality Scores</h3>
          <div style={{ marginTop: 16 }}>
            <QualityScoreBar score={overallScore} label="Overall" />
            {latestCompression && (
              <>
                <QualityScoreBar
                  score={latestCompression.depthCm >= 5 && latestCompression.depthCm <= 6 ? 100 :
                    Math.max(0, 100 - Math.abs(5.5 - latestCompression.depthCm) * 60)}
                  label="Depth"
                />
                <QualityScoreBar
                  score={latestCompression.rateBpm >= 100 && latestCompression.rateBpm <= 120 ? 100 :
                    Math.max(0, 100 - Math.abs(110 - latestCompression.rateBpm) * 2.5)}
                  label="Rate"
                />
              </>
            )}
          </div>
        </div>
      </div>

      {chartData.length > 1 && (
        <div className="card">
          <h3>Compression Quality Over Time</h3>
          <div className={styles.chartContainer}>
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis dataKey="index" stroke="var(--text-secondary)" />
                <YAxis stroke="var(--text-secondary)" domain={[0, 100]} />
                <Tooltip
                  contentStyle={{ backgroundColor: 'var(--bg-secondary)', border: '1px solid var(--border)' }}
                />
                <Line type="monotone" dataKey="quality" stroke="var(--accent)" strokeWidth={2} dot={false} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}

      {!isRunning && compressions.length > 0 && (
        <SessionSummary compressions={compressions} overallScore={overallScore} />
      )}
    </div>
  );
}
