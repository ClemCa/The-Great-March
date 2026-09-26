import { useGameState } from './bridge/useGameState';
import { MainMenuScreen } from './screens/MainMenuScreen';
import { LoseScreen } from './screens/LoseScreen';
import { GameScreen } from './screens/GameScreen';

export function App() {
  const state = useGameState();

  if (!state) {
    return (
      <view className="app-root flex flex-col items-center justify-center">
        <text className="text-sm text-slate-500">Connecting to Unity…</text>
      </view>
    );
  }

  // Only the hand-authored menu scenes get a dedicated screen; every other scene
  // (Game, Tutorial, StoryTest, any future gameplay scene) is the in-game HUD.
  if (state.scene === 'MainMenu') return <MainMenuScreen state={state} />;
  if (state.scene === 'LoseMenu') return <LoseScreen state={state} />;
  return <GameScreen state={state} />;
}
