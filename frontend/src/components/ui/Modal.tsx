import { useEffect, useRef, ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';
import anime from 'animejs';
import styles from './Modal.module.css';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

export function Modal({ isOpen, onClose, title, children, size = 'md' }: ModalProps) {
  const overlayRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';

      if (overlayRef.current && contentRef.current) {
        anime({
          targets: overlayRef.current,
          opacity: [0, 1],
          duration: 200,
          easing: 'easeOutCubic',
        });

        anime({
          targets: contentRef.current,
          opacity: [0, 1],
          scale: [0.95, 1],
          duration: 300,
          easing: 'easeOutCubic',
        });
      }
    }

    return () => {
      document.body.style.overflow = '';
    };
  }, [isOpen]);

  const handleClose = () => {
    if (overlayRef.current && contentRef.current) {
      anime({
        targets: overlayRef.current,
        opacity: 0,
        duration: 150,
        easing: 'easeOutCubic',
      });

      anime({
        targets: contentRef.current,
        opacity: 0,
        scale: 0.95,
        duration: 150,
        easing: 'easeOutCubic',
        complete: onClose,
      });
    } else {
      onClose();
    }
  };

  const handleOverlayClick = (e: React.MouseEvent) => {
    if (e.target === overlayRef.current) {
      handleClose();
    }
  };

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        handleClose();
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  return createPortal(
    <div
      ref={overlayRef}
      className={styles.overlay}
      onClick={handleOverlayClick}
    >
      <div ref={contentRef} className={`${styles.modal} ${styles[size]}`}>
        {title && (
          <div className={styles.header}>
            <h3 className={styles.title}>{title}</h3>
            <button
              className={styles.closeButton}
              onClick={handleClose}
              aria-label="Close modal"
            >
              <X size={20} />
            </button>
          </div>
        )}
        <div className={styles.content}>{children}</div>
      </div>
    </div>,
    document.body
  );
}
