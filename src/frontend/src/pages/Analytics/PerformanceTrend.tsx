import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { LearnerAnalytics } from '../../types';
import styles from './Analytics.module.css';

interface PerformanceTrendProps {
  analytics: LearnerAnalytics;
}

export default function PerformanceTrend({ analytics }: PerformanceTrendProps) {
  const trendColor = analytics.recentTrend === 'improving' ? 'var(--success)' :
    analytics.recentTrend === 'declining' ? 'var(--danger)' : 'var(--warning)';

  return (
    <div className="card">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3>Performance Trend</h3>
        <span className={styles.trendBadge} style={{ backgroundColor: trendColor, color: '#000' }}>
          {analytics.recentTrend}
        </span>
      </div>
      <div className={styles.statGrid}>
        <div className={styles.stat}>
          <div className={styles.statValue}>{analytics.totalSessions}</div>
          <div className={styles.statLabel}>Total Sessions</div>
        </div>
        <div className={styles.stat}>
          <div className={styles.statValue}>{analytics.averageScore.toFixed(1)}%</div>
          <div className={styles.statLabel}>Average Score</div>
        </div>
        <div className={styles.stat}>
          <div className={styles.statValue}>{analytics.bestScore.toFixed(1)}%</div>
          <div className={styles.statLabel}>Best Score</div>
        </div>
      </div>
      {analytics.sessionScores.length > 1 && (
        <div className={styles.chartContainer}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={analytics.sessionScores}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="date" stroke="var(--text-secondary)" />
              <YAxis stroke="var(--text-secondary)" domain={[0, 100]} />
              <Tooltip contentStyle={{ backgroundColor: 'var(--bg-secondary)', border: '1px solid var(--border)' }} />
              <Line type="monotone" dataKey="score" stroke="var(--accent)" strokeWidth={2} dot />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}
