import { useState } from 'react';
import { actions } from '../bridge/actions';
import { iconFor } from '../assets/icons';
import type { PlanetSnapshot } from '../bridge/types';
import { cn } from '../lib/cn';

/**
 * Consumption priorities submenu, mirroring `PriorityMenu`/`PriorityUpdater`: a food/fuel
 * and local/global switch over a reorderable list of resources.
 */
export function PriorityMenu({ planet, onClose }: { planet: PlanetSnapshot; onClose: () => void }) {
  const [fuel, setFuel] = useState(false);
  const [global, setGlobal] = useState(false);

  const list = global
    ? fuel
      ? planet.globalPrioritiesFuel
      : planet.globalPrioritiesFood
    : fuel
      ? planet.prioritiesFuel
      : planet.prioritiesFood;

  const catalog = planet.resources;
  const entry = (id: number) => (id >= 0 && id < catalog.length ? catalog[id] : undefined);
  const display = fuel ? 1 : 0;

  return (
    <view className="hud-submenu hud-submenu--priority">
      <view className="hud-submenu__title">
        <text>{fuel ? 'Fuel Consumption' : 'Food Consumption'}</text>
      </view>

      <view className="priority-switches">
        <button
          className={cn('priority-toggle', !fuel && 'priority-toggle--on')}
          onClick={() => setFuel(false)}
        >
          <text>Food</text>
        </button>
        <button
          className={cn('priority-toggle', fuel && 'priority-toggle--on')}
          onClick={() => setFuel(true)}
        >
          <text>Fuel</text>
        </button>
        <button
          className={cn('priority-toggle', global && 'priority-toggle--on')}
          onClick={() => setGlobal((value) => !value)}
        >
          <text>{global ? 'Global' : 'Local'}</text>
        </button>
      </view>

      <scroll className="hud-submenu__list">
        <view className="priority-list">
          {list.map((id, index) => {
            const resource = entry(id);
            return (
              <view key={`${id}-${index}`} className="priority-row">
                <image className="priority-row__icon" src={iconFor(resource?.icon)} />
                <view className="priority-row__text">
                  <text className="priority-row__name">{resource?.name ?? `#${id}`}</text>
                  <text className="priority-row__value">
                    {`${fuel ? 'Fueling' : 'Feeding'} power: ${resource?.value ?? 0}`}
                  </text>
                </view>
                <view className="priority-row__controls">
                  <button
                    className="priority-row__btn"
                    onClick={() => actions.movePriority(display, index, -1, global)}
                  >
                    <text>Up</text>
                  </button>
                  <button
                    className="priority-row__btn"
                    onClick={() => actions.movePriority(display, index, 1, global)}
                  >
                    <text>Dn</text>
                  </button>
                </view>
              </view>
            );
          })}
        </view>
      </scroll>

      <button className="hud-submenu__return" onClick={onClose}>
        <text>Return</text>
      </button>
    </view>
  );
}
