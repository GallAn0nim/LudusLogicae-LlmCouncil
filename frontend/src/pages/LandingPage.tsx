import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import anime from 'animejs';
import styles from './LandingPage.module.css';

export function LandingPage() {
  const [isOpening, setIsOpening] = useState(false);
  const navigate = useNavigate();
  const leftGateRef = useRef<HTMLDivElement>(null);
  const rightGateRef = useRef<HTMLDivElement>(null);
  const fogRef = useRef<HTMLDivElement>(null);
  const pagesRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const entered = sessionStorage.getItem('ludus-entered');
    if (entered === 'true') {
      navigate('/dashboard', { replace: true });
    }
  }, [navigate]);

  useEffect(() => {
    if (fogRef.current) {
      anime({
        targets: fogRef.current.children,
        translateX: ['-100%', '100%'],
        opacity: [0.3, 0.6, 0.3],
        duration: 20000,
        loop: true,
        easing: 'linear',
        delay: anime.stagger(5000),
      });
    }

    if (pagesRef.current) {
      anime({
        targets: pagesRef.current.children,
        translateX: () => anime.random(-50, 50),
        translateY: () => anime.random(-30, 30),
        rotate: () => anime.random(-20, 20),
        opacity: [0, 0.7, 0],
        duration: 8000,
        loop: true,
        delay: anime.stagger(2000),
        easing: 'easeInOutQuad',
      });
    }
  }, []);

  const handleEnter = () => {
    if (isOpening) return;
    setIsOpening(true);

    const timeline = anime.timeline({
      easing: 'easeInOutQuart',
      complete: () => {
        sessionStorage.setItem('ludus-entered', 'true');
        setTimeout(() => navigate('/dashboard'), 300);
      },
    });

    timeline
      .add({
        targets: leftGateRef.current,
        rotateY: -110,
        duration: 1500,
      })
      .add({
        targets: rightGateRef.current,
        rotateY: 110,
        duration: 1500,
      }, 0);
  };

  return (
    <div className={styles.landing} onClick={handleEnter}>
      <div ref={fogRef} className={styles.fogContainer}>
        <div className={styles.fogLayer} />
        <div className={styles.fogLayer} />
        <div className={styles.fogLayer} />
      </div>

      <div ref={pagesRef} className={styles.pagesContainer}>
        <div className={styles.flyingPage}>&#x1F4C4;</div>
        <div className={styles.flyingPage}>&#x1F4DC;</div>
        <div className={styles.flyingPage}>&#x1F4C3;</div>
      </div>

      <div className={styles.gateContainer}>
        <div className={`${styles.gatePillar} ${styles.left}`} />

        <div className={styles.gateWrapper}>
          <div ref={leftGateRef} className={`${styles.gate} ${styles.leftGate}`}>
            <div className={styles.gatePattern}>
              <div className={styles.gateBar} />
              <div className={styles.gateBar} />
              <div className={styles.gateBar} />
            </div>
            <span className={styles.gateName}>LUDUS</span>
          </div>

          <div ref={rightGateRef} className={`${styles.gate} ${styles.rightGate}`}>
            <div className={styles.gatePattern}>
              <div className={styles.gateBar} />
              <div className={styles.gateBar} />
              <div className={styles.gateBar} />
            </div>
            <span className={styles.gateName}>LOGICAE</span>
          </div>
        </div>

        <div className={`${styles.gatePillar} ${styles.right}`} />
      </div>

      {!isOpening && (
        <div className={styles.enterPrompt}>
          <span>Click to Enter</span>
        </div>
      )}

      <div className={styles.lizard}>&#x1F98E;</div>
    </div>
  );
}
