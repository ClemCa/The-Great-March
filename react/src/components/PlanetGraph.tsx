import type { GraphSnapshot } from '../bridge/types';

const WIDTH = 320;
const HEIGHT = 110;

const SERIES: { key: keyof GraphSnapshot; color: string; label: string }[] = [
  { key: 'people', color: '#ffffff', label: 'People' },
  { key: 'resources', color: '#38d430', label: 'Resources' },
  { key: 'facilities', color: '#4d79ff', label: 'Facilities' },
];

/**
 * Port of the original bitmap `GraphDrawer`. ReactUnity has no canvas/drawing API, so each
 * series is drawn as a group of vertical bars (one per sample) instead of a polyline; the
 * legend and colours match the original.
 */
export function PlanetGraph({ graph }: { graph: GraphSnapshot }) {
  const values = SERIES.map((series) => ({ ...series, points: graph?.[series.key] ?? [] }));
  const max = Math.max(1, ...values.flatMap((series) => series.points));
  const count = Math.max(1, ...values.map((series) => series.points.length));
  const sample = WIDTH / count;
  const barWidth = Math.max(2, Math.floor((sample - 2) / values.length));

  return (
    <view className="planet-graph">
      <view className="planet-graph__legend">
        {SERIES.map((series) => (
          <view key={series.key} className="planet-graph__legend-item">
            <view className="planet-graph__swatch" style={{ backgroundColor: series.color }} />
            <text className="planet-graph__legend-label">{series.label}</text>
          </view>
        ))}
      </view>

      <view className="planet-graph__plot">
        {values.map((series, seriesIndex) =>
          series.points.map((point, index) => (
            <view
              key={`${series.key}-${index}`}
              className="planet-graph__bar"
              style={{
                left: index * sample + seriesIndex * barWidth + 1,
                width: barWidth,
                height: Math.max(1, (point / max) * HEIGHT),
                backgroundColor: series.color,
              }}
            />
          )),
        )}
      </view>
    </view>
  );
}
