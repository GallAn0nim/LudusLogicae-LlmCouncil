import { InputHTMLAttributes, forwardRef } from 'react';
import styles from './Slider.module.css';

interface SliderProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: string;
  showValue?: boolean;
  formatValue?: (value: number) => string;
}

export const Slider = forwardRef<HTMLInputElement, SliderProps>(
  (
    {
      label,
      showValue = true,
      formatValue = (v) => v.toString(),
      className = '',
      value,
      min = 0,
      max = 100,
      step = 1,
      ...props
    },
    ref
  ) => {
    const numValue = typeof value === 'string' ? parseFloat(value) : (value as number) || 0;
    const numMin = typeof min === 'string' ? parseFloat(min) : min;
    const numMax = typeof max === 'string' ? parseFloat(max) : max;
    const percentage = ((numValue - numMin) / (numMax - numMin)) * 100;

    return (
      <div className={styles.wrapper}>
        {(label || showValue) && (
          <div className={styles.header}>
            {label && <label className={styles.label}>{label}</label>}
            {showValue && (
              <span className={styles.value}>{formatValue(numValue)}</span>
            )}
          </div>
        )}
        <div className={styles.sliderWrapper}>
          <input
            ref={ref}
            type="range"
            className={`${styles.slider} ${className}`}
            value={value}
            min={min}
            max={max}
            step={step}
            style={{ '--progress': `${percentage}%` } as React.CSSProperties}
            {...props}
          />
        </div>
      </div>
    );
  }
);

Slider.displayName = 'Slider';
