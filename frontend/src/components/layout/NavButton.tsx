import { useState, useRef, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Menu, X, Home, Users, MessageSquare, ChevronLeft } from 'lucide-react';
import anime from 'animejs';
import styles from './NavButton.module.css';

interface BreadcrumbItem {
  label: string;
  path: string;
}

export function NavButton() {
  const [isOpen, setIsOpen] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const menuRef = useRef<HTMLDivElement>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);

  const navItems = [
    { path: '/', icon: Home, label: 'Dashboard' },
    { path: '/members', icon: Users, label: 'Members' },
    { path: '/debates', icon: MessageSquare, label: 'Debates' },
  ];

  const getBreadcrumbs = (): BreadcrumbItem[] => {
    const pathParts = location.pathname.split('/').filter(Boolean);
    const breadcrumbs: BreadcrumbItem[] = [];

    let currentPath = '';
    pathParts.forEach((part) => {
      currentPath += `/${part}`;
      if (part.length < 30) {
        breadcrumbs.push({
          label: part.charAt(0).toUpperCase() + part.slice(1),
          path: currentPath,
        });
      }
    });

    return breadcrumbs;
  };

  const breadcrumbs = getBreadcrumbs();
  const canGoBack = location.pathname !== '/';

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (
        menuRef.current &&
        buttonRef.current &&
        !menuRef.current.contains(e.target as Node) &&
        !buttonRef.current.contains(e.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    if (menuRef.current) {
      if (isOpen) {
        anime({
          targets: menuRef.current,
          opacity: [0, 1],
          translateY: [-10, 0],
          duration: 200,
          easing: 'easeOutCubic',
        });
        anime({
          targets: menuRef.current.querySelectorAll(`.${styles.menuItem}`),
          opacity: [0, 1],
          translateX: [-10, 0],
          delay: anime.stagger(50, { start: 100 }),
          duration: 200,
          easing: 'easeOutCubic',
        });
      }
    }
  }, [isOpen]);

  const handleNavigate = (path: string) => {
    navigate(path);
    setIsOpen(false);
  };

  const handleBack = () => {
    navigate(-1);
  };

  return (
    <div className={styles.navContainer}>
      {canGoBack && (
        <button className={styles.backButton} onClick={handleBack}>
          <ChevronLeft size={20} />
        </button>
      )}

      {breadcrumbs.length > 0 && (
        <nav className={styles.breadcrumbs}>
          {breadcrumbs.map((crumb, index) => (
            <span key={crumb.path}>
              {index > 0 && <span className={styles.separator}>/</span>}
              <button
                className={`${styles.breadcrumb} ${
                  index === breadcrumbs.length - 1 ? styles.current : ''
                }`}
                onClick={() => handleNavigate(crumb.path)}
              >
                {crumb.label}
              </button>
            </span>
          ))}
        </nav>
      )}

      <button
        ref={buttonRef}
        className={`${styles.menuButton} ${isOpen ? styles.open : ''}`}
        onClick={() => setIsOpen(!isOpen)}
        aria-label="Navigation menu"
      >
        {isOpen ? <X size={20} /> : <Menu size={20} />}
      </button>

      {isOpen && (
        <div ref={menuRef} className={styles.menu}>
          <div className={styles.menuHeader}>
            <span className={styles.logo}>LudusLogicae</span>
          </div>
          {navItems.map((item) => (
            <button
              key={item.path}
              className={`${styles.menuItem} ${
                item.path === '/'
                  ? location.pathname === '/' ? styles.active : ''
                  : location.pathname.startsWith(item.path) ? styles.active : ''
              }`}
              onClick={() => handleNavigate(item.path)}
            >
              <item.icon size={18} />
              <span>{item.label}</span>
            </button>
          ))}
          <div className={styles.menuFooter}>
            <span>AI Debate Platform</span>
          </div>
        </div>
      )}
    </div>
  );
}
