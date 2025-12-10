import { ReactNode, useEffect, useRef } from 'react';
import anime from 'animejs';
import styles from './PageHeader.module.css';

interface PageHeaderProps {
  title: string;
  description?: string;
  action?: ReactNode;
}

export function PageHeader({ title, description, action }: PageHeaderProps) {
  const titleRef = useRef<HTMLHeadingElement>(null);

  useEffect(() => {
    if (titleRef.current) {
      anime({
        targets: titleRef.current,
        opacity: [0, 1],
        translateY: [-10, 0],
        duration: 400,
        easing: 'easeOutCubic',
      });
    }
  }, [title]);

  return (
    <header className={styles.header}>
      <div className={styles.content}>
        <h1 ref={titleRef} className={styles.title}>
          {title}
        </h1>
        {description && <p className={styles.description}>{description}</p>}
      </div>
      {action && <div className={styles.action}>{action}</div>}
    </header>
  );
}
