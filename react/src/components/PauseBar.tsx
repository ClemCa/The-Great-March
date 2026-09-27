import { useState } from 'react';
import { actions } from '../bridge/actions';
import type { GameState, SlotEntry } from '../bridge/types';
import { cn } from '../lib/cn';
import { SettingsPanel } from './SettingsPanel';

/**
 * Top-right pause cluster. The plain white pause bars hold the corner and toggle pause; while
 * paused they brighten and a console-styled panel of session actions drops beneath them. Save/Load
 * reveal their slot list inline, directly beneath the row that opened it, and are grouped away from
 * the options and the exit by hairline rules.
 */
export function PauseBar({ state }: { state: GameState }) {
  const [openSlots, setOpenSlots] = useState<'save' | 'load' | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const slots = state.slots ?? [];

  const toggleSlots = (mode: 'save' | 'load') =>
    setOpenSlots((current) => (current === mode ? null : mode));

  return (
    <>
      <view className="pause-cluster">
        <button
          className={cn('pause-icon', state.paused && 'pause-icon--pulsing')}
          onClick={actions.togglePause}
        >
          <view className="pause-icon__bar" />
          <view className="pause-icon__bar" />
        </button>

        {state.paused && (
          <view className="pause-panel">
            <view className="pause-panel__vignette" />

            <view className="pause-panel__actions">
              <button className="pause-action pause-action--primary" onClick={actions.togglePause}>
                <text>Resume</text>
              </button>

              <view className="pause-sep" />

              <button className="pause-action" onClick={() => toggleSlots('save')}>
                <text>Save Game</text>
              </button>
              {openSlots === 'save' && <SlotPicker mode="save" slots={slots} />}

              <button className="pause-action" onClick={() => toggleSlots('load')}>
                <text>Load Game</text>
              </button>
              {openSlots === 'load' && <SlotPicker mode="load" slots={slots} />}

              <view className="pause-sep" />

              <button className="pause-action" onClick={() => actions.setPrompt(!state.prompt)}>
                <text>{state.prompt ? 'Hide Tooltips' : 'Show Tooltips'}</text>
              </button>
              <button className="pause-action" onClick={() => setSettingsOpen(true)}>
                <text>Settings</text>
              </button>

              <view className="pause-sep" />

              <button className="pause-action" onClick={actions.goToMainMenu}>
                <text>Exit to Main Menu</text>
              </button>
            </view>
          </view>
        )}
      </view>

      {settingsOpen && <view className="sidebar-scrim" onClick={() => setSettingsOpen(false)} />}
      <SettingsPanel
        open={settingsOpen}
        settings={state.settings}
        onClose={() => setSettingsOpen(false)}
      />
    </>
  );
}

/** Inline slot list: five rows under the Save/Load action, indented so it reads as a sub-level. */
function SlotPicker({ mode, slots }: { mode: 'save' | 'load'; slots: SlotEntry[] }) {
  return (
    <view className="pause-slots">
      {slots.map((slot) => {
        const disabled = mode === 'load' && !slot.used;
        return (
          <button
            key={slot.index}
            className={cn('pause-slot-row', disabled && 'pause-slot-row--disabled')}
            onClick={
              disabled
                ? undefined
                : () =>
                    mode === 'save' ? actions.saveGame(slot.index) : actions.loadGame(slot.index)
            }
          >
            <text className={cn('pause-slot-row__label', disabled && 'pause-slot-row__label--disabled')}>
              {`Slot ${slot.index}`}
            </text>
            <text className={cn('pause-slot-row__meta', disabled && 'pause-slot-row__meta--disabled')}>
              {slot.used ? 'Occupied' : 'Empty'}
            </text>
          </button>
        );
      })}
    </view>
  );
}
