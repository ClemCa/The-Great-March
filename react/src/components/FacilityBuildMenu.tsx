import { actions } from '../bridge/actions';
import { iconFor } from '../assets/icons';
import type { FacilityOption } from '../bridge/types';
import { cn } from '../lib/cn';
import { Panel } from './Panel';

interface FacilityBuildMenuProps {
  title: string;
  options: FacilityOption[];
  onClose: () => void;
}

/**
 * Sliding submenu listing the facilities that can be installed in a facility slot,
 * mirroring the original `FacilitySubMenu` / `TransformationFacilitySubMenu`.
 */
export function FacilityBuildMenu({ title, options, onClose }: FacilityBuildMenuProps) {
  return (
    <Panel
      className="hud-submenu"
      edge="left"
      surface="hud"
      title={title}
      icon="plus"
      headerSize="sm"
    >
      <scroll className="hud-submenu__list">
        <view className="hud-submenu__grid">
          {options.map((option) => (
            <button
              key={option.id}
              className={cn('hud-submenu__cell', !option.canBuild && 'hud-submenu__cell--locked')}
              onMouseEnter={() => actions.promptTarget(option.wildcard ? 'transformation' : 'facility', option.id)}
              onMouseLeave={actions.clearPrompt}
              onClick={
                option.canBuild
                  ? () => {
                      actions.buildFacility(option.id);
                      onClose();
                    }
                  : undefined
              }
            >
              <image className="hud-submenu__icon" src={iconFor(option.icon)} />
            </button>
          ))}
        </view>
      </scroll>

      <button className="hud-submenu__return" onClick={onClose}>
        <text>Return</text>
      </button>
    </Panel>
  );
}
