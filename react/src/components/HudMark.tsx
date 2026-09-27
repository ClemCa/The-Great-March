import { cn } from '../lib/cn';

/**
 * Console heading glyphs.
 *
 * Each mark is a 5x5 pixel pattern ("#" lit, anything else empty), rendered as absolutely
 * positioned cells inside a fixed box so it scales with CSS alone. This keeps one consistent,
 * data-only icon language that works identically in Unity and the browser preview, and lets a
 * heading carry a glyph that actually says what it is (a planet reading, a queue, a build menu).
 *
 * Add a variant by dropping another 5x5 array in `HUD_GLYPHS`; the preview gallery at
 * `?gallery=marks` lists them all.
 */
export const HUD_GLYPHS: Record<string, readonly string[]> = {
  // --- Telemetry / status -------------------------------------------------
  signal: ['....#', '....#', '..#.#', '#.#.#', '#.#.#'],
  'signal-d': ['#....', '#....', '#.#..', '#.#.#', '#.#.#'],
  pulse: ['..#..', '..#..', '#.#.#', '#.#.#', '#.#.#'],
  bars: ['#.#.#', '#.#.#', '#.#.#', '#.#.#', '#.#.#'],
  grid: ['##.##', '##.##', '.....', '##.##', '##.##'],
  dots: ['.....', '.....', '#.#.#', '.....', '.....'],
  'dots-v': ['..#..', '.....', '..#..', '.....', '..#..'],
  matrix: ['#.#.#', '.....', '#.#.#', '.....', '#.#.#'],

  // --- Shapes -------------------------------------------------------------
  plus: ['..#..', '..#..', '#####', '..#..', '..#..'],
  diamond: ['..#..', '.#.#.', '#...#', '.#.#.', '..#..'],
  'diamond-fill': ['..#..', '.###.', '#####', '.###.', '..#..'],
  square: ['#####', '#...#', '#...#', '#...#', '#####'],
  block: ['.....', '.###.', '.###.', '.###.', '.....'],
  ring: ['.###.', '#...#', '#...#', '#...#', '.###.'],
  target: ['.###.', '#...#', '#.#.#', '#...#', '.###.'],
  reticle: ['#...#', '.....', '..#..', '.....', '#...#'],
  concentric: ['#####', '#...#', '#.#.#', '#...#', '#####'],
  hash: ['.#.#.', '#####', '.#.#.', '#####', '.#.#.'],
  // 7x7: a proper cog needs the finer grid (toothed rim, hollow hub) to read as "settings".
  cog: [
    '.#...#.',
    '#######',
    '##...##',
    '##.#.##',
    '##...##',
    '#######',
    '.#...#.',
  ],

  // --- Direction ----------------------------------------------------------
  chevron: ['#....', '.#...', '..#..', '.#...', '#....'],
  'chevron-d': ['#...#', '.#.#.', '..#..', '.....', '.....'],
  caret: ['.....', '..#..', '.#.#.', '#...#', '.....'],
  'arrow-r': ['.#...', '..##.', '#####', '..##.', '.#...'],
  'arrow-u': ['..#..', '.###.', '#.#.#', '..#..', '..#..'],
  'arrow-d': ['..#..', '..#..', '#.#.#', '.###.', '..#..'],
  slash: ['#....', '.#...', '..#..', '...#.', '....#'],
  'slash-d': ['....#', '...#.', '..#..', '.#...', '#....'],

  // --- Progression / structure -------------------------------------------
  stairs: ['....#', '...##', '..###', '.####', '#####'],
  'stairs-d': ['#####', '####.', '###..', '##...', '#....'],
  blocks: ['##...', '##...', '.....', '...##', '...##'],
  'split-v': ['##.##', '##.##', '##.##', '##.##', '##.##'],
  'split-h': ['#####', '#####', '.....', '#####', '#####'],
  menu: ['#####', '.....', '#####', '.....', '#####'],
  'menu-alt': ['#####', '.....', '.###.', '.....', '#####'],
  list: ['#.###', '.....', '#.###', '.....', '#.###'],
  pause: ['.#.#.', '.#.#.', '.#.#.', '.#.#.', '.#.#.'],
  zap: ['#####', '...##', '..#..', '##...', '#####'],
  fork: ['#...#', '.#.#.', '..#..', '..#..', '..#..'],

  // --- Resource-ish -------------------------------------------------------
  drop: ['..#..', '.###.', '#####', '#####', '.###.'],
  flame: ['..#..', '..#..', '.###.', '.###.', '#####'],
  leaf: ['..##.', '.###.', '.###.', '..#..', '..#..'],
  food: ['#.#.#', '#.#.#', '#####', '..#..', '..#..'],
  grain: ['..#..', '.#.#.', '.#.#.', '..#..', '..#..'],
  bowl: ['.....', '#####', '#####', '.###.', '..#..'],
  // 7x7: "consumption" as a shaft feeding into a baseline (Lucide arrow-down-to-line), and a
  // funnel intake as an alternative.
  consume: [
    '...#...',
    '...#...',
    '...#...',
    '.#####.',
    '..###..',
    '...#...',
    '#######',
  ],
  funnel: [
    '#######',
    '#######',
    '.#####.',
    '.#####.',
    '..###..',
    '...#...',
    '...#...',
  ],

  // --- Actors -------------------------------------------------------------
  // 7x7: two figures side by side (square heads, a gap, then separate 3-wide bodies). The old
  // 5x5 merged both torsos into one block, so it read as a texture, not people.
  people: [
    '.......',
    '.##.##.',
    '.##.##.',
    '.......',
    '###.###',
    '###.###',
    '.......',
  ],
  star: ['..#..', '.###.', '#####', '.#.#.', '#...#'],
};

export const HUD_MARK_NAMES = Object.keys(HUD_GLYPHS);

export function HudMark({ icon, className }: { icon: string; className?: string }) {
  const rows = HUD_GLYPHS[icon] ?? HUD_GLYPHS.signal;
  const n = rows.length;
  const step = 100 / n;
  const cells: Array<{ r: number; c: number }> = [];

  rows.forEach((row, r) => {
    for (let c = 0; c < n; c++) {
      if (row[c] === '#') cells.push({ r, c });
    }
  });

  return (
    <view className={cn('hud-mark', className)}>
      {cells.map(({ r, c }) => (
        <view
          key={`${r}-${c}`}
          className="hud-mark__cell"
          style={{ left: `${c * step}%`, top: `${r * step}%`, width: `${step}%`, height: `${step}%` }}
        />
      ))}
    </view>
  );
}
