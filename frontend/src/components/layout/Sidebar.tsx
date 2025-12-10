import { NavLink } from 'react-router-dom';
import { Home, Users, MessageSquare, Zap } from 'lucide-react';
import styles from './Sidebar.module.css';

const navItems = [
  { path: '/', icon: Home, label: 'Dashboard' },
  { path: '/members', icon: Users, label: 'Members' },
  { path: '/debates', icon: MessageSquare, label: 'Debates' },
];

export function Sidebar() {
  return (
    <aside className={styles.sidebar}>
      <div className={styles.logo}>
        <Zap className={styles.logoIcon} size={24} />
        <span className={styles.logoText}>LudusLogicae</span>
      </div>

      <nav className={styles.nav}>
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              `${styles.navLink} ${isActive ? styles.active : ''}`
            }
          >
            <item.icon size={20} />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className={styles.footer}>
        <p className={styles.footerText}>AI Debate Platform</p>
      </div>
    </aside>
  );
}
