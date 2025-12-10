import { useEffect, useRef, useState } from 'react';
import anime from 'animejs';
import styles from './DebateLoadingOverlay.module.css';

const LOADING_MESSAGES = [
  "Members are introducing themselves to the council...",
  "Philosophers are stroking their beards thoughtfully...",
  "Someone just made a controversial claim that sparked debate...",
  "Heated argument in progress, tempers are flaring...",
  "Consulting ancient scrolls for wisdom and precedent...",
  "Members are dramatically pausing for effect...",
  "Egos are being checked at the door (with varying success)...",
  "Synthesizing opposing viewpoints into something coherent...",
  "Someone is playing devil's advocate just to be difficult...",
  "Logic is being weaponized against weak arguments...",
  "Forming temporary alliances that won't last the round...",
  "Respectfully agreeing to disagree on fundamental principles...",
  "Minds are being changed (allegedly, no confirmation yet)...",
  "Preparing devastating counterarguments in real-time...",
  "Citing sources of questionable academic rigor...",
  "Invoking ancient wisdom that may or may not apply here...",
  "Someone just said 'actually...' and everyone groaned...",
  "Diplomatic tensions rising, mediators standing by...",
  "Brewing an intellectual storm of unprecedented magnitude...",
  "Members are taking notes furiously, pens scratching...",
  "Consensus seems unlikely, but hope springs eternal...",
  "Someone is definitely overthinking this problem...",
  "Existential crisis in progress, please wait...",
  "Calculating Borda scores with mathematical precision...",
  "Final statements are being polished to perfection...",
];

type AnimationScene = 'talking' | 'fighting' | 'coffee' | 'writing' | 'thinking';

const SCENE_SEQUENCE: AnimationScene[] = ['thinking', 'writing', 'coffee', 'talking', 'fighting', 'thinking', 'writing', 'coffee', 'fighting'];

const ACCENT_COLORS = ['#818cf8', '#10b981', '#f59e0b', '#0ea5e9', '#f43f5e'];

export function DebateLoadingOverlay() {
  const [currentMessage, setCurrentMessage] = useState(LOADING_MESSAGES[0]);
  const [currentScene, setCurrentScene] = useState<AnimationScene>('thinking');
  const [opacity, setOpacity] = useState(1);
  const [colorIndex, setColorIndex] = useState(0);
  const sceneRef = useRef<HTMLDivElement>(null);
  const sceneIndexRef = useRef(0);

  // Rotate messages every 4-6 seconds (randomized)
  useEffect(() => {
    const shuffled = [...LOADING_MESSAGES].sort(() => Math.random() - 0.5);
    let index = 0;
    let timeoutId: ReturnType<typeof setTimeout>;

    const scheduleNext = () => {
      const delay = 4000 + Math.random() * 2000;
      timeoutId = setTimeout(() => {
        index = (index + 1) % shuffled.length;
        setCurrentMessage(shuffled[index]);
        scheduleNext();
      }, delay);
    };

    scheduleNext();
    return () => clearTimeout(timeoutId);
  }, []);

  // Rotate scenes every 8-12 seconds with smooth transition
  useEffect(() => {
    let timeoutId: ReturnType<typeof setTimeout>;

    const scheduleSceneChange = () => {
      const delay = 8000 + Math.random() * 4000;
      timeoutId = setTimeout(() => {
        // Fade out (600ms for smoother transition)
        setOpacity(0);

        // Change scene after fade out completes
        setTimeout(() => {
          sceneIndexRef.current = (sceneIndexRef.current + 1) % SCENE_SEQUENCE.length;
          setCurrentScene(SCENE_SEQUENCE[sceneIndexRef.current]);
          setColorIndex(prev => (prev + 1) % ACCENT_COLORS.length);

          // Fade in after brief pause
          setTimeout(() => {
            setOpacity(1);
          }, 100);
        }, 600);

        scheduleSceneChange();
      }, delay);
    };

    scheduleSceneChange();
    return () => clearTimeout(timeoutId);
  }, []);

  // Animate current scene
  useEffect(() => {
    if (!sceneRef.current || opacity === 0) return;

    // Small delay to let the DOM update
    const animationTimeout = setTimeout(() => {
      if (!sceneRef.current) return;

      const elements = sceneRef.current.querySelectorAll('[data-animate]');
      anime.remove(elements);

      switch (currentScene) {
        case 'talking':
          animateTalking(sceneRef.current);
          break;
        case 'fighting':
          animateFighting(sceneRef.current);
          break;
        case 'coffee':
          animateCoffee(sceneRef.current);
          break;
        case 'writing':
          animateWriting(sceneRef.current);
          break;
        case 'thinking':
          animateThinking(sceneRef.current);
          break;
      }
    }, 100);

    return () => clearTimeout(animationTimeout);
  }, [currentScene, opacity]);

  return (
    <div className={styles.overlay}>
      <div className={styles.content}>
        <div
          ref={sceneRef}
          className={styles.sceneContainer}
          style={{
            opacity,
            transition: 'opacity 0.6s cubic-bezier(0.4, 0, 0.2, 1)',
            '--animation-color': ACCENT_COLORS[colorIndex]
          } as React.CSSProperties}
        >
          {currentScene === 'talking' && <TalkingScene />}
          {currentScene === 'fighting' && <FightingScene />}
          {currentScene === 'coffee' && <CoffeeScene />}
          {currentScene === 'writing' && <WritingScene />}
          {currentScene === 'thinking' && <ThinkingScene />}
        </div>

        <h2 className={styles.title}>Debate in Progress</h2>
        <p className={styles.message}>{currentMessage}</p>
      </div>
    </div>
  );
}

