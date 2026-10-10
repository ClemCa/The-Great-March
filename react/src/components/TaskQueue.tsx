import type { GameState } from '../bridge/types';
import { cn } from '../lib/cn';
import { Panel } from './Panel';

/**
 * Left-hand task queue. Mirrors the original `Queue` canvas: it slides in only
 * while a planet is selected and lists that planet's orders with progress.
 */
export function TaskQueue({ state }: { state: GameState }) {
  const planet = state.hasSelectedPlanet ? state.selectedPlanet : null;
  const orders = planet ? (state.queue ?? []).filter((order) => order.planet === planet.name) : [];

  return (
    <Panel
      className={cn('task-queue', !planet && 'task-queue--hidden')}
      edge="right"
      surface="hud"
      title="Task Queue"
      icon="list"
    >
      <view className="task-queue__list">
        {orders.length === 0 && <text className="task-queue__empty">No active tasks</text>}
        {orders.map((order, index) => (
          <view key={index} className="queue-card">
            <text className="queue-card__type">{order.type.replace(/([a-z0-9])([A-Z])/g, '$1 $2')}</text>
            <text className="queue-card__line">{`Assigned: ${order.assigned}/${order.maxPeople} people`}</text>
            <text className="queue-card__line">{`Speed: x${order.speed}`}</text>
            <text className="queue-card__line">{`Tasks: ${order.lengthLeft}/${order.length}`}</text>
            <view className="queue-card__track">
              <view
                className="queue-card__progress"
                style={{ width: `${Math.round(order.progress * 100)}%` }}
              />
            </view>
          </view>
        ))}
      </view>
    </Panel>
  );
}
