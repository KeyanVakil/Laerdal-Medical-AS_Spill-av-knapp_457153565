import type { TrainingSession } from '../../types';
import styles from './Analytics.module.css';

interface SessionListProps {
  sessions: TrainingSession[];
  selectedId: string | null;
  onSelect: (id: string) => void;
}

export default function SessionList({ sessions, selectedId, onSelect }: SessionListProps) {
  if (sessions.length === 0) {
    return <p style={{ color: 'var(--text-secondary)' }}>No completed sessions found</p>;
  }

  return (
    <div className={styles.sessionList}>
      {sessions.map((s) => (
        <div
          key={s.id}
          className={`${styles.sessionItem} ${selectedId === s.id ? styles.sessionItemActive : ''}`}
          onClick={() => onSelect(s.id)}
        >
          <div>
            <span>{s.learnerName || 'Unknown'}</span>
            <span className={styles.sessionDate}>
              {' '} — {new Date(s.startedAt).toLocaleDateString()}
            </span>
          </div>
          <span
            className={styles.sessionScore}
            style={{
              color: (s.overallScore ?? 0) >= 80 ? 'var(--success)' :
                (s.overallScore ?? 0) >= 50 ? 'var(--warning)' : 'var(--danger)'
            }}
          >
            {s.overallScore?.toFixed(1) ?? '-'}%
          </span>
        </div>
      ))}
    </div>
  );
}