function TalkingScene() {
  return (
    <div className={styles.debateScene}>
      <div data-animate="left-person" className={styles.person}>
        <div className={styles.head} />
        <div className={styles.body} />
      </div>
      <div className={styles.speechArea}>
        <div data-animate="bubble" className={`${styles.speechBubble} ${styles.leftBubble}`}>
          <span className={styles.bubbleText}>!</span>
        </div>
        <div data-animate="bubble" className={`${styles.speechBubble} ${styles.rightBubble}`}>
          <span className={styles.bubbleText}>?</span>
        </div>
      </div>
      <div data-animate="right-person" className={styles.person}>
        <div className={styles.head} />
        <div className={styles.body} />
      </div>
    </div>
  );
}

function FightingScene() {
  return (
    <div className={styles.debateScene}>
      <div data-animate="left-person" className={styles.person}>
        <div className={styles.head} />
        <div className={styles.body} />
      </div>
      <div className={styles.fightCenter}>
        <div data-animate="dust-cloud" className={styles.dustCloud}>
          <span data-animate="symbol" className={`${styles.fightSymbol} ${styles.symbol1}`}>@</span>
          <span data-animate="symbol" className={`${styles.fightSymbol} ${styles.symbol2}`}>#</span>
          <span data-animate="symbol" className={`${styles.fightSymbol} ${styles.symbol3}`}>!</span>
          <span data-animate="symbol" className={`${styles.fightSymbol} ${styles.symbol4}`}>*</span>
        </div>
        <div data-animate="sword" className={`${styles.fightSword} ${styles.sword1}`}>⚔</div>
        <div data-animate="sword" className={`${styles.fightSword} ${styles.sword2}`}>⚔</div>
      </div>
      <div data-animate="right-person" className={styles.person}>
        <div className={styles.head} />
        <div className={styles.body} />
      </div>
    </div>
  );
}

function CoffeeScene() {
  return (
    <div className={styles.debateScene}>
      <div data-animate="person-coffee" className={styles.person}>
        <div className={styles.head} />
        <div className={styles.body} />
      </div>
      <div className={styles.coffeeArea}>
        <div data-animate="cup" className={styles.coffeeCup}>
          <div className={styles.cupBody} />
          <div className={styles.cupHandle} />
        </div>
        <div data-animate="steam" className={styles.steam}>
          <div className={styles.steamLine} />
          <div className={styles.steamLine} />
          <div className={styles.steamLine} />
        </div>
      </div>
      <div data-animate="person-coffee" className={styles.person}>
        <div className={styles.head} />
        <div className={styles.body} />
      </div>
    </div>
  );
}

