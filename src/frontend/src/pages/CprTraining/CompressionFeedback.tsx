import type { CompressionEvent } from '../../types';
import styles from './CprTraining.module.css';

interface CompressionFeedbackProps {
  compression: CompressionEvent | null;
}

export default function CompressionFeedback({ compression }: CompressionFeedbackProps) {
  if (!compression) {
    return (
      <div className={`card ${styles.feedbackCard}`}>
        <p className={styles.waitingText}>Waiting for compression data...</p>
      </div>
    );
  }

  const depthColor = compression.depthCm >= 5 && compression.depthCm <= 6 ? 'var(--success)' :
    compression.depthCm >= 4 && compression.depthCm <= 6.5 ? 'var(--warning)' : 'var(--danger)';

  const rateColor = compression.rateBpm >= 100 && compression.rateBpm <= 120 ? 'var(--success)' :
    compression.rateBpm >= 80 && compression.rateBpm <= 140 ? 'var(--warning)' : 'var(--danger)';

  return (
    <div className={`card ${styles.feedbackCard}`}>
      <h3>Last Compression</h3>
      <div className={styles.metrics}>
        <div className={styles.metric}>
          <span className={styles.metricValue} style={{ color: depthColor }}>
            {compression.depthCm.toFixed(1)} cm
          </span>
          <span className={styles.metricLabel}>
            {compression.depthFeedback || 'Depth'}
          </span>
        </div>
        <div className={styles.metric}>
          <span className={styles.metricValue} style={{ color: rateColor }}>
            {compression.rateBpm} bpm
          </span>
          <span className={styles.metricLabel}>
            {compression.rateFeedback || 'Rate'}
          </span>
        </div>
        <div className={styles.metric}>
          <span className={styles.metricValue} style={{ color: compression.fullRecoil ? 'var(--success)' : 'var(--danger)' }}>
            {compression.fullRecoil ? 'Yes' : 'No'}
          </span>
          <span className={styles.metricLabel}>Full Recoil</span>
        </div>
        <div className={styles.metric}>
          <span className={styles.metricValue}>
            {compression.qualityScore.toFixed(0)}%
          </span>
          <span className={styles.metricLabel}>Quality</span>
        </div>
      </div>
    </div>
  );
}
