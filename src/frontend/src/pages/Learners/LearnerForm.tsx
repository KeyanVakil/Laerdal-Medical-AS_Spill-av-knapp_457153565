import { useState } from 'react';
import styles from './Learners.module.css';

interface LearnerFormProps {
  onSubmit: (name: string, role: string) => void;
}

const ROLES = ['Nurse', 'Doctor', 'Paramedic', 'Medical Student', 'Resident', 'Instructor'];

export default function LearnerForm({ onSubmit }: LearnerFormProps) {
  const [name, setName] = useState('');
  const [role, setRole] = useState(ROLES[0]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    onSubmit(name.trim(), role);
    setName('');
  };

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <div className={styles.field}>
        <label>Name</label>
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Full name"
          required
        />
      </div>
      <div className={styles.field}>
        <label>Role</label>
        <select value={role} onChange={(e) => setRole(e.target.value)}>
          {ROLES.map((r) => (
            <option key={r} value={r}>{r}</option>
          ))}
        </select>
      </div>
      <button className="btn-primary" type="submit">Add Learner</button>
    </form>
  );
}