function WritingScene() {
  return (
    <div className={styles.debateScene}>
      <div data-animate="writer" className={styles.person}>
        <div className={styles.head} />
        <div className={styles.body} />
      </div>
      <div className={styles.documentArea}>
        <div data-animate="document" className={styles.document}>
          <div data-animate="line" className={styles.docLine} />
          <div data-animate="line" className={styles.docLine} />
          <div data-animate="line" className={styles.docLine} />
        </div>
        <div data-animate="pen" className={styles.pen} />
      </div>
    </div>
  );
}

function ThinkingScene() {
  return (
    <div className={styles.debateScene}>
      <div data-animate="thinker" className={`${styles.person} ${styles.thinker}`}>
        <div className={styles.head} />
        <div className={styles.body} />
        <div data-animate="hand" className={styles.thinkingHand} />
      </div>
      <div className={styles.thoughtArea}>
        <div data-animate="thought-tiny" className={styles.thoughtBubbleTiny} />
        <div data-animate="thought-small" className={styles.thoughtBubbleSmall} />
        <div data-animate="thought-main" className={styles.thoughtBubble}>
          <span className={styles.thoughtDots}>...</span>
        </div>
      </div>
    </div>
  );
}

// Animation functions
function animateTalking(container: HTMLElement) {
  const leftPerson = container.querySelector('[data-animate="left-person"]');
  const rightPerson = container.querySelector('[data-animate="right-person"]');
  const bubbles = container.querySelectorAll('[data-animate="bubble"]');

  if (leftPerson) {
    anime({
      targets: leftPerson,
      translateY: [-2, 2, -2],
      duration: 1500,
      loop: true,
      easing: 'easeInOutSine',
    });
  }

  if (rightPerson) {
    anime({
      targets: rightPerson,
      translateY: [2, -2, 2],
      duration: 1500,
      loop: true,
      easing: 'easeInOutSine',
      delay: 750,
    });
  }

  if (bubbles.length) {
    anime({
      targets: bubbles,
      opacity: [0, 1, 1, 0],
      scale: [0.8, 1, 1, 0.8],
      duration: 2000,
      delay: anime.stagger(1000),
      loop: true,
      easing: 'easeInOutQuad',
    });
  }
}

function animateFighting(container: HTMLElement) {
  const leftPerson = container.querySelector('[data-animate="left-person"]');
  const rightPerson = container.querySelector('[data-animate="right-person"]');
  const dustCloud = container.querySelector('[data-animate="dust-cloud"]');
  const symbols = container.querySelectorAll('[data-animate="symbol"]');
  const swords = container.querySelectorAll('[data-animate="sword"]');

  if (leftPerson) {
    anime({
      targets: leftPerson,
      translateX: [0, 5, 0, -3, 0],
      translateY: [-2, 2, -2],
      duration: 500,
      loop: true,
      easing: 'easeInOutQuad',
    });
  }

  if (rightPerson) {
    anime({
      targets: rightPerson,
      translateX: [0, -5, 0, 3, 0],
      translateY: [2, -2, 2],
      duration: 500,
      loop: true,
      easing: 'easeInOutQuad',
      delay: 100,
    });
  }

  if (dustCloud) {
    anime({
      targets: dustCloud,
      scale: [0.9, 1.1, 0.95, 1.05, 0.9],
      rotate: [-3, 3, -2, 2, -3],
      opacity: [0.7, 1, 0.8, 1, 0.7],
      duration: 400,
      loop: true,
      easing: 'easeInOutQuad',
    });
  }

  if (symbols.length) {
    symbols.forEach((symbol, index) => {
      anime({
        targets: symbol,
        translateX: [
          { value: anime.random(-10, 10), duration: 200 },
          { value: anime.random(-10, 10), duration: 200 },
        ],
        translateY: [
          { value: anime.random(-10, 10), duration: 200 },
          { value: anime.random(-10, 10), duration: 200 },
        ],
        rotate: [0, anime.random(-20, 20)],
        opacity: [0.6, 1, 0.6],
        duration: 400,
        delay: index * 80,
        loop: true,
        easing: 'easeInOutQuad',
      });
    });
  }

  if (swords.length) {
    anime({
      targets: swords,
      scale: [0, 1.2, 0],
      rotate: [0, 20, -20, 0],
      opacity: [0, 1, 0],
      duration: 600,
      delay: anime.stagger(200),
      loop: true,
      easing: 'easeOutQuad',
    });
  }
}

