import { useRef, useState } from 'react';
import { actions } from '../bridge/actions';
import { iconFor } from '../assets/icons';
import type { PlanetSnapshot } from '../bridge/types';
import { cn } from '../lib/cn';
import { HudMark } from './HudMark';
import { Panel } from './Panel';

/** Rows are 60px tall; a drag moves one slot per row-height of accumulated vertical travel. */
const ROW_STEP = 60;

/**
 * Consumption priorities submenu, mirroring `PriorityMenu`/`PriorityUpdater`: a food/fuel
 * and local/global switch over a reorderable list of resources. Rows can be reordered by
 * dragging the grip handle or by the up/down buttons beside it.
 */
export function PriorityMenu({ planet, onClose }: { planet: PlanetSnapshot; onClose: () => void }) {
  const [fuel, setFuel] = useState(false);
  const [global, setGlobal] = useState(false);
  const [dragIndex, setDragIndex] = useState<number | null>(null);
  const drag = useRef<{ index: number; travel: number } | null>(null);

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

  const beginDrag = (index: number) => {
    drag.current = { index, travel: 0 };
    setDragIndex(index);
  };

  const onDrag = (event: { delta?: { y?: number } }) => {
    const state = drag.current;
    if (!state) return;
    state.travel += event?.delta?.y ?? 0;
    while (state.travel >= ROW_STEP && state.index > 0) {
      actions.movePriority(display, state.index, -1, global);
      state.index -= 1;
      state.travel -= ROW_STEP;
    }
    while (state.travel <= -ROW_STEP && state.index < list.length - 1) {
      actions.movePriority(display, state.index, 1, global);
      state.index += 1;
      state.travel += ROW_STEP;
    }
    setDragIndex(state.index);
  };

  const endDrag = () => {
    drag.current = null;
    setDragIndex(null);
  };

  return (
    <Panel
      className="hud-submenu hud-submenu--priority"
      edge="left"
      surface="hud"
      title="Consumption Priorities"
      icon="consume"
      headerSize="sm"
      onClose={onClose}
      closePlacement="flush"
    >
      <view className="priority-switches">
        {/* What the list governs: feeding the population or refuelling ships. */}
        <view className="priority-switches__group">
          <button
            className={cn('priority-toggle', !fuel && 'priority-toggle--on')}
            onClick={() => setFuel(false)}
          >
            <text>People</text>
          </button>
          <button
            className={cn('priority-toggle', fuel && 'priority-toggle--on')}
            onClick={() => setFuel(true)}
          >
            <text>Fuel</text>
          </button>
        </view>

        {/* Which variant of the same list we are editing: this planet's or the shared default. */}
        <view className="priority-switches__group">
          <button
            className={cn('priority-toggle', !global && 'priority-toggle--on')}
            onClick={() => setGlobal(false)}
          >
            <text>Local</text>
          </button>
          <button
            className={cn('priority-toggle', global && 'priority-toggle--on')}
            onClick={() => setGlobal(true)}
          >
            <text>Global</text>
          </button>
        </view>
      </view>

      <scroll className="hud-submenu__list">
        <view className="priority-list">
          {list.map((id, index) => {
            const resource = entry(id);
            return (
              <view
                key={id}
                className={cn('priority-row', dragIndex === index && 'priority-row--dragging')}
              >
                <view className="priority-row__grip" onBeginDrag={() => beginDrag(index)} onDrag={onDrag} onEndDrag={endDrag}>
                  <HudMark icon="grip" className="hud-mark--sm" />
                </view>
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
    </Panel>
  );
}
