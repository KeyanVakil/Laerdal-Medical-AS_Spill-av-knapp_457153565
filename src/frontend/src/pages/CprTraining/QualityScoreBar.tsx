import styles from './CprTraining.module.css';

interface QualityScoreBarProps {
  score: number;
  label: string;
}

export default function QualityScoreBar({ score, label }: QualityScoreBarProps) {
  const getColor = (s: number) => {
    if (s >= 80) return 'var(--success)';
    if (s >= 50) return 'var(--warning)';
    return 'var(--danger)';
  };

  return (
    <div className={styles.scoreBar}>
      <div className={styles.scoreLabel}>
        <span>{label}</span>
        <span style={{ color: getColor(score) }}>{score.toFixed(0)}%</span>
      </div>
      <div className={styles.barTrack}>
        <div
          className={styles.barFill}
          style={{ width: `${Math.min(100, score)}%`, backgroundColor: getColor(score) }}
        />
      </div>
    </div>
  );
}
