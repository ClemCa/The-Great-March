import type { GameState } from '../bridge/types';
import { cn } from '../lib/cn';

/**
 * Left-hand task queue. Mirrors the original `Queue` canvas: it slides in only
 * while a planet is selected and lists that planet's orders with progress.
 */
export function TaskQueue({ state }: { state: GameState }) {
  const planet = state.hasSelectedPlanet ? state.selectedPlanet : null;
  const orders = planet ? (state.queue ?? []).filter((order) => order.planet === planet.name) : [];

  return (
    <view className={cn('task-queue', !planet && 'task-queue--hidden')}>
      <view className="task-queue__title">
        <text>Task Queue</text>
      </view>
      <view className="task-queue__list">
        {orders.map((order, index) => (
          <view key={index} className="queue-card">
            <text className="queue-card__type">{order.type.replace(/([A-Z])/g, ' $0')}</text>
            <text className="queue-card__line">{`Assigned: ${order.assigned}/${order.maxPeople} people`}</text>
            <text className="queue-card__line">{`Speed: x${order.speed}`}</text>
            <text className="queue-card__line">{`Tasks: ${order.lengthLeft}/${order.length}`}</text>
            <view className="queue-card__track">
              <view
                className="queue-card__progress"
                style={{ width: `${Math.round(order.progress * 200)}px` }}
              />
            </view>
          </view>
        ))}
      </view>
    </view>
  );
}