function animateCoffee(container: HTMLElement) {
  const persons = container.querySelectorAll('[data-animate="person-coffee"]');
  const cup = container.querySelector('[data-animate="cup"]');
  const steam = container.querySelector('[data-animate="steam"]');

  if (persons.length) {
    anime({
      targets: persons,
      translateY: [-1, 1, -1],
      duration: 2500,
      loop: true,
      easing: 'easeInOutSine',
    });
  }

  if (cup) {
    anime({
      targets: cup,
      rotate: [-2, 2, -2],
      duration: 2000,
      loop: true,
      easing: 'easeInOutSine',
    });
  }

  if (steam) {
    const steamLines = steam.querySelectorAll(`.${styles.steamLine}`);
    anime({
      targets: steamLines,
      translateY: [-15, -25],
      opacity: [0.8, 0],
      duration: 1500,
      delay: anime.stagger(300),
      loop: true,
      easing: 'easeOutQuad',
    });
  }
}

function animateWriting(container: HTMLElement) {
  const writer = container.querySelector('[data-animate="writer"]');
  const lines = container.querySelectorAll('[data-animate="line"]');
  const pen = container.querySelector('[data-animate="pen"]');

  if (writer) {
    anime({
      targets: writer,
      translateY: [-1, 1, -1],
      duration: 1000,
      loop: true,
      easing: 'easeInOutSine',
    });
  }

  if (lines.length) {
    anime({
      targets: lines,
      width: ['0%', '100%'],
      opacity: [0, 0.6],
      duration: 1500,
      delay: anime.stagger(400),
      loop: true,
      easing: 'easeInOutQuad',
    });
  }

  if (pen) {
    anime({
      targets: pen,
      translateX: [0, 40, 0],
      translateY: [0, 5, 10, 15, 10, 5, 0],
      duration: 2000,
      loop: true,
      easing: 'easeInOutQuad',
    });
  }
}

function animateThinking(container: HTMLElement) {
  const thinker = container.querySelector('[data-animate="thinker"]');
  const hand = container.querySelector('[data-animate="hand"]');
  const thoughtTiny = container.querySelector('[data-animate="thought-tiny"]');
  const thoughtSmall = container.querySelector('[data-animate="thought-small"]');
  const thoughtMain = container.querySelector('[data-animate="thought-main"]');

  if (thinker) {
    anime({
      targets: thinker,
      translateY: [-1, 1, -1],
      duration: 2000,
      loop: true,
      easing: 'easeInOutSine',
    });
  }

  if (hand) {
    anime({
      targets: hand,
      translateY: [-2, 0, -2],
      duration: 1500,
      loop: true,
      easing: 'easeInOutSine',
    });
  }

  // Staggered thought bubble appearance - from head upward
  const bubbles = [thoughtTiny, thoughtSmall, thoughtMain].filter(Boolean);
  if (bubbles.length) {
    anime({
      targets: bubbles,
      opacity: [0, 1, 1, 0],
      scale: [0.6, 1, 1, 0.8],
      translateY: [10, 0, 0, -5],
      duration: 3000,
      delay: anime.stagger(300),
      loop: true,
      easing: 'easeInOutQuad',
    });
  }
}
