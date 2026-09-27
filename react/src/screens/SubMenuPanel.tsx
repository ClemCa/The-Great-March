import { actions } from '../bridge/actions';
import type { Credit } from '../menu/credits';

interface SubMenuPanelProps {
  open: boolean;
  entries: Credit[];
  /** Uppercase console heading, mirroring the settings panel. */
  title: string;
  onClose: () => void;
}

/**
 * The left-hand sliding panel used for the Credits and Team submenus. It stays mounted so the
 * `left` transition can animate it in and out, matching `SubMenu`'s lerp in the original.
 */
export function SubMenuPanel({ open, entries, title, onClose }: SubMenuPanelProps) {
  return (
    <view
      className="sidebar-slot sidebar-slot--left"
      style={{ left: open ? 0 : -300 }}
    >
      <view className="submenu-panel">
        <view className="submenu-panel__vignette" />
        <view className="submenu-header">
          <view className="submenu-header__mark" />
          <text className="submenu-header__title">{title}</text>
          <button className="sidebar-close" onClick={onClose}>
            <text>X</text>
          </button>
        </view>
        <scroll className="submenu-list">
          {entries.map((entry) => (
            <view key={entry.name} className="credit-item" onClick={() => actions.openUrl(entry.url)}>
              <text className="credit-name">{entry.name}</text>
              <text className="credit-desc">{entry.description}</text>
            </view>
          ))}
        </scroll>
      </view>
    </view>
  );
}
