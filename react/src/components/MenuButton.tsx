import { cn } from '../lib/cn';

interface MenuButtonProps {
  label: string;
  onClick?: () => void;
  enabled?: boolean;
  className?: string;
}

/**
 * The white, square, 200x60 button used by the main menu panel.
 */
export function MenuButton({ label, onClick, enabled = true, className }: MenuButtonProps) {
  return (
    <button
      className={cn('menu-button', !enabled && 'menu-button--disabled', className)}
      onClick={enabled ? onClick : undefined}
    >
      <text>{label}</text>
    </button>
  );
}
