import styles from './PatientMonitor.module.css';

const SCENARIOS = ['Normal', 'Tachycardia', 'Bradycardia', 'Hypoxia', 'Cardiac Arrest'];

interface ScenarioSelectorProps {
  activeScenario: string;
  onSelect: (scenario: string) => void;
  disabled: boolean;
}

export default function ScenarioSelector({ activeScenario, onSelect, disabled }: ScenarioSelectorProps) {
  return (
    <div className={styles.scenarioPanel}>
      {SCENARIOS.map((scenario) => (
        <button
          key={scenario}
          className={`${styles.scenarioBtn} ${activeScenario === scenario ? styles.scenarioBtnActive : ''}`}
          onClick={() => onSelect(scenario)}
          disabled={disabled}
        >
          {scenario}
        </button>
      ))}
    </div>
  );
}
