import { Button } from '../components/Button';
import { actions } from '../bridge/actions';
import type { GameState } from '../bridge/types';

export function LoseScreen({ state }: { state: GameState }) {
  return (
    <view className="app-root flex flex-col items-center justify-center bg-slate-900 px-10">
      <text className="text-4xl font-bold text-red-400">The March Ends</text>
      <text className="mt-2 text-sm text-slate-400">Your civilisation did not survive the journey.</text>

      <view className="mt-8 flex flex-col items-stretch gap-2 w-80">
        <StatRow label="Survived" value={`${state.survivalTime} min`} />
        <StatRow label="Total time" value={`${state.totalTime} min`} />
        <StatRow label="Systems visited" value={`${state.systems}`} />
        <StatRow label="Natural resources" value={`${state.naturalResourcesUnits}`} />
        <StatRow label="Advanced resources" value={`${state.advancedResourcesUnits}`} />
        <StatRow label="Facilities built" value={`${state.facilitiesCount}`} />
        <StatRow label="Transformation facilities" value={`${state.transformativeFacilitiesCount}`} />
      </view>

      <view className="mt-8 flex flex-col items-stretch gap-3 w-80">
        <Button variant="primary" onClick={actions.restart}>
          <text>Try Again</text>
        </Button>
        <Button onClick={actions.goToMainMenu}>
          <text>Main Menu</text>
        </Button>
        <Button variant="ghost" onClick={actions.exit}>
          <text>Exit</text>
        </Button>
      </view>
    </view>
  );
}

function StatRow({ label, value }: { label: string; value: string }) {
  return (
    <view className="flex flex-row items-center justify-between rounded-lg bg-slate-800 px-4 py-2">
      <text className="text-sm text-slate-400">{label}</text>
      <text className="text-sm font-semibold text-slate-100">{value}</text>
    </view>
  );
}
