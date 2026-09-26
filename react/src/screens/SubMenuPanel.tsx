import { actions } from '../bridge/actions';
import type { Credit } from '../menu/credits';

interface SubMenuPanelProps {
  open: boolean;
  entries: Credit[];
}

/**
 * The left-hand sliding panel used for the Credits and Team submenus. It stays mounted so the
 * `left` transition can animate it in and out, matching `SubMenu`'s lerp in the original.
 */
export function SubMenuPanel({ open, entries }: SubMenuPanelProps) {
  return (
    <view className="submenu-panel" style={{ left: open ? 0 : -300 }}>
      <scroll className="submenu-list">
        {entries.map((entry) => (
          <view key={entry.name} className="credit-item" onClick={() => actions.openUrl(entry.url)}>
            <text className="credit-name">{entry.name}</text>
            <text className="credit-desc">{entry.description}</text>
          </view>
        ))}
      </scroll>
    </view>
  );
}
