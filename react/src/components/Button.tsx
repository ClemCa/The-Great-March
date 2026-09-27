import type { ReactNode } from 'react';
import { cn } from '../lib/cn';

interface ButtonProps {
  children: ReactNode;
  variant?: 'primary' | 'default' | 'ghost';
  disabled?: boolean;
  className?: string;
  onClick?: () => void;
}

// All full-screen actions use the same stark white rectangle as the main menu; `ghost` only
// drops the fill, it never introduces a second accent colour.
const VARIANTS: Record<NonNullable<ButtonProps['variant']>, string> = {
  primary: '',
  default: '',
  ghost: 'button--ghost',
};

export function Button({ children, variant = 'default', disabled, className, onClick }: ButtonProps) {
  return (
    <button
      className={cn('button', VARIANTS[variant], disabled && 'button--disabled', className)}
      onClick={disabled ? undefined : onClick}
    >
      {children}
    </button>
  );
}
