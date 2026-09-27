import { cn } from '../lib/cn';

interface MenuButtonProps {
  label: string;
  onClick?: () => void;
  enabled?: boolean;
  /** The single filled action in the panel; every other entry is an outlined row. */
  primary?: boolean;
  /** The secondary lose-screen action: transparent fill, brighter hairline border. */
  ghost?: boolean;
  className?: string;
}

/**
 * The 200x60 entry used by the main menu panel. `primary` fills it white, `ghost` reuses the lose
 * screen's secondary action; the rest are outlined rows so the panel reads as a layered list
 * rather than a stack of identical blocks.
 */
export function MenuButton({ label, onClick, enabled = true, primary, ghost, className }: MenuButtonProps) {
  return (
    <button
      className={cn(
        'menu-button',
        primary && 'menu-button--primary',
        ghost && 'menu-button--ghost',
        !enabled && 'menu-button--disabled',
        className,
      )}
      onClick={enabled ? onClick : undefined}
    >
      <text>{label}</text>
    </button>
  );
}
