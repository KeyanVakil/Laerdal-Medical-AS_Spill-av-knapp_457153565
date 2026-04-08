import { useState, useEffect } from 'react';
import { api } from '../../api/client';
import type { Learner } from '../../types';
import LearnerForm from './LearnerForm';
import styles from './Learners.module.css';

export default function LearnersPage() {
  const [learners, setLearners] = useState<Learner[]>([]);

  const loadLearners = () => {
    api.learners.list().then(setLearners).catch(console.error);
  };

  useEffect(() => { loadLearners(); }, []);

  const handleCreate = async (name: string, role: string) => {
    await api.learners.create({ name, role });
    loadLearners();
  };

  const handleDelete = async (id: string) => {
    await api.learners.delete(id);
    loadLearners();
  };

  return (
    <div className={styles.container}>
      <div className="page-header">
        <h1>Learners</h1>
        <p>Manage learner profiles for training sessions</p>
      </div>

      <div className="card">
        <h3>Add New Learner</h3>
        <div style={{ marginTop: 12 }}>
          <LearnerForm onSubmit={handleCreate} />
        </div>
      </div>

      <div className="card">
        <h3>All Learners</h3>
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Name</th>
              <th>Role</th>
              <th>Sessions</th>
              <th>Avg Score</th>
              <th>Joined</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {learners.map((l) => (
              <tr key={l.id}>
                <td>{l.name}</td>
                <td>{l.role}</td>
                <td>{l.sessionCount ?? 0}</td>
                <td>{(l.averageScore ?? 0).toFixed(1)}%</td>
                <td>{new Date(l.createdAt).toLocaleDateString()}</td>
                <td>
                  <div className={styles.actions}>
                    <button className="btn-danger" onClick={() => handleDelete(l.id)}>
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {learners.length === 0 && (
              <tr>
                <td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-secondary)', padding: 32 }}>
                  No learners yet. Add one above to get started.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
