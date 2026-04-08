import { NavLink } from 'react-router-dom';
import styles from './Navigation.module.css';

const navItems = [
  { path: '/cpr', label: 'CPR Training', icon: '❤' },
  { path: '/monitor', label: 'Patient Monitor', icon: '📊' },
  { path: '/analytics', label: 'Analytics', icon: '📈' },
  { path: '/learners', label: 'Learners', icon: '👥' },
];

export default function Navigation() {
  return (
    <nav className={styles.nav}>
      <div className={styles.brand}>
        <span className={styles.logo}>⚕</span>
        <span className={styles.title}>SimTrainer</span>
      </div>
      <ul className={styles.links}>
        {navItems.map((item) => (
          <li key={item.path}>
            <NavLink
              to={item.path}
              className={({ isActive }) =>
                `${styles.link} ${isActive ? styles.active : ''}`
              }
            >
              <span className={styles.icon}>{item.icon}</span>
              {item.label}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}
