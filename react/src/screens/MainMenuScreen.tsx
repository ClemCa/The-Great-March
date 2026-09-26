import { useState } from 'react';
import { MenuButton } from '../components/MenuButton';
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

  const togglePanel = (next: Exclude<OpenPanel, null>) =>
    setPanel((current) => (current === next ? null : next));

  return (
    <view className="app-root">
      <view className="menu-title">
        <text className="menu-title-line">The Great</text>
        <view className="title-gap" />
        <text className="menu-title-line">March</text>
      </view>

      <SubMenuPanel open={panel === 'credits'} entries={CREDITS} />
      <SubMenuPanel open={panel === 'team'} entries={TEAM} />

      {loadOpen && (
        <view className="load-slots">
          {slots.map((slot) => (
            <button
              key={slot.index}
              className={cn('load-slot', !slot.used && 'menu-button--disabled')}
              onClick={slot.used ? () => actions.loadGame(slot.index) : undefined}
            >
              <text>{`Slot ${slot.index}`}</text>
            </button>
          ))}
        </view>
      )}

      <view className="side-panel">
        <view className="side-column">
          <MenuButton label="Start Game" onClick={actions.newGame} />
          <MenuButton label="Load Save" onClick={() => setLoadOpen((open) => !open)} />
          <MenuButton label="Tutorial" onClick={actions.tutorial} />
          <MenuButton label="Team" onClick={() => togglePanel('team')} />
          <MenuButton label="Credits" onClick={() => togglePanel('credits')} />
          <MenuButton label="Settings" onClick={() => setSettingsOpen(true)} />
          <MenuButton label="Exit" onClick={actions.exit} />
        </view>
      </view>

      {settingsOpen && <SettingsPanel settings={state.settings} onClose={() => setSettingsOpen(false)} />}
    </view>
  );
}
