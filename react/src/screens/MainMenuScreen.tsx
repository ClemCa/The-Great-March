import { useState } from 'react';
import { MenuButton } from '../components/MenuButton';
import { Panel } from '../components/Panel';
import { SubMenuPanel } from './SubMenuPanel';
import { SettingsPanel } from '../components/SettingsPanel';
import { actions } from '../bridge/actions';
import type { GameState } from '../bridge/types';
import { CREDITS, TEAM } from '../menu/credits';
import { cn } from '../lib/cn';

type OpenPanel = 'credits' | 'team' | null;

export function MainMenuScreen({ state }: { state: GameState }) {
  const [panel, setPanel] = useState<OpenPanel>(null);
  const [loadOpen, setLoadOpen] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const slots = state.slots ?? [];

  const closeAll = () => {
    setPanel(null);
    setSettingsOpen(false);
  };

  const togglePanel = (next: Exclude<OpenPanel, null>) => {
    setSettingsOpen(false);
    setPanel((current) => (current === next ? null : next));
  };

  const openSettings = () => {
    setPanel(null);
    setSettingsOpen(true);
  };

  return (
    <view className="app-root">
      <view className="menu-title">
        <text className="menu-title-line">The Great</text>
        <view className="title-gap" />
        <text className="menu-title-line">March</text>
      </view>

      {(panel !== null || settingsOpen) && <view className="sidebar-scrim" onClick={closeAll} />}

      <view className="sidebar-slot sidebar-slot--right">
        <Panel className="side-panel" edge="left" surface="menu">
          {loadOpen && (
            <view className="load-slots">
              {slots.map((slot) => (
                <button
                  key={slot.index}
                  className={cn('load-slot', !slot.used && 'load-slot--disabled')}
                  onClick={slot.used ? () => actions.loadGame(slot.index) : undefined}
                >
                  <text>{`Slot ${slot.index}`}</text>
                </button>
              ))}
            </view>
          )}

          <view className="side-panel__body">
            <view className="side-column">
              <MenuButton label="Start Game" primary onClick={actions.newGame} />
              <MenuButton label="Load Save" onClick={() => setLoadOpen((open) => !open)} />
              <MenuButton label="Tutorial" onClick={actions.tutorial} />
              <MenuButton label="Team" ghost onClick={() => togglePanel('team')} />
              <MenuButton label="Credits" ghost onClick={() => togglePanel('credits')} />
              <MenuButton label="Settings" ghost onClick={openSettings} />
              <MenuButton label="Exit" ghost onClick={actions.exit} />
            </view>
          </view>
        </Panel>
      </view>

      <SubMenuPanel
        open={panel === 'credits'}
        entries={CREDITS}
        title="CREDITS"
        icon="star"
        onClose={() => setPanel(null)}
      />
      <SubMenuPanel
        open={panel === 'team'}
        entries={TEAM}
        title="TEAM"
        icon="people"
        onClose={() => setPanel(null)}
      />

      <SettingsPanel
        open={settingsOpen}
        settings={state.settings}
        onClose={() => setSettingsOpen(false)}
      />
    </view>
  );
}
