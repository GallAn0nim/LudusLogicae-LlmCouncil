import anime from 'animejs';

export const fadeInUp = (targets: string | HTMLElement | HTMLElement[], delay = 0) => {
  return anime({
    targets,
    opacity: [0, 1],
    translateY: [20, 0],
    duration: 600,
    delay: anime.stagger(50, { start: delay }),
    easing: 'easeOutCubic',
  });
};

export const fadeIn = (targets: string | HTMLElement | HTMLElement[], duration = 400) => {
  return anime({
    targets,
    opacity: [0, 1],
    duration,
    easing: 'easeOutCubic',
  });
};

export const scaleIn = (targets: string | HTMLElement | HTMLElement[], delay = 0) => {
  return anime({
    targets,
    scale: [0.9, 1],
    opacity: [0, 1],
    duration: 400,
    delay,
    easing: 'easeOutBack',
  });
};

export const slideInFromRight = (targets: string | HTMLElement | HTMLElement[]) => {
  return anime({
    targets,
    translateX: [50, 0],
    opacity: [0, 1],
    duration: 500,
    easing: 'easeOutCubic',
  });
};

export const slideInFromLeft = (targets: string | HTMLElement | HTMLElement[]) => {
  return anime({
    targets,
    translateX: [-50, 0],
    opacity: [0, 1],
    duration: 500,
    easing: 'easeOutCubic',
  });
};

export const staggerCards = (targets: string | HTMLElement | HTMLElement[]) => {
  return anime({
    targets,
    opacity: [0, 1],
    translateY: [30, 0],
    scale: [0.95, 1],
    duration: 500,
    delay: anime.stagger(80),
    easing: 'easeOutCubic',
  });
};

export const pulseGlow = (targets: string | HTMLElement | HTMLElement[]) => {
  return anime({
    targets,
    boxShadow: [
      '0 0 0 rgba(129, 140, 248, 0)',
      '0 0 30px rgba(129, 140, 248, 0.4)',
      '0 0 0 rgba(129, 140, 248, 0)',
    ],
    duration: 1500,
    easing: 'easeInOutQuad',
  });
};

export const shake = (targets: string | HTMLElement | HTMLElement[]) => {
  return anime({
    targets,
    translateX: [-10, 10, -10, 10, -5, 5, 0],
    duration: 500,
    easing: 'easeOutElastic(1, .5)',
  });
};

export const celebrate = (targets: string | HTMLElement | HTMLElement[]) => {
  return anime({
    targets,
    scale: [1, 1.1, 1],
    rotate: [-3, 3, -3, 3, 0],
    duration: 800,
    easing: 'easeOutElastic(1, .5)',
  });
};

export const typewriter = (
  element: HTMLElement,
  text: string,
  speed = 30
): Promise<void> => {
  return new Promise((resolve) => {
    let i = 0;
    element.textContent = '';

    const type = () => {
      if (i < text.length) {
        element.textContent += text.charAt(i);
        i++;
        setTimeout(type, speed);
      } else {
        resolve();
      }
    };

    type();
  });
};

export const progressBar = (targets: string | HTMLElement | HTMLElement[], toWidth: string) => {
  return anime({
    targets,
    width: toWidth,
    duration: 1000,
    easing: 'easeOutExpo',
  });
};

export const morphNumber = (
  element: HTMLElement,
  from: number,
  to: number,
  duration = 1000
) => {
  return anime({
    targets: { value: from },
    value: to,
    duration,
    round: 1,
    easing: 'easeOutExpo',
    update: (anim) => {
      const obj = anim.animations[0].animatable.target as unknown as { value: number };
      element.textContent = String(obj.value);
    },
  });
};
