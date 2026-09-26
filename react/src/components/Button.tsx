import type { ReactNode } from 'react';
import { cn } from '../lib/cn';

interface ButtonProps {
  children: ReactNode;
  variant?: 'primary' | 'default' | 'ghost';
  disabled?: boolean;
  className?: string;
  onClick?: () => void;
}

const VARIANTS: Record<NonNullable<ButtonProps['variant']>, string> = {
  primary: 'bg-amber-500 text-slate-900',
  default: 'bg-slate-700 text-slate-100',
  ghost: 'bg-transparent text-slate-300',
};

export function Button({ children, variant = 'default', disabled, className, onClick }: ButtonProps) {
  return (
    <button
      className={cn(
        'items-center justify-center rounded-lg px-5 py-3',
        VARIANTS[variant],
        disabled && 'opacity-40',
        className,
      )}
      onClick={disabled ? undefined : onClick}
    >
      {children}
    </button>
  );
}
