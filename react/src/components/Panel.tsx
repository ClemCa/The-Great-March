import type { ReactNode } from 'react';
import type { Style } from '@reactunity/renderer';
import { cn } from '../lib/cn';
import { HudMark } from './HudMark';

/** Which side of the frame carries the blue leading line. It always faces the screen centre. */
export type PanelEdge = 'left' | 'right' | 'bottom';

/** `menu` surfaces read as solid sheets; `hud` stay slightly translucent so the scene shows through. */
export type PanelSurface = 'menu' | 'hud';

/** Heading scale: `sm` HUD submenus, `md` sidebars, `lg` the planet panel's identity header. */
export type PanelHeaderSize = 'sm' | 'md' | 'lg';

/** `inset` parks the dismiss button inside the corner; `flush` lets it fill the title bar's tail. */
export type PanelClosePlacement = 'inset' | 'flush';

export interface PanelProps {
  /** Geometry and body styling; everything shared by every panel comes from the component. */
  className?: string;
  style?: Style;
  edge?: PanelEdge;
  surface?: PanelSurface;
  /** Console heading. Leave undefined for a panel with no title bar. */
  title?: string;
  /** Console glyph shown before the heading. */
  icon?: string;
  headerSize?: PanelHeaderSize;
  /** Renders the dismiss button when provided. */
  onClose?: () => void;
  closePlacement?: PanelClosePlacement;
  children?: ReactNode;
}

/**
 * The single frame behind every menu, HUD panel and submenu: a near-black surface, a hairline rule
 * with a blue leading edge facing the centre of the screen, and a bottom vignette that sinks it
 * into the scene. Panels layer their own geometry and body on top rather than restating the frame,
 * so a new panel is a size, a position and an edge.
 *
 * `children` is the panel body, so a panel can host another panel — that is how the planet
 * submenus are built on top of the planet panel.
 */
export function Panel({
  className,
  style,
  edge,
  surface,
  title,
  icon,
  headerSize = 'md',
  onClose,
  closePlacement = 'inset',
  children,
}: PanelProps) {
  return (
    <view
      className={cn(
        'console-panel',
        edge && `console-panel--edge-${edge}`,
        surface && `console-panel--${surface}`,
        className,
      )}
      style={style}
    >
      <view className="console-panel__vignette" />

      {title !== undefined && (
        <PanelHeader
          title={title}
          icon={icon}
          size={headerSize}
          roomy={!!onClose && closePlacement === 'inset'}
        />
      )}

      {children}

      {onClose && <PanelClose placement={closePlacement} onClick={onClose} />}
    </view>
  );
}

/** Console heading: a blue mark and an uppercase label over a hairline rule. */
export function PanelHeader({
  title,
  icon,
  size,
  roomy,
}: {
  title: string;
  icon?: string;
  size: PanelHeaderSize;
  roomy?: boolean;
}) {
  return (
    <view
      className={cn(
        'console-panel__header',
        `console-panel__header--${size}`,
        roomy && 'console-panel__header--roomy',
      )}
    >
      {icon && <HudMark icon={icon} />}
      <text className="console-panel__title">{title}</text>
    </view>
  );
}

/** Dismiss button. Anchored to the panel corner so surrounding padding can never shift it. */
export function PanelClose({
  placement,
  onClick,
}: {
  placement: PanelClosePlacement;
  onClick: () => void;
}) {
  return (
    <button
      className={cn('console-panel__close', placement === 'flush' && 'console-panel__close--flush')}
      onClick={onClick}
    >
      <text>X</text>
    </button>
  );
}