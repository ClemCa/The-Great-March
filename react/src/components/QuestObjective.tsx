import { cn } from '../lib/cn';
import type { QuestSnapshot } from '../bridge/types';

/**
 * Objective banner driven by `Questing`. Mirrors the original `Quest` canvas: it slides down from
 * above the screen while a questline objective is active, and back up once it finishes.
 */
export function QuestObjective({ quest }: { quest: QuestSnapshot | undefined }) {
  const visible = !!quest && quest.visible && !!quest.text;

  return (
    <view className={cn('quest-objective', visible && 'quest-objective--open')}>
      <text className="quest-objective__text">{visible ? quest!.text : ''}</text>
    </view>
  );
}
