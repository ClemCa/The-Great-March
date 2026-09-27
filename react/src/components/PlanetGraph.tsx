import { useState } from 'react';
import type { GraphSnapshot } from '../bridge/types';

const WIDTH = 320;
const HEIGHT = 110;
const PAD_X = 10;
// The original let peaks run to the very edge of the texture; here the plot sits right under the
// legend, so keep a little head/foot room or a series at max touches the labels.
const PAD_Y = 10;
const TOOLTIP_W = 118;
const DOT = 6;

const SERIES: { key: keyof GraphSnapshot; color: string; label: string }[] = [
  { key: 'people', color: '#ffffff', label: 'People' },
  { key: 'resources', color: '#00ff00', label: 'Resources' },
  { key: 'facilities', color: '#0000ff', label: 'Facilities' },
];

interface PlotPoint {
  x: number;
  y: number;
}

/** Maps a series into plot coordinates: the original's `x = 10 + i * (w - 20) / n` and a shared
 *  `value / max` vertical scale, flipped and inset by PAD_Y so the curve never hits an edge. */
function plot(values: number[], max: number): PlotPoint[] {
  const n = values.length;
  return values.map((value, i) => ({
    x: PAD_X + (i * (WIDTH - PAD_X * 2)) / n,
    y: PAD_Y + (1 - value / max) * (HEIGHT - PAD_Y * 2),
  }));
}

/**
 * Joins the samples with a Catmull-Rom spline converted to cubic Béziers. The original
 * `GraphDrawer` drew straight pixel segments -- it could not curve them -- so this is the one
 * place we deliberately improve on it.
 */
function toPath(points: PlotPoint[]): string {
  const n = points.length;
  if (n === 0) return '';

  const at = (i: number) => points[Math.max(0, Math.min(n - 1, i))];
  const round = (v: number) => v.toFixed(2);
  let d = `M ${round(points[0].x)},${round(points[0].y)}`;

  for (let i = 0; i < n - 1; i++) {
    const p0 = at(i - 1);
    const p1 = at(i);
    const p2 = at(i + 1);
    const p3 = at(i + 2);
    const c1x = p1.x + (p2.x - p0.x) / 6;
    const c1y = p1.y + (p2.y - p0.y) / 6;
    const c2x = p2.x - (p3.x - p1.x) / 6;
    const c2y = p2.y - (p3.y - p1.y) / 6;
    d += ` C ${round(c1x)},${round(c1y)} ${round(c2x)},${round(c2y)} ${round(p2.x)},${round(p2.y)}`;
  }
  return d;
}

function tooltipLeft(x: number): number {
  return x < WIDTH / 2 ? Math.min(x + 8, WIDTH - TOOLTIP_W) : Math.max(x - TOOLTIP_W - 8, 0);
}

/**
 * Port of the original bitmap `GraphDrawer`. It painted each series as a connected line straight
 * onto the graph texture, so we reproduce that with an inline `<svg>` -- ReactUnity tessellates it
 * through `com.unity.vectorgraphics`. The point mapping matches the original and the series are
 * ordered by their last value, descending. On top of that, hovering a sample shows a crosshair,
 * a dot per series and a tooltip; that interaction did not exist in the original.
 */
export function PlanetGraph({ graph }: { graph: GraphSnapshot }) {
  const [hover, setHover] = useState<number | null>(null);

  const series = SERIES.map((s) => ({ ...s, points: graph?.[s.key] ?? [] }));
  const max = Math.max(1, ...series.flatMap((s) => s.points));
  const count = Math.max(1, ...series.map((s) => s.points.length));
  const hasData = series.some((s) => s.points.length > 0);

  const ordered = [...series].sort(
    (a, b) => (b.points[b.points.length - 1] ?? 0) - (a.points[a.points.length - 1] ?? 0),
  );
  const projected = ordered.map((s) => ({ ...s, pts: plot(s.points, max) }));

  const hoverX = hover === null ? 0 : PAD_X + (hover * (WIDTH - PAD_X * 2)) / count;

  return (
    <view className="planet-graph">
      <view className="planet-graph__legend">
        {ordered.map((s) => (
          <view key={s.key} className="planet-graph__legend-item">
            <view className="planet-graph__swatch" style={{ backgroundColor: s.color }} />
            <text className="planet-graph__legend-label">{s.label}</text>
          </view>
        ))}
      </view>

      <view className="planet-graph__plot" onMouseLeave={() => setHover(null)}>
        <svg className="planet-graph__svg" viewBox={`0 0 ${WIDTH} ${HEIGHT}`}>
          {/* Transparent frame holds the sprite at a stable 320x110 so the scale never drifts. */}
          <rect x="0" y="0" width={WIDTH} height={HEIGHT} fill="#000000" fillOpacity={0} />
          {projected.map((s) => (
            <path
              key={s.key}
              d={toPath(s.pts)}
              fill="none"
              stroke={s.color}
              strokeWidth={2}
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ))}
        </svg>

        {hover !== null && (
          <>
            <view
              className="planet-graph__hover-line"
              style={{ left: hoverX, top: PAD_Y, height: HEIGHT - PAD_Y * 2 }}
            />
            {projected.map((s) => {
              const point = s.pts[hover];
              if (!point) return null;
              return (
                <view
                  key={s.key}
                  className="planet-graph__hover-dot"
                  style={{
                    left: point.x - DOT / 2,
                    top: point.y - DOT / 2,
                    backgroundColor: s.color,
                  }}
                />
              );
            })}
            <view
              className="planet-graph__tooltip"
              style={{ left: tooltipLeft(hoverX), top: 0, width: TOOLTIP_W }}
            >
              {projected.map((s) => (
                <view key={s.key} className="planet-graph__tooltip-row">
                  <view className="planet-graph__tooltip-swatch" style={{ backgroundColor: s.color }} />
                  <text className="planet-graph__tooltip-label">{s.label}</text>
                  <text className="planet-graph__tooltip-value">{s.points[hover] ?? '—'}</text>
                </view>
              ))}
            </view>
          </>
        )}

        {hasData &&
          Array.from({ length: count }, (_, i) => (
            <button
              key={i}
              className="planet-graph__hit"
              style={{ left: (i * WIDTH) / count, width: WIDTH / count }}
              onMouseEnter={() => setHover(i)}
            />
          ))}
      </view>
    </view>
  );
}
