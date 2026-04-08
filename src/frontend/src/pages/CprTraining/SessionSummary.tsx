import type { CompressionEvent } from '../../types';
import styles from './CprTraining.module.css';

interface SessionSummaryProps {
  compressions: CompressionEvent[];
  overallScore: number;
}

export default function SessionSummary({ compressions, overallScore }: SessionSummaryProps) {
  if (compressions.length === 0) return null;

  const avgDepth = compressions.reduce((sum, c) => sum + c.depthCm, 0) / compressions.length;
  const avgRate = compressions.reduce((sum, c) => sum + c.rateBpm, 0) / compressions.length;
  const recoilRate = compressions.filter(c => c.fullRecoil).length / compressions.length * 100;

  return (
    <div className={`card ${styles.summary}`}>
      <h3>Session Summary</h3>
      <div className={styles.summaryGrid}>
        <div className={styles.summaryItem}>
          <span className={styles.summaryValue}>{compressions.length}</span>
          <span className={styles.summaryLabel}>Total Compressions</span>
        </div>
        <div className={styles.summaryItem}>
          <span className={styles.summaryValue}>{overallScore.toFixed(1)}%</span>
          <span className={styles.summaryLabel}>Overall Score</span>
        </div>
        <div className={styles.summaryItem}>
          <span className={styles.summaryValue}>{avgDepth.toFixed(1)} cm</span>
          <span className={styles.summaryLabel}>Avg Depth</span>
        </div>
        <div className={styles.summaryItem}>
          <span className={styles.summaryValue}>{avgRate.toFixed(0)} bpm</span>
          <span className={styles.summaryLabel}>Avg Rate</span>
        </div>
        <div className={styles.summaryItem}>
          <span className={styles.summaryValue}>{recoilRate.toFixed(0)}%</span>
          <span className={styles.summaryLabel}>Recoil Rate</span>
        </div>
      </div>
    </div>
  );
}
