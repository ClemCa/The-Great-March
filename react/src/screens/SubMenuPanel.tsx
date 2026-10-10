import { actions } from '../bridge/actions';
import type { Credit } from '../menu/credits';
import { Panel } from '../components/Panel';

interface SubMenuPanelProps {
  open: boolean;
  entries: Credit[];
  /** Uppercase console heading, mirroring the settings panel. */
  title: string;
  /** Console glyph shown before the heading. */
  icon: string;
  onClose: () => void;
}

/**
 * The left-hand sliding panel used for the Credits and Team submenus. It stays mounted so the
 * `left` transition can animate it in and out, matching `SubMenu`'s lerp in the original.
 */
export function SubMenuPanel({ open, entries, title, icon, onClose }: SubMenuPanelProps) {
  return (
    <view
      className="sidebar-slot sidebar-slot--left"
      style={{ left: open ? 0 : -300 }}
    >
      <Panel className="submenu-panel" edge="right" surface="menu" title={title} icon={icon} onClose={onClose}>
        <scroll className="submenu-list">
          {entries.map((entry) => (
            <view key={entry.name} className="credit-item" onClick={() => actions.openUrl(entry.url)}>
              <text className="credit-name">{entry.name}</text>
              <text className="credit-desc">{entry.description}</text>
            </view>
          ))}
        </scroll>
      </Panel>
    </view>
  );
}