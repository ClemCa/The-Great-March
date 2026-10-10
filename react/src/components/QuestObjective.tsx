import { cn } from '../lib/cn';
import { Panel } from './Panel';
import type { QuestSnapshot } from '../bridge/types';

/**
 * Objective banner driven by `Questing`. Mirrors the original `Quest` canvas: it slides down from
 * above the screen while a questline objective is active, and back up once it finishes.
 */
export function QuestObjective({ quest }: { quest: QuestSnapshot | undefined }) {
  const visible = !!quest && quest.visible && !!quest.text;

  return (
    <Panel
      className={cn('quest-objective', visible && 'quest-objective--open')}
      edge="bottom"
      surface="hud"
    >
      <text className="quest-objective__text">{visible ? quest!.text : ''}</text>
    </Panel>
  );
}
