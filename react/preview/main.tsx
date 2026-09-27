import './preview.css';
import { globals, scenarioNames, setScenario } from './globals';

/**
 * Preview entry: wires the mocked Unity `Globals`, builds the scenario toolbar, then boots the
 * real app (`src/index.tsx`) unchanged. Kept as a dynamic import so `Globals` exists before the
 * bridge's action wrappers capture it.
 *
 * URL params:
 *   ?scenario=hud          pick the starting scenario (default `hud`)
 *   ?clean=1               hide the toolbar (used for screenshots)
 *   ?clicks=Settings,Credits
 *                          click through UI in order; a token is matched against button text,
 *                          or used as a CSS selector when it starts with `.`/`#`. Lets the
 *                          local-only panels (settings, submenus) be screenshotted.
 *   ?aspect=21:9           force a stage aspect ratio; default follows the window's own ratio
 */
(globalThis as unknown as { Globals: unknown }).Globals = globals;

const params = new URLSearchParams(window.location.search);
if (params.has('clean')) document.body.classList.add('clean');

const REFERENCE_HEIGHT = 1080;

function parseAspect(raw: string | null): number | null {
  if (!raw) return null;
  const [w, h] = raw.split(':').map(Number);
  return w > 0 && h > 0 ? w / h : null;
}

let forcedAspect = parseAspect(params.get('aspect'));

/**
 * Emulates the Unity CanvasScaler (ScaleWithScreenSize, reference 1920x1080, match height):
 * the logical height stays 1080, the logical width flexes with the aspect ratio, then the
 * whole stage is scaled to fit the window. With no `?aspect` the window's own ratio is used,
 * so resizing the browser reproduces what Unity does at that resolution.
 */
function fitStage() {
  const stage = document.getElementById('stage');
  if (!stage) return;
  const aspect = forcedAspect ?? window.innerWidth / window.innerHeight;
  const width = REFERENCE_HEIGHT * aspect;
  const height = REFERENCE_HEIGHT;
  const scale = Math.min(window.innerWidth / width, window.innerHeight / height);
  stage.style.width = `${width}px`;
  stage.style.height = `${height}px`;
  stage.style.left = `${Math.max(0, (window.innerWidth - width * scale) / 2)}px`;
  stage.style.top = `${Math.max(0, (window.innerHeight - height * scale) / 2)}px`;
  stage.style.transform = `scale(${scale})`;
}

const ASPECTS: Array<[string, number | null]> = [
  ['fill', null],
  ['16:9', 16 / 9],
  ['21:9', 21 / 9],
  ['4:3', 4 / 3],
  ['1:1', 1],
  ['9:16', 9 / 16],
];

function buildToolbar(active: string) {
  const toolbar = document.getElementById('toolbar');
  if (!toolbar) return;

  const label = document.createElement('span');
  label.className = 'label';
  label.textContent = 'scenario';
  toolbar.append(label);

  const buttons: HTMLButtonElement[] = [];
  const select = (name: string) => {
    setScenario(name);
    buttons.forEach((button) => button.classList.toggle('active', button.dataset.scenario === name));
  };

  for (const name of scenarioNames) {
    const button = document.createElement('button');
    button.textContent = name;
    button.dataset.scenario = name;
    button.addEventListener('click', () => select(name));
    toolbar.append(button);
    buttons.push(button);
  }

  buttons.forEach((button) => button.classList.toggle('active', button.dataset.scenario === active));

  const aspectLabel = document.createElement('span');
  aspectLabel.className = 'label';
  aspectLabel.textContent = 'aspect';
  toolbar.append(aspectLabel);

  const activeAspect = params.get('aspect') ?? 'fill';
  for (const [name, value] of ASPECTS) {
    const button = document.createElement('button');
    button.textContent = name;
    button.dataset.aspect = name;
    button.classList.toggle('active', name === activeAspect);
    button.addEventListener('click', () => {
      forcedAspect = value;
      fitStage();
      toolbar.querySelectorAll<HTMLButtonElement>('[data-aspect]').forEach((other) => {
        other.classList.toggle('active', other.dataset.aspect === name);
      });
    });
    toolbar.append(button);
  }

  const spacer = document.createElement('span');
  spacer.className = 'label';
  toolbar.append(spacer);

  const clean = document.createElement('button');
  clean.textContent = 'hide toolbar';
  clean.addEventListener('click', () => document.body.classList.toggle('clean'));
  toolbar.append(clean);

  return select;
}

window.addEventListener('resize', fitStage);
document.addEventListener('keydown', (event) => {
  if (event.key === 't' || event.key === 'T') document.body.classList.toggle('clean');
});

fitStage();

const requested = params.get('scenario') ?? 'hud';
const start = scenarioNames.includes(requested) ? requested : 'hud';
const select = buildToolbar(start);
select?.(start);

function findTarget(token: string): HTMLElement | null {
  const root = document.getElementById('app-root');
  if (!root) return null;
  if (token.startsWith('.') || token.startsWith('#')) {
    return root.querySelector<HTMLElement>(token);
  }
  const buttons = Array.from(root.querySelectorAll<HTMLElement>('button'));
  return buttons.find((button) => (button.textContent ?? '').trim() === token) ?? null;
}

async function runClicks(tokens: string[]) {
  for (const token of tokens) {
    await new Promise((resolve) => setTimeout(resolve, 450));
    findTarget(token)?.click();
  }
}

const clicks = params.get('clicks');
void import('../src/index.tsx').then(() => {
  if (clicks) void runClicks(clicks.split(',').map((token) => token.trim()).filter(Boolean));
});
