import { useEffect, useRef } from 'react';
import anime from 'animejs';
import styles from './Loading.module.css';

interface LoadingProps {
  size?: 'sm' | 'md' | 'lg';
  text?: string;
}

export function Loading({ size = 'md', text }: LoadingProps) {
  const dotsRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (dotsRef.current) {
      const dots = dotsRef.current.querySelectorAll(`.${styles.dot}`);

      anime({
        targets: dots,
        scale: [1, 1.3, 1],
        opacity: [0.5, 1, 0.5],
        duration: 800,
        delay: anime.stagger(150),
        loop: true,
        easing: 'easeInOutSine',
      });
    }
  }, []);

  return (
    <div className={`${styles.wrapper} ${styles[size]}`}>
      <div ref={dotsRef} className={styles.dots}>
        <span className={styles.dot} />
        <span className={styles.dot} />
        <span className={styles.dot} />
      </div>
      {text && <p className={styles.text}>{text}</p>}
    </div>
  );
}

interface LoadingOverlayProps {
  text?: string;
}

export function LoadingOverlay({ text = 'Loading...' }: LoadingOverlayProps) {
  return (
    <div className={styles.overlay}>
      <Loading size="lg" text={text} />
    </div>
  );
}

interface LoadingSkeletonProps {
  width?: string;
  height?: string;
  borderRadius?: string;
}

export function LoadingSkeleton({
  width = '100%',
  height = '1rem',
  borderRadius = 'var(--radius-md)',
}: LoadingSkeletonProps) {
  return (
    <div
      className={styles.skeleton}
      style={{ width, height, borderRadius }}
    />
  );
}
