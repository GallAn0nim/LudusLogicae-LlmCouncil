import { useEffect, useRef } from 'react';
import anime from 'animejs';

interface AnimationOptions {
  animation?: 'fadeInUp' | 'fadeIn' | 'scaleIn' | 'slideRight' | 'slideLeft' | 'stagger';
  delay?: number;
  duration?: number;
  staggerDelay?: number;
}

export function useAnimateOnMount<T extends HTMLElement>(options: AnimationOptions = {}) {
  const ref = useRef<T>(null);
  const {
    animation = 'fadeInUp',
    delay = 0,
    duration = 500,
    staggerDelay = 50,
  } = options;

  useEffect(() => {
    if (!ref.current) return;

    const element = ref.current;
    element.style.opacity = '0';

    const animations: Record<string, anime.AnimeParams> = {
      fadeInUp: {
        opacity: [0, 1],
        translateY: [20, 0],
      },
      fadeIn: {
        opacity: [0, 1],
      },
      scaleIn: {
        opacity: [0, 1],
        scale: [0.9, 1],
      },
      slideRight: {
        opacity: [0, 1],
        translateX: [50, 0],
      },
      slideLeft: {
        opacity: [0, 1],
        translateX: [-50, 0],
      },
      stagger: {
        opacity: [0, 1],
        translateY: [20, 0],
      },
    };

    const animConfig = animations[animation] || animations.fadeInUp;

    anime({
      targets: element,
      ...animConfig,
      duration,
      delay: animation === 'stagger' ? anime.stagger(staggerDelay, { start: delay }) : delay,
      easing: 'easeOutCubic',
    });
  }, [animation, delay, duration, staggerDelay]);

  return ref;
}

export function useStaggerAnimation<T extends HTMLElement>(
  containerSelector: string,
  childSelector: string,
  delay = 0
) {
  const containerRef = useRef<T>(null);

  useEffect(() => {
    if (!containerRef.current) return;

    const children = containerRef.current.querySelectorAll(childSelector);
    if (children.length === 0) return;

    children.forEach((child) => {
      (child as HTMLElement).style.opacity = '0';
    });

    anime({
      targets: children,
      opacity: [0, 1],
      translateY: [30, 0],
      scale: [0.95, 1],
      duration: 500,
      delay: anime.stagger(80, { start: delay }),
      easing: 'easeOutCubic',
    });
  }, [containerSelector, childSelector, delay]);

  return containerRef;
}
