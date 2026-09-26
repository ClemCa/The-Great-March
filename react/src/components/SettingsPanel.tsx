import { useState } from 'react';
import { actions } from '../bridge/actions';
import { cn } from '../lib/cn';
import type { SettingsSnapshot } from '../bridge/types';

const TABS = ['Audio', 'Display', 'Gameplay', 'Dialogue'];

function Row({ label, children, dim }: { label: string; children: React.ReactNode; dim?: boolean }) {
  return (
    <view className={cn('settings-row', dim && 'settings-row--dim')}>
      <text className="settings-row__label">{label}</text>
      <view className="settings-row__controls">{children}</view>
    </view>
  );
}

function Stepper({
  value,
  onDelta,
  onSet,
}: {
  value: number;
  onDelta: (delta: number) => void;
  onSet?: (value: number) => void;
}) {
  return (
    <view className="settings-stepper">
      <button className="settings-btn" onClick={() => onDelta(-1)}>
        <text>-</text>
      </button>
      <text className="settings-value" onClick={onSet ? () => onSet(value) : undefined}>
        {value}
      </text>
      <button className="settings-btn" onClick={() => onDelta(1)}>
        <text>+</text>
      </button>
    </view>
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
 * React port of the `SettingsMenu` prefab. Values are read back from the bridge every frame, so a
 * change made here (or by the game) is always reflected; every edit is dispatched as a bridge action.
 */
export function SettingsPanel({ settings, onClose }: { settings: SettingsSnapshot; onClose: () => void }) {
  const [tab, setTab] = useState(0);
  const llmDisabled = settings.mode !== 'LLM';

  return (
    <view className="settings-overlay">
      <view className="settings-panel">
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
          <button className="settings-close" onClick={onClose}>
            <text>X</text>
          </button>
        </view>

        <view className="settings-body">
          {tab === 0 && (
            <>
              <Row label="Master Volume">
                <Stepper
                  value={Math.round(settings.master * 100) / 100}
                  onDelta={(d) => actions.setSettingFloat('master', clamp01(settings.master + d * 0.05))}
                />
              </Row>
              <Row label="UI Volume">
                <Stepper
                  value={Math.round(settings.ui * 100) / 100}
                  onDelta={(d) => actions.setSettingFloat('ui', clamp01(settings.ui + d * 0.05))}
                />
              </Row>
              <Row label="SFX Volume">
                <Stepper
                  value={Math.round(settings.sfx * 100) / 100}
                  onDelta={(d) => actions.setSettingFloat('sfx', clamp01(settings.sfx + d * 0.05))}
                />
              </Row>
              <Row label="Music Volume">
                <Stepper
                  value={Math.round(settings.music * 100) / 100}
                  onDelta={(d) => actions.setSettingFloat('music', clamp01(settings.music + d * 0.05))}
                />
              </Row>
              <Row label="Background Sound">
                <Toggle on={settings.backgroundSound} onClick={() => actions.setSettingBool('backgroundSound', !settings.backgroundSound)} />
              </Row>
            </>
          )}

          {tab === 1 && (
            <>
              <Row label="Fullscreen">
                <Toggle on={settings.fullscreen} onClick={() => actions.setSettingBool('fullscreen', !settings.fullscreen)} />
              </Row>
              <Row label="VSync">
                <Toggle on={settings.vsync} onClick={() => actions.setSettingBool('vsync', !settings.vsync)} />
              </Row>
              <Row label="Resolution">
                <Cycle value={settings.resolution} onClick={() => actions.cycleSetting('resolution')} />
              </Row>
              <Row label="Quality">
                <Cycle value={settings.quality} onClick={() => actions.cycleSetting('quality')} />
              </Row>
              <Row label="Antialiasing">
                <Cycle value={settings.antialiasing} onClick={() => actions.cycleSetting('antialiasing')} />
              </Row>
              <Row label="Antialiasing Quality">
                <Cycle value={settings.antialiasingQuality} onClick={() => actions.cycleSetting('antialiasingQuality')} />
              </Row>
            </>
          )}

          {tab === 2 && (
            <>
              <Row label="Show Help Prompts">
                <Toggle on={settings.showPrompt} onClick={() => actions.setSettingBool('showPrompt', !settings.showPrompt)} />
              </Row>
            </>
          )}

          {tab === 3 && (
            <>
              <Row label="Mode">
                <Cycle value={settings.mode} onClick={() => actions.cycleSetting('mode')} />
              </Row>
              <Row label="Provider" dim={llmDisabled}>
                <Cycle value={settings.provider} disabled={llmDisabled} onClick={() => actions.cycleSetting('provider')} />
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
              <Row label="Temperature" dim={llmDisabled}>
                <Stepper
                  value={Math.round(settings.temperature * 100) / 100}
                  onDelta={(d) =>
                    actions.setSettingFloat('temperature', Math.max(0, Math.min(2, settings.temperature + d * 0.05)))
                  }
                />
              </Row>
              <Row label="Verbatim History">
                <Stepper value={settings.verbatim} onDelta={(d) => actions.setSettingFloat('verbatim', Math.max(0, settings.verbatim + d))} />
              </Row>
              <Row label="Summarized History">
                <Stepper value={settings.summarized} onDelta={(d) => actions.setSettingFloat('summarized', Math.max(0, settings.summarized + d))} />
              </Row>
              <Row label="Include Thoughts JSON">
                <Toggle on={settings.thoughts} onClick={() => actions.setSettingBool('thoughts', !settings.thoughts)} />
              </Row>
            </>
          )}
        </view>
      </view>
    </view>
  );
}

function clamp01(value: number) {
  return Math.max(0, Math.min(1, value));
}
