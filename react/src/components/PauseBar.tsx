import { useState } from 'react';
import { actions } from '../bridge/actions';
import type { GameState } from '../bridge/types';
import { cn } from '../lib/cn';
import { SettingsPanel } from './SettingsPanel';

/**
 * Top-right pause cluster. Mirrors the original `PauseCanvas`:
 *  - the two-bar pause icon pulses while paused,
 *  - an expandable strip (`<`/`>`) reveals the Help/Settings buttons,
 *  - Save/Load open their four slot lists (Load slots disabled when unused).
 */
export function PauseBar({ state }: { state: GameState }) {
  const [extended, setExtended] = useState(false);
  const [openSlots, setOpenSlots] = useState<'save' | 'load' | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const slots = state.slots ?? [];

  return (
    <view className="pause-bar">
      {state.paused && (
        <view className="pause-menu">
          <view className="pause-row">
            <button className="pause-button" onClick={actions.goToMainMenu}>
              <text>Exit</text>
            </button>
            <button
              className="pause-button"
              onClick={() => setOpenSlots((current) => (current === 'load' ? null : 'load'))}
            >
              <text>Load</text>
            </button>
            <button
              className="pause-button"
              onClick={() => setOpenSlots((current) => (current === 'save' ? null : 'save'))}
            >
              <text>Save</text>
            </button>
            {extended && (
              <button className="pause-button" onClick={() => actions.setPrompt(!state.prompt)}>
                <text>{state.prompt ? 'Hide Help' : 'Show Help'}</text>
              </button>
            )}
            {extended && (
              <button className="pause-button" onClick={() => setSettingsOpen(true)}>
                <text>Settings</text>
              </button>
            )}
            <button className="pause-button pause-extend" onClick={() => setExtended((e) => !e)}>
              <text>{extended ? '>' : '<'}</text>
            </button>
          </view>

          {openSlots && (
            <view className="pause-slots">
              {slots.map((slot) => {
                const disabled = openSlots === 'load' && !slot.used;
                return (
                  <button
                    key={slot.index}
                    className={cn('pause-slot', disabled && 'pause-slot--disabled')}
                    onClick={
                      disabled
                        ? undefined
                        : () =>
                            openSlots === 'save'
                              ? actions.saveGame(slot.index)
                              : actions.loadGame(slot.index)
                    }
                  >
                    <text>{`Slot ${slot.index}`}</text>
                  </button>
                );
              })}
            </view>
          )}
        </view>
      )}

      <button
        className={cn('pause-icon', state.paused && 'pause-icon--pulsing')}
        onClick={actions.togglePause}
      >
        <view className="pause-icon__bar" />
        <view className="pause-icon__bar" />
      </button>

      {settingsOpen && <SettingsPanel settings={state.settings} onClose={() => setSettingsOpen(false)} />}
    </view>
  );
}
