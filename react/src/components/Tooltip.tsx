import type { TooltipSnapshot } from '../bridge/types';

/**
 * Cursor-following help tooltip. Mirrors the original `PromptMenu`: Unity resolves the hovered
 * element's title/description/info lines (it owns the registry lookups) and reports the cursor as
 * a normalised position, which React turns into a viewport-relative anchor.
 */
export function Tooltip({ tooltip }: { tooltip: TooltipSnapshot | undefined }) {
  if (!tooltip || !tooltip.visible || (!tooltip.title && !tooltip.description)) return null;

  const left = `${Math.min(92, Math.max(0, tooltip.x * 100))}%`;
  const top = `${Math.min(90, Math.max(0, (1 - tooltip.y) * 100))}%`;

  return (
    <view className="tooltip" style={{ left, top }}>
      {tooltip.title ? <text className="tooltip__title">{tooltip.title}</text> : null}
      {tooltip.description ? <text className="tooltip__desc">{tooltip.description}</text> : null}
      {tooltip.info1 ? <text className="tooltip__info">{tooltip.info1}</text> : null}
      {tooltip.info2 ? <text className="tooltip__info">{tooltip.info2}</text> : null}
    </view>
  );
}
