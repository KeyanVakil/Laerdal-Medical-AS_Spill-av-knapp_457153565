import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line } from 'recharts';
import type { SessionAnalytics } from '../../types';
import styles from './Analytics.module.css';

interface CompressionDetailProps {
  analytics: SessionAnalytics;
}

export default function CompressionDetail({ analytics }: CompressionDetailProps) {
  const depthData = [
    { name: 'Below 5cm', count: analytics.depthDistribution.below5, fill: 'var(--danger)' },
    { name: '5-6cm (Optimal)', count: analytics.depthDistribution.optimal, fill: 'var(--success)' },
    { name: 'Above 6cm', count: analytics.depthDistribution.above6, fill: 'var(--warning)' },
  ];

  const qualityOverTime = analytics.compressions.map((c, i) => ({
    index: i + 1,
    quality: c.qualityScore,
    depth: c.depthCm,
    rate: c.rateBpm,
  }));

  return (
    <>
      <div className="card">
        <h3>Session Detail</h3>
        <div className={styles.statGrid}>
          <div className={styles.stat}>
            <div className={styles.statValue}>{analytics.totalCompressions}</div>
            <div className={styles.statLabel}>Compressions</div>
          </div>
          <div className={styles.stat}>
            <div className={styles.statValue}>{analytics.averageDepth.toFixed(1)} cm</div>
            <div className={styles.statLabel}>Avg Depth</div>
          </div>
          <div className={styles.stat}>
            <div className={styles.statValue}>{analytics.averageRate} bpm</div>
            <div className={styles.statLabel}>Avg Rate</div>
          </div>
          <div className={styles.stat}>
            <div className={styles.statValue}>{analytics.recoilRate.toFixed(0)}%</div>
            <div className={styles.statLabel}>Recoil Rate</div>
          </div>
        </div>
      </div>

      {analytics.totalCompressions > 0 && (
        <>
          <div className="card">
            <h3>Depth Distribution</h3>
            <div className={styles.chartContainer}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={depthData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                  <XAxis dataKey="name" stroke="var(--text-secondary)" />
                  <YAxis stroke="var(--text-secondary)" />
                  <Tooltip contentStyle={{ backgroundColor: 'var(--bg-secondary)', border: '1px solid var(--border)' }} />
                  <Bar dataKey="count" fill="var(--accent)" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          <div className="card">
            <h3>Quality Over Time</h3>
            <div className={styles.chartContainer}>
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={qualityOverTime}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                  <XAxis dataKey="index" stroke="var(--text-secondary)" />
                  <YAxis stroke="var(--text-secondary)" domain={[0, 100]} />
                  <Tooltip contentStyle={{ backgroundColor: 'var(--bg-secondary)', border: '1px solid var(--border)' }} />
                  <Line type="monotone" dataKey="quality" stroke="var(--accent)" strokeWidth={2} dot={false} />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </div>
        </>
      )}
    </>
  );
}
