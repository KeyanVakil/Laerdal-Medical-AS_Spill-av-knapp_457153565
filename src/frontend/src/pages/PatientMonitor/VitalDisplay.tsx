import { LineChart, Line, ResponsiveContainer } from 'recharts';
import styles from './PatientMonitor.module.css';

interface VitalDisplayProps {
  name: string;
  value: number | string;
  unit: string;
  color: string;
  history: number[];
  isAlarm?: boolean;
}

export default function VitalDisplay({ name, value, unit, color, history, isAlarm }: VitalDisplayProps) {
  const chartData = history.map((v, i) => ({ i, v }));

  return (
    <div className={styles.vitalCard} style={{ borderColor: isAlarm ? 'var(--danger)' : undefined }}>
      <div className={styles.vitalHeader}>
        <span className={styles.vitalName} style={{ color }}>{name}</span>
        <span className={styles.vitalUnit}>{unit}</span>
      </div>
      <div className={styles.waveformContainer}>
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData}>
            <Line
              type="monotone"
              dataKey="v"
              stroke={color}
              strokeWidth={2}
              dot={false}
              isAnimationActive={false}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
      <div className={styles.vitalValue} style={{ color }}>
        {value}
      </div>
    </div>
  );
}
