import { Button } from '../components/Button';
import { actions } from '../bridge/actions';
import type { GameState } from '../bridge/types';

/**
 * Run summary shown when the march ends. Deliberately uses the same monochrome, square language
 * as the main menu and the HUD: flat rows, no rounded corners, no accent colour beyond the red
 * already reserved for progression/warnings.
 */
export function LoseScreen({ state }: { state: GameState }) {
  return (
    <view className="app-root">
      <view className="lose-overlay">
        <view className="lose-panel">
          <text className="lose-title">The March Ends</text>
          <text className="lose-subtitle">Despite meritorious efforts, you couldn't succeed at saving yourself (nor, for that matter, your species). Though somewhere, in these dark clouds, there must be some kind of trace of the heroic deeds of the one that was once called President.</text>

          <view className="lose-stats">
            <StatRow label="Survived" value={`${state.survivalTime} min`} />
            <StatRow label="Total time" value={`${state.totalTime} min`} />
            <StatRow label="Systems visited" value={`${state.systems}`} />
            <StatRow label="Natural resources" value={`${state.naturalResourcesUnits}`} />
            <StatRow label="Advanced resources" value={`${state.advancedResourcesUnits}`} />
            <StatRow label="Facilities built" value={`${state.facilitiesCount}`} />
            <StatRow label="Transformation facilities" value={`${state.transformativeFacilitiesCount}`} />
          </view>

          <view className="lose-actions">
            <Button variant="primary" onClick={actions.restart}>
              <text>Try Again</text>
            </Button>
            <Button variant="ghost" onClick={actions.goToMainMenu}>
              <text>Main Menu</text>
            </Button>
            <Button variant="ghost" onClick={actions.exit}>
              <text>Exit</text>
            </Button>
          </view>
        </view>
      </view>
    </view>
  );
}

function StatRow({ label, value }: { label: string; value: string }) {
  return (
    <view className="lose-stat">
      <text className="lose-stat__label">{label}</text>
      <text className="lose-stat__value">{value}</text>
    </view>
  );
}
