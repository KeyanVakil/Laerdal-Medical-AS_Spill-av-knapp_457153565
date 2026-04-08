import styles from './Analytics.module.css';

interface SuggestionsProps {
  suggestions: string[];
}

export default function Suggestions({ suggestions }: SuggestionsProps) {
  if (suggestions.length === 0) return null;

  return (
    <div className="card">
      <h3>Improvement Suggestions</h3>
      <ul className={styles.suggestions}>
        {suggestions.map((s, i) => (
          <li key={i} className={styles.suggestion}>{s}</li>
        ))}
      </ul>
    </div>
  );
}
