import styles from './PatientMonitor.module.css';

interface AlarmOverlayProps {
  active: boolean;
}

export default function AlarmOverlay({ active }: AlarmOverlayProps) {
  if (!active) return null;
  return <div className={styles.alarmOverlay} />;
}
