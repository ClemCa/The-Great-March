import { useEffect, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { actions } from '../bridge/actions';
import { cn } from '../lib/cn';
import { Panel } from './Panel';
import type { SettingSelector, SettingsSnapshot } from '../bridge/types';

const TABS = ['Audio', 'Display', 'Gameplay', 'Dialogue'];

/** A 20-segment level readout; the lit tail is the blue reserved for active controls. */
const METER_SEGMENTS = 20;

/** How a numeric readout commits/scrubs: bounds, increment and value-change per dragged pixel. */
interface NumberFormat {
  min: number;
  max: number;
  step: number;
  sensitivity: number;
  format?: (value: number) => string;
}

const twoDecimals = (value: number) => String(Math.round(value * 100) / 100);
const wholeNumber = (value: number) => String(Math.round(value));

const VOLUME_FIELD: NumberFormat = { min: 0, max: 1, step: 0.01, sensitivity: 0.004, format: twoDecimals };
const TEMPERATURE_FIELD: NumberFormat = { min: 0, max: 2, step: 0.05, sensitivity: 0.008, format: twoDecimals };
const HISTORY_FIELD: NumberFormat = { min: 0, max: 1000, step: 1, sensitivity: 0.1, format: wholeNumber };

/** Rounds to the field's step, then trims float noise (0.30000000000000004 -> 0.3). */
function quantise(value: number, step: number) {
  return Number((Math.round(value / step) * step).toFixed(6));
}

function Group({ label, children }: { label: string; children: ReactNode }) {
  return (
    <view className="settings-group">
      <view className="settings-group__head">
        <view className="settings-group__tick" />
        <text className="settings-group__label">{label}</text>
      </view>
      {children}
    </view>
  );
}

function Row({
  label,
  children,
  dim,
  ghost,
}: {
  label: string;
  children: ReactNode;
  dim?: boolean;
  ghost?: boolean;
}) {
  const [hot, setHot] = useState(false);
  return (
    <view
      className={cn('settings-row', dim && 'settings-row--dim')}
      onMouseEnter={() => setHot(true)}
      onMouseLeave={() => setHot(false)}
    >
      <text className="settings-row__label">{label}</text>
      <view className="settings-row__controls">
        {children}
        {ghost && (
          <view className="settings-slot-ghost">
            <view className={cn('settings-slot-ghost__line', hot && 'settings-slot-ghost__line--hot')} />
          </view>
        )}
      </view>
    </view>
  );
}

interface MeterProps {
  value: number;
  onChange: (next: number) => void;
}

/**
 * Direct-manipulation level bar. Hovering a segment previews the level under the cursor (up *or*
 * down: the span between the committed level and the cursor is shown as a pending tail); pressing
 * and dragging scrubs it, so the whole bar works without ever touching the +/- steppers.
 */
function Meter({ value, onChange }: MeterProps) {
  const [hover, setHover] = useState<number | null>(null);
  const [dragging, setDragging] = useState(false);

  const lit = Math.round(clamp01(value) * METER_SEGMENTS);
  const target = hover ?? lit;
  const low = Math.min(lit, target);
  const high = Math.max(lit, target);

  return (
    <view
      className="settings-meter"
      onMouseLeave={() => {
        setHover(null);
        setDragging(false);
      }}
      onPointerUp={() => setDragging(false)}
    >
      {Array.from({ length: METER_SEGMENTS }, (_, index) => {
        const level = (index + 1) / METER_SEGMENTS;
        const on = index < low;
        const preview = index >= low && index < high;
        const hovered = hover !== null && index === hover - 1;
        return (
          <button
            key={index}
            className={cn(
              'settings-meter__seg',
              on && 'settings-meter__seg--on',
              preview && 'settings-meter__seg--preview',
              hovered && 'settings-meter__seg--hovered',
            )}
            style={hovered ? { scale: '1.8' } : undefined}
            onMouseEnter={() => {
              setHover(index + 1);
              if (dragging) onChange(level);
            }}
            onPointerDown={() => {
              setDragging(true);
              onChange(level);
            }}
            onClick={() => onChange(level)}
          />
        );
      })}
    </view>
  );
}

function MeterRow({
  label,
  value,
  onDelta,
  onChange,
}: {
  label: string;
  value: number;
  onDelta: (delta: number) => void;
  onChange: (next: number) => void;
}) {
  return (
    <Row label={label}>
      <Meter value={value} onChange={onChange} />
      <view className="settings-stepper-slot">
        <Stepper value={Math.round(value * 100) / 100} onDelta={onDelta} onChange={onChange} field={VOLUME_FIELD} />
      </view>
    </Row>
  );
}

function Stepper({
  value,
  onDelta,
  onChange,
  field,
  disabled,
}: {
  value: number;
  onDelta: (delta: number) => void;
  onChange: (value: number) => void;
  field: NumberFormat;
  disabled?: boolean;
}) {
  return (
    <view className="settings-stepper">
      <button className={cn('settings-btn', disabled && 'settings-btn--off')} onClick={disabled ? undefined : () => onDelta(-1)}>
        <text>-</text>
      </button>
      <NumberField value={value} onChange={onChange} disabled={disabled} {...field} />
      <button className={cn('settings-btn', disabled && 'settings-btn--off')} onClick={disabled ? undefined : () => onDelta(1)}>
        <text>+</text>
      </button>
    </view>
  );
}

/**
 * The stepper's readout, made direct: click the number to type a value, or press and drag it
 * left/right (or up/down) to scrub. Both axes move the value so the gesture reads the same in
 * either direction. The working value is held in a ref so a drag stays smooth between bridge
 * updates, and a drag that never passes the threshold still counts as a click and opens the editor.
 */
function NumberField({
  value,
  min,
  max,
  step,
  sensitivity,
  format,
  onChange,
  disabled,
}: NumberFormat & { value: number; onChange: (value: number) => void; disabled?: boolean }) {
  const [editing, setEditing] = useState(false);
  const working = useRef(value);
  const dragged = useRef(false);
  const inputRef = useRef<any>(null);

  useEffect(() => {
    if (!editing) return;
    const field: any = inputRef.current;
    if (!field) return;
    if (typeof field.focus === 'function') {
      // Browser preview: focus the DOM input and preselect, so typing replaces the value.
      field.focus();
      field.select?.();
      return;
    }
    // Unity: activate the field, then select its whole contents so a first keystroke replaces it.
    const inner = field.InputField;
    inner?.ActivateInputField?.();
    if (inner) {
      inner.selectionAnchorPosition = 0;
      inner.selectionFocusPosition = (inner.text ?? '').length;
    }
  }, [editing]);

  if (editing) {
    return (
      <input
        ref={inputRef}
        className="settings-value settings-value--edit"
        value={format ? format(value) : String(value)}
        contentType="decimal-number"
        disabled={disabled}
        onEndEdit={(text) => {
          const parsed = Number.parseFloat(text);
          if (Number.isFinite(parsed)) onChange(quantise(Math.min(max, Math.max(min, parsed)), step));
          setEditing(false);
        }}
      />
    );
  }

  return (
    <text
      className={cn('settings-value', 'settings-value--scrub', disabled && 'settings-btn--off')}
      onPointerDown={() => {
        working.current = value;
        dragged.current = false;
      }}
      onBeginDrag={() => {
        working.current = value;
        dragged.current = true;
      }}
      onDrag={(event) => {
        if (disabled) return;
        dragged.current = true;
        const next = working.current + (event.delta.x + event.delta.y) * sensitivity;
        working.current = Math.min(max, Math.max(min, next));
        onChange(quantise(working.current, step));
      }}
      onClick={() => {
        if (disabled) return;
        if (dragged.current) {
          dragged.current = false;
          return;
        }
        setEditing(true);
      }}
    >
      {format ? format(value) : String(value)}
    </text>
  );
}

function Toggle({ on, onClick, disabled }: { on: boolean; onClick: () => void; disabled?: boolean }) {
  return (
    <button
      className={cn('settings-toggle', on && 'settings-toggle--on', disabled && 'settings-btn--off')}
      onClick={disabled ? undefined : onClick}
    >
      <text>{on ? 'On' : 'Off'}</text>
    </button>
  );
}

function Cycle({ value, onClick, disabled }: { value: string; onClick: () => void; disabled?: boolean }) {
  return (
    <button className={cn('settings-cycle', disabled && 'settings-btn--off')} onClick={disabled ? undefined : onClick}>
      <text>{value}</text>
    </button>
  );
}

/**
 * Carousel selector: the current choice sits framed in the middle with the neighbouring options
 * faded to either side. Clicking a side jumps straight to it, clicking the middle advances; the
 * strip remounts on every pick (keyed by index) so it replays its slide-in animation. The slide is
 * held back on first mount (only `picked` strips animate) so a tab change enters uniformly with the
 * other rows instead of the selector sliding twice. Falls back to the plain `Cycle` button when the
 * bridge does not supply an option list.
 */
function Selector({
  settingKey,
  value,
  selector,
  disabled,
}: {
  settingKey: string;
  value: string;
  selector?: SettingSelector;
  disabled?: boolean;
}) {
  const [direction, setDirection] = useState<'left' | 'right'>('right');
  const [picked, setPicked] = useState(false);
  const options = selector?.options ?? [];
  const count = options.length;

  if (!selector || count <= 1) {
    return <Cycle value={value} disabled={disabled} onClick={() => actions.cycleSetting(settingKey)} />;
  }

  const index = selector.index;
  const hasLeft = index > 0;
  const hasRight = index < count - 1;
  const pick = (next: number, dir: 'left' | 'right') => {
    if (disabled || next < 0 || next >= count) return;
    setDirection(dir);
    setPicked(true);
    actions.selectSetting(settingKey, next);
  };

  return (
    <view className={cn('settings-selector', disabled && 'settings-btn--off')}>
      <view
        key={index}
        className={cn(
          'settings-selector__strip',
          `settings-selector__strip--${direction}`,
          picked && 'settings-selector__strip--anim',
        )}
      >
        {hasLeft ? (
          <button className="settings-selector__side" onClick={() => pick(index - 1, 'left')}>
            <text className="settings-selector__side-text">{options[index - 1]}</text>
          </button>
        ) : (
          <view className="settings-selector__slot" />
        )}
        <button className="settings-selector__current" onClick={() => pick(index + 1, 'right')}>
          <text>{options[index] ?? value}</text>
        </button>
        {hasRight ? (
          <button className="settings-selector__side" onClick={() => pick(index + 1, 'right')}>
            <text className="settings-selector__side-text">{options[index + 1]}</text>
          </button>
        ) : (
          <view className="settings-selector__slot" />
        )}
      </view>
    </view>
  );
}

/**
 * React port of the `SettingsMenu` prefab. Values are read back from the bridge every frame, so a
 * change made here (or by the game) is always reflected; every edit is dispatched as a bridge action.
 * Laid out as grouped console sections rather than a flat list, so the tabs read as panels.
 */
export function SettingsPanel({
  open,
  settings,
  onClose,
}: {
  open: boolean;
  settings: SettingsSnapshot;
  onClose: () => void;
}) {
  const [tab, setTab] = useState(0);
  const llmDisabled = !settings.thoughtExploration;
  const selectorFor = (key: string) => settings.selectors?.find((entry) => entry.key === key);

  return (
    <view className="settings-sidebar" style={{ left: open ? '0%' : '-100%' }}>
      <Panel
        className="settings-panel"
        edge="right"
        surface="menu"
        title="SETTINGS"
        icon="cog"
        onClose={onClose}
      >
        <view className="settings-tabs">
          {TABS.map((name, index) => (
            <button
              key={name}
              className={cn('settings-tab', index === tab && 'settings-tab--active')}
              onClick={() => {
                actions.click();
                setTab(index);
              }}
            >
              <text>{name}</text>
            </button>
          ))}
        </view>

        <view className="settings-body" key={tab}>
          {tab === 0 && (
            <>
              <Group label="MIXER">
                <MeterRow
                  label="Master Volume"
                  value={settings.master}
                  onDelta={(d) => actions.setSettingFloat('master', clamp01(settings.master + d * 0.05))}
                  onChange={(next) => actions.setSettingFloat('master', next)}
                />
                <MeterRow
                  label="UI Volume"
                  value={settings.ui}
                  onDelta={(d) => actions.setSettingFloat('ui', clamp01(settings.ui + d * 0.05))}
                  onChange={(next) => actions.setSettingFloat('ui', next)}
                />
                <MeterRow
                  label="SFX Volume"
                  value={settings.sfx}
                  onDelta={(d) => actions.setSettingFloat('sfx', clamp01(settings.sfx + d * 0.05))}
                  onChange={(next) => actions.setSettingFloat('sfx', next)}
                />
                <MeterRow
                  label="Music Volume"
                  value={settings.music}
                  onDelta={(d) => actions.setSettingFloat('music', clamp01(settings.music + d * 0.05))}
                  onChange={(next) => actions.setSettingFloat('music', next)}
                />
              </Group>
              <Group label="AMBIENCE">
                <Row label="Background Sound" ghost>
                  <Toggle
                    on={settings.backgroundSound}
                    onClick={() => actions.setSettingBool('backgroundSound', !settings.backgroundSound)}
                  />
                </Row>
              </Group>
            </>
          )}

          {tab === 1 && (
            <>
              <Group label="WINDOW">
                <Row label="Fullscreen" ghost>
                  <Toggle on={settings.fullscreen} onClick={() => actions.setSettingBool('fullscreen', !settings.fullscreen)} />
                </Row>
                <Row label="VSync" ghost>
                  <Toggle on={settings.vsync} onClick={() => actions.setSettingBool('vsync', !settings.vsync)} />
                </Row>
                <Row label="Resolution">
                  <Selector settingKey="resolution" value={settings.resolution} selector={selectorFor('resolution')} />
                </Row>
              </Group>
              <Group label="GRAPHICS">
                <Row label="Quality">
                  <Selector settingKey="quality" value={settings.quality} selector={selectorFor('quality')} />
                </Row>
                <Row label="Antialiasing">
                  <Selector settingKey="antialiasing" value={settings.antialiasing} selector={selectorFor('antialiasing')} />
                </Row>
                <Row label="Antialiasing Quality">
                  <Selector
                    settingKey="antialiasingQuality"
                    value={settings.antialiasingQuality}
                    selector={selectorFor('antialiasingQuality')}
                  />
                </Row>
              </Group>
            </>
          )}

          {tab === 2 && (
            <Group label="ASSISTANCE">
              <Row label="Show Tooltips" ghost>
                <Toggle on={settings.showPrompt} onClick={() => actions.setSettingBool('showPrompt', !settings.showPrompt)} />
              </Row>
            </Group>
          )}

          {tab === 3 && (
            <Group label="EXPERIMENTAL">
              <Row label="Enable Thought Exploration" ghost>
                <Toggle
                  on={settings.thoughtExploration}
                  onClick={() => actions.setSettingBool('thoughtExploration', !settings.thoughtExploration)}
                />
              </Row>
              <Row label="Mode" dim={llmDisabled}>
                <Selector
                  settingKey="mode"
                  value={settings.mode}
                  selector={selectorFor('mode')}
                  disabled={llmDisabled}
                />
              </Row>
              <Row label="Provider" dim={llmDisabled}>
                <Selector
                  settingKey="provider"
                  value={settings.provider}
                  selector={selectorFor('provider')}
                  disabled={llmDisabled}
                />
              </Row>
              <Row label="Base URL" dim={llmDisabled}>
                <input
                  className="settings-input"
                  value={settings.baseUrl}
                  placeholder="default"
                  disabled={llmDisabled}
                  onEndEdit={(value) => actions.setLlmSetting('baseUrl', value)}
                />
              </Row>
              <Row label="Model" dim={llmDisabled}>
                <input
                  className="settings-input"
                  value={settings.model}
                  placeholder="default"
                  disabled={llmDisabled}
                  onEndEdit={(value) => actions.setLlmSetting('model', value)}
                />
              </Row>
              <Row label="API Key" dim={llmDisabled}>
                <input
                  className="settings-input"
                  value={settings.apiKey}
                  placeholder="none"
                  disabled={llmDisabled}
                  onEndEdit={(value) => actions.setLlmSetting('apiKey', value)}
                />
              </Row>
              <Row label="Temperature" dim={llmDisabled} ghost>
                <Stepper
                  value={Math.round(settings.temperature * 100) / 100}
                  onDelta={(d) =>
                    actions.setSettingFloat('temperature', Math.max(0, Math.min(2, settings.temperature + d * 0.05)))
                  }
                  onChange={(value) => actions.setSettingFloat('temperature', value)}
                  field={TEMPERATURE_FIELD}
                  disabled={llmDisabled}
                />
              </Row>
              <Row label="Verbatim History" dim={llmDisabled} ghost>
                <Stepper
                  value={settings.verbatim}
                  onDelta={(d) => actions.setSettingFloat('verbatim', Math.max(0, settings.verbatim + d))}
                  onChange={(value) => actions.setSettingFloat('verbatim', value)}
                  field={HISTORY_FIELD}
                  disabled={llmDisabled}
                />
              </Row>
              <Row label="Summarized History" dim={llmDisabled} ghost>
                <Stepper
                  value={settings.summarized}
                  onDelta={(d) => actions.setSettingFloat('summarized', Math.max(0, settings.summarized + d))}
                  onChange={(value) => actions.setSettingFloat('summarized', value)}
                  field={HISTORY_FIELD}
                  disabled={llmDisabled}
                />
              </Row>
              <Row label="Include Thoughts JSON" dim={llmDisabled} ghost>
                <Toggle
                  on={settings.thoughts}
                  onClick={() => actions.setSettingBool('thoughts', !settings.thoughts)}
                  disabled={llmDisabled}
                />
              </Row>
            </Group>
          )}
        </view>
      </Panel>
    </view>
  );
}

function clamp01(value: number) {
  return Math.max(0, Math.min(1, value));
}
