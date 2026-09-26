import { PauseBar } from '../components/PauseBar';
import { TaskQueue } from '../components/TaskQueue';
import { PlanetPanel } from '../components/PlanetPanel';
import { DialogBox } from '../components/DialogBox';
import { QuestObjective } from '../components/QuestObjective';
import { Tooltip } from '../components/Tooltip';
import type { GameState } from '../bridge/types';

export function GameScreen({ state }: { state: GameState }) {
  return (
    <view className="app-root">
      <TaskQueue state={state} />
      <PlanetPanel state={state} />
      <QuestObjective quest={state.quest} />
      <PauseBar state={state} />
      <DialogBox dialog={state.dialog} />
      <Tooltip tooltip={state.tooltip} />
    </view>
  );
}
