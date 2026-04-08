import { useState, useEffect, useCallback } from 'react';
import { api } from '../../api/client';
import { monitorConnection, startConnection } from '../../api/signalr';
import type { VitalSnapshot, Learner } from '../../types';
import VitalDisplay from './VitalDisplay';
import ScenarioSelector from './ScenarioSelector';
import AlarmOverlay from './AlarmOverlay';
import styles from './PatientMonitor.module.css';

const HISTORY_SIZE = 30;

export default function PatientMonitorPage() {
  const [learners, setLearners] = useState<Learner[]>([]);
  const [selectedLearnerId, setSelectedLearnerId] = useState('');
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [isRunning, setIsRunning] = useState(false);
  const [activeScenario, setActiveScenario] = useState('Normal');
  const [alarmActive, setAlarmActive] = useState(false);
  const [vitals, setVitals] = useState<VitalSnapshot | null>(null);
  const [hrHistory, setHrHistory] = useState<number[]>([]);
  const [spo2History, setSpo2History] = useState<number[]>([]);
  const [bpHistory, setBpHistory] = useState<number[]>([]);
  const [rrHistory, setRrHistory] = useState<number[]>([]);

  useEffect(() => {
    api.learners.list().then(setLearners).catch(console.error);
  }, []);

  const handleVitals = useCallback((v: VitalSnapshot) => {
    setVitals(v);
    setHrHistory(prev => [...prev.slice(-(HISTORY_SIZE - 1)), v.heartRate]);
    setSpo2History(prev => [...prev.slice(-(HISTORY_SIZE - 1)), v.spO2]);
    setBpHistory(prev => [...prev.slice(-(HISTORY_SIZE - 1)), v.systolicBp]);
    setRrHistory(prev => [...prev.slice(-(HISTORY_SIZE - 1)), v.respiratoryRate]);
  }, []);

  const handleAlarm = useCallback((_type: string) => {
    setAlarmActive(true);
  }, []);

  const handleScenarioChanged = useCallback((name: string) => {
    setActiveScenario(name);
    if (name !== 'Cardiac Arrest') setAlarmActive(false);
  }, []);

  useEffect(() => {
    monitorConnection.on('ReceiveVitals', handleVitals);
    monitorConnection.on('AlarmTriggered', handleAlarm);
    monitorConnection.on('ScenarioChanged', handleScenarioChanged);

    return () => {
      monitorConnection.off('ReceiveVitals', handleVitals);
      monitorConnection.off('AlarmTriggered', handleAlarm);
      monitorConnection.off('ScenarioChanged', handleScenarioChanged);
    };
  }, [handleVitals, handleAlarm, handleScenarioChanged]);

  const startMonitoring = async () => {
    if (!selectedLearnerId) return;

    const session = await api.sessions.start({
      learnerId: selectedLearnerId,
      sessionType: 'PatientMonitoring',
    });

    setSessionId(session.id);
    setIsRunning(true);
    setHrHistory([]);
    setSpo2History([]);
    setBpHistory([]);
    setRrHistory([]);
    setAlarmActive(false);
    setActiveScenario('Normal');

    await startConnection(monitorConnection);
    await monitorConnection.invoke('StartMonitoring', session.id);
  };

  const stopMonitoring = async () => {
    if (!sessionId) return;
    await monitorConnection.invoke('StopMonitoring', sessionId);
    await api.sessions.end(sessionId);
    setIsRunning(false);
    setAlarmActive(false);
  };

  const changeScenario = async (scenario: string) => {
    if (!sessionId) return;
    await api.scenarios.activate({ sessionId, scenarioName: scenario });
    setActiveScenario(scenario);
    if (scenario !== 'Cardiac Arrest') setAlarmActive(false);
  };

  return (
    <div className={styles.container}>
      <AlarmOverlay active={alarmActive} />

      <div className="page-header">
        <h1>Patient Monitor</h1>
        <p>Simulated bedside monitor with real-time vital signs and scenario control</p>
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
          <button className="btn-primary" onClick={startMonitoring} disabled={!selectedLearnerId}>
            Start Monitoring
          </button>
        ) : (
          <button className="btn-danger" onClick={stopMonitoring}>
            Stop Monitoring
          </button>
        )}
        {isRunning && (
          <span className={styles.scenarioLabel}>
            Active Scenario: <strong>{activeScenario}</strong>
          </span>
        )}
      </div>

      {isRunning && (
        <ScenarioSelector
          activeScenario={activeScenario}
          onSelect={changeScenario}
          disabled={!isRunning}
        />
      )}

      <div className={styles.monitorGrid}>
        <VitalDisplay
          name="Heart Rate"
          value={vitals?.heartRate ?? '--'}
          unit="bpm"
          color="var(--monitor-green)"
          history={hrHistory}
          isAlarm={alarmActive}
        />
        <VitalDisplay
          name="SpO2"
          value={vitals?.spO2 ?? '--'}
          unit="%"
          color="#00bfff"
          history={spo2History}
          isAlarm={vitals !== null && vitals.spO2 < 90}
        />
        <VitalDisplay
          name="Blood Pressure"
          value={vitals ? `${vitals.systolicBp}/${vitals.diastolicBp}` : '--/--'}
          unit="mmHg"
          color="#ff6b6b"
          history={bpHistory}
        />
        <VitalDisplay
          name="Respiratory Rate"
          value={vitals?.respiratoryRate ?? '--'}
          unit="br/min"
          color="#ffd93d"
          history={rrHistory}
        />
      </div>
    </div>
  );
}
