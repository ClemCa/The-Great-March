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

  // Only the main menu gets a dedicated screen. The lose screen and the tutorial now run
  // inside the Game scene, so they are chosen from the session state rather than a scene name.
  if (state.scene === 'MainMenu') return <MainMenuScreen state={state} />;
  if (state.gameOver) return <LoseScreen state={state} />;
  return <GameScreen state={state} />;
}
