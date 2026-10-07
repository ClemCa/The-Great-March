import type {
  GameState,
  GraphSnapshot,
  PlanetSnapshot,
  SettingsSnapshot,
  ShippingSnapshot,
} from '../src/bridge/types';

/* ------------------------------------------------------------------ */
/* Sample data                                                         */
/* ------------------------------------------------------------------ */

function settings(overrides: Partial<SettingsSnapshot> = {}): SettingsSnapshot {
  return {
    master: 0.8,
    ui: 0.7,
    sfx: 0.9,
    music: 0.35,
    backgroundSound: true,
    showPrompt: true,
    vsync: true,
    fullscreen: false,
    resolution: '1920x1080 @60Hz',
    quality: 'High',
    antialiasing: 'SMAA',
    antialiasingQuality: 'High',
    thoughtExploration: true,
    mode: 'LLM',
    provider: 'OpenAI Compatible',
    baseUrl: 'http://localhost:11434',
    model: 'llama3',
    apiKey: '',
    temperature: 0.7,
    verbatim: 4,
    summarized: 8,
    thoughts: false,
    selectors: [
      { key: 'resolution', options: ['Native', '1280x720 @60Hz', '1920x1080 @60Hz', '2560x1440 @144Hz'], index: 2 },
      { key: 'quality', options: ['Low', 'Medium', 'High', 'Ultra'], index: 2 },
      { key: 'antialiasing', options: ['None', 'FXAA', 'SMAA'], index: 2 },
      { key: 'antialiasingQuality', options: ['Low', 'Medium', 'High'], index: 2 },
      { key: 'mode', options: ['Raw', 'LLM'], index: 1 },
      { key: 'provider', options: ['Ollama', 'OpenAI Compatible'], index: 1 },
    ],
    ...overrides,
  };
}

function graph(seed: number, length = 42): GraphSnapshot {
  const people: number[] = [];
  const resources: number[] = [];
  const facilities: number[] = [];
  for (let i = 0; i < length; i++) {
    people.push(Math.round(2 + 4 * Math.abs(Math.sin((i + seed) / 5))));
    resources.push(Math.round(1 + 6 * Math.abs(Math.sin((i + seed) / 3.5))));
    facilities.push(Math.round(1 + 3 * Math.abs(Math.cos((i + seed) / 6))));
  }
  return { people, resources, facilities };
}

function planet(name: string, seed = 0): PlanetSnapshot {
  return {
    name,
    people: 7,
    hasPlayer: true,
    temperate: 'Temperate',
    fuel: 34,
    ships: 3,
    resources: [
      { id: 'Food', name: 'Food', amount: 18, advanced: false, icon: 'food_resource', value: 2 },
      { id: 'Water', name: 'Water', amount: 9, advanced: false, icon: 'water_resource', value: 1 },
      { id: 'Metal', name: 'Metal', amount: 5, advanced: false, icon: 'metal_resource', value: 3 },
      { id: 'Gas', name: 'Gas', amount: 4, advanced: false, icon: 'gas_resource', value: 3 },
      { id: 'Oil', name: 'Oil', amount: 2, advanced: false, icon: 'oil_resource', value: 4 },
      { id: 'Hydrogen', name: 'Hydrogen', amount: 1, advanced: false, icon: 'hydrogen_resource', value: 5 },
      { id: 'Plant', name: 'Plant', amount: 3, advanced: false, icon: 'plant_resource', value: 2 },
      { id: 'Animal', name: 'Animal', amount: 2, advanced: false, icon: 'animal_resource', value: 2 },
      { id: 'PreparedFood', name: 'Prepared food', amount: 4, advanced: true, icon: 'prepared_food', value: 6 },
      { id: 'HighEfficiencyFuel', name: 'High-efficiency fuel', amount: 1, advanced: true, icon: 'high-efficiency_fuel', value: 12 },
    ],
    facilities: ['Farm', 'Mine', 'WaterPump'],
    graph: graph(seed),
    facilitySlots: [
      {
        resourceId: 'Food',
        resourceName: 'Food',
        resourceIcon: 'food_resource',
        built: true,
        facilityId: 'Farm',
        facilityName: 'Farm',
        facilityIcon: 'Farm',
        progression: 0.62,
      },
      {
        resourceId: 'Metal',
        resourceName: 'Metal',
        resourceIcon: 'metal_resource',
        built: true,
        facilityId: 'Mine',
        facilityName: 'Mine',
        facilityIcon: 'mine_facility',
        progression: 0.2,
      },
      {
        resourceId: 'Water',
        resourceName: 'Water',
        resourceIcon: 'water_resource',
        built: false,
        facilityId: '',
        facilityName: '',
        facilityIcon: '',
        progression: 0,
      },
      {
        resourceId: 'Gas',
        resourceName: 'Gas',
        resourceIcon: 'gas_resource',
        built: false,
        facilityId: '',
        facilityName: '',
        facilityIcon: '',
        progression: 0,
      },
    ],
    wildcards: [
      { empty: false, facilityId: 'Refinery', facilityName: 'Refinery', facilityIcon: 'Refinery', progression: 0.45 },
    ],
    wildcardSlots: 2,
    facilityOptions: [
      { id: 'Farm', name: 'Farm', description: 'Produces food.', icon: 'Farm', wildcard: false, slotResource: 'Food', canBuild: true, built: true },
      { id: 'Greenhouse', name: 'Greenhouse', description: 'Produces food faster.', icon: 'greenhouse_facility', wildcard: false, slotResource: 'Food', canBuild: true, built: false },
      { id: 'Mine', name: 'Mine', description: 'Extracts metal.', icon: 'mine_facility', wildcard: false, slotResource: 'Metal', canBuild: false, built: false },
      { id: 'Refinery', name: 'Refinery', description: 'Transforms resources.', icon: 'Refinery', wildcard: true, slotResource: 'Metal', canBuild: true, built: false },
      { id: 'Kitchen', name: 'Kitchen', description: 'Cooks prepared food.', icon: 'Kitchen', wildcard: true, slotResource: 'Food', canBuild: true, built: false },
    ],
    prioritiesFood: [0, 1, 2, 3],
    prioritiesFuel: [3, 2, 4, 5],
    globalPrioritiesFood: [0, 1, 2],
    globalPrioritiesFuel: [3, 2],
    shipping: {
      mode: 'ships',
      selectedShip: 0,
      hasPlayer: true,
      leaderInTransit: false,
      resourceId: '',
      resourceAdvanced: false,
      resourceAmount: 0,
      peopleAmount: 0,
      people: 7,
      shipType: 'Cargo',
      maxResource: 5,
      maxPeople: 5,
      availableFuel: 34,
      ships: [
        { index: 0, type: 'Cargo', name: 'Hauler', icon: 'Factory', fuel: 10, requiredFuel: 8, canLaunch: true, canRefuel: false, refuelAmount: 0 },
        { index: 1, type: 'Passenger', name: '', icon: 'Farm', fuel: 3, requiredFuel: 6, canLaunch: false, canRefuel: true, refuelAmount: 3 },
        { index: 2, type: 'Presidential', name: 'Air Force One', icon: 'Kitchen', fuel: 12, requiredFuel: 12, canLaunch: true, canRefuel: false, refuelAmount: 0 },
      ],
    },
  };
}

const slots = [
  { index: 1, used: true },
  { index: 2, used: true },
  { index: 3, used: false },
  { index: 4, used: false },
  { index: 5, used: false },
];

function gameHud(): GameState {
  return {
    scene: 'Game',
    isGameScene: true,
    gameOver: false,
    tutorial: false,
    paused: false,
    prompt: true,
    survivalTime: 12,
    totalTime: 24,
    systems: 3,
    naturalResourcesUnits: 41,
    advancedResourcesUnits: 5,
    facilitiesCount: 4,
    transformativeFacilitiesCount: 1,
    slots,
    hasSelectedPlanet: true,
    selectedPlanet: planet('Kepler-442b', 1),
    queue: [
      { planet: 'Kepler-442b', type: 'Building', progress: 0.35, assigned: 3, maxPeople: 5, length: 140, lengthLeft: 91, speed: 1.5 },
      { planet: 'Kepler-442b', type: 'Gathering', progress: 0.8, assigned: 2, maxPeople: 4, length: 60, lengthLeft: 12, speed: 1 },
    ],
    dialog: { visible: false, name: '', text: '', choices: [] },
    quest: { visible: true, text: 'Establish a second settlement before the next cycle.' },
    tooltip: { visible: false, title: '', description: '', info1: '', info2: '', x: 0, y: 0 },
    settings: settings(),
  };
}

const SCENARIOS: Record<string, () => GameState | null> = {
  'main-menu': () => ({
    ...gameHud(),
    scene: 'MainMenu',
    isGameScene: false,
    hasSelectedPlanet: false,
    selectedPlanet: null,
    queue: [],
    quest: { visible: false, text: '' },
    slots,
  }),

  'hud': gameHud,

  'paused': () => ({ ...gameHud(), paused: true }),

  'tutorial': () => ({
    ...gameHud(),
    tutorial: true,
    quest: { visible: true, text: 'Welcome, President. Learn to build your first facility.' },
    dialog: {
      visible: true,
      name: 'Advisor',
      text: 'The colony needs food before the next silence. Shall we break ground on a farm?',
      choices: ['Build a farm', 'Explain later', 'Not now'],
    },
  }),

  'lose': () => ({
    ...gameHud(),
    scene: 'Game',
    isGameScene: true,
    gameOver: true,
    survivalTime: 42,
    totalTime: 61,
    systems: 2,
    naturalResourcesUnits: 17,
    advancedResourcesUnits: 0,
    facilitiesCount: 3,
    transformativeFacilitiesCount: 0,
    hasSelectedPlanet: false,
    selectedPlanet: null,
    queue: [],
    quest: { visible: false, text: '' },
  }),

  'empty': () => null,
};

/* ------------------------------------------------------------------ */
/* Mutable store + mocked bridge actions                               */
/* ------------------------------------------------------------------ */

const listeners = new Set<() => void>();
let version = 0;
let current: GameState | null = SCENARIOS['hud']();

function publish() {
  version += 1;
  listeners.forEach((listener) => listener());
}

export function subscribe(listener: () => void) {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export function getVersion() {
  return version;
}

export const scenarioNames = Object.keys(SCENARIOS);

export function setScenario(name: string) {
  if (!SCENARIOS[name]) return;
  current = SCENARIOS[name]();
  publish();
}

function update(mutate: (state: GameState) => void) {
  if (!current) return;
  const next: GameState = JSON.parse(JSON.stringify(current));
  mutate(next);
  current = next;
  publish();
}

function log(name: string, ...args: unknown[]) {
  // eslint-disable-next-line no-console
  console.log(`[preview] ${name}`, ...args);
}

function shipOf(state: GameState) {
  return state.selectedPlanet?.shipping;
}

function selectedShipEntry(shipping: ShippingSnapshot) {
  return shipping.ships.find((ship) => ship.index === shipping.selectedShip);
}

/**
 * Re-derives the shipping snapshot the way `ReactGameBridge.BuildShipping` does, so the preview's
 * ship-first flow (fuel, capacity, resource stock) behaves like the game while clicking around.
 */
function syncShipping(state: GameState) {
  const shipping = shipOf(state);
  const planet = state.selectedPlanet;
  if (!shipping || !planet) return;

  const ship = selectedShipEntry(shipping);
  const stock = shipping.resourceId
    ? (planet.resources.find((r) => r.id === shipping.resourceId && r.advanced === shipping.resourceAdvanced)?.amount ?? 0)
    : 0;

  shipping.people = planet.people;
  shipping.availableFuel = planet.fuel;
  for (const entry of shipping.ships) {
    entry.canLaunch = entry.fuel >= entry.requiredFuel;
    entry.refuelAmount = Math.max(0, entry.requiredFuel - entry.fuel);
    entry.canRefuel = !entry.canLaunch && entry.refuelAmount < planet.fuel;
  }

  if (!ship) {
    shipping.shipType = '';
    shipping.maxResource = 0;
    shipping.maxPeople = 0;
    return;
  }

  shipping.shipType = ship.type;
  if (ship.type === 'Cargo') {
    shipping.maxResource = stock <= 0 ? 0 : Math.min(stock, Math.max(0, 11 - shipping.peopleAmount));
    shipping.maxPeople = Math.min(planet.people, Math.max(0, 11 - shipping.resourceAmount), 5);
  } else if (ship.type === 'Passenger') {
    shipping.maxResource = stock <= 0 ? 0 : Math.min(stock, Math.max(0, 15 - shipping.peopleAmount), 5);
    shipping.maxPeople = Math.min(planet.people, Math.max(0, 15 - shipping.resourceAmount));
  } else {
    shipping.maxResource = 0;
    shipping.maxPeople = Math.min(planet.people, 5);
  }
}

const noopAction = () => undefined;

export const globals: Record<string, unknown> = {
  get state() {
    return current ? JSON.stringify(current) : '';
  },

  ping: (message: string) => `pong:${message}`,

  newGame: () => setScenario('hud'),
  loadGame: (slot: number) => {
    log('loadGame', slot);
    setScenario('hud');
  },
  tutorial: () => setScenario('tutorial'),
  exit: () => log('exit'),
  restart: () => setScenario('hud'),
  goToMainMenu: () => setScenario('main-menu'),

  saveGame: (slot: number) => log('saveGame', slot),
  slotUsed: () => false,

  setPaused: (paused: boolean) => update((state) => void (state.paused = paused)),
  togglePause: () => update((state) => void (state.paused = !state.paused)),
  setPrompt: (enabled: boolean) =>
    update((state) => {
      state.prompt = enabled;
      state.settings.showPrompt = enabled;
    }),
  click: () => undefined,

  buildFacility: (facilityId: string) =>
    update((state) => {
      const p = state.selectedPlanet;
      if (!p) return;
      const option = p.facilityOptions.find((o) => o.id === facilityId);
      if (!option) return;
      if (option.wildcard) {
        p.wildcards.push({ empty: false, facilityId: option.id, facilityName: option.name, facilityIcon: option.icon, progression: 0 });
      } else {
        const slot = p.facilitySlots.find((s) => s.resourceId === option.slotResource && !s.built);
        if (slot) {
          slot.built = true;
          slot.facilityId = option.id;
          slot.facilityName = option.name;
          slot.facilityIcon = option.icon;
          slot.progression = 0.01;
        }
      }
      option.built = true;
    }),

  movePriority: (display: number, index: number, delta: number, global: boolean) =>
    update((state) => {
      const p = state.selectedPlanet;
      if (!p) return;
      const list = global
        ? display === 1
          ? p.globalPrioritiesFuel
          : p.globalPrioritiesFood
        : display === 1
          ? p.prioritiesFuel
          : p.prioritiesFood;
      const target = index + delta;
      if (index < 0 || index >= list.length || target < 0 || target >= list.length) return;
      [list[index], list[target]] = [list[target], list[index]];
    }),

  dialogSelect: (index: number) => {
    log('dialogSelect', index);
    update((state) => {
      state.dialog.visible = false;
      state.dialog.choices = [];
    });
  },
  dialogFlip: noopAction,
  startDialogue: (dialogue: string) =>
    update((state) => {
      state.dialog.visible = true;
      state.dialog.name = 'Advisor';
      state.dialog.text = dialogue;
    }),

  promptTarget: (kind: string, id: string) =>
    update((state) => {
      if (!state.prompt) return;
      state.tooltip.visible = true;
      state.tooltip.x = 0.5;
      state.tooltip.y = 0.35;
      state.tooltip.title = id || (kind === 'wildcard' ? 'Plot of land' : kind);
      state.tooltip.description = `${kind} sample tooltip for the preview`;
      state.tooltip.info1 = 'Normally resolved by the Unity registry';
      state.tooltip.info2 = '';
    }),
  clearPrompt: () => update((state) => void (state.tooltip.visible = false)),

  setSettingFloat: (key: string, value: number) =>
    update((state) => void ((state.settings as unknown as Record<string, number>)[key] = value)),
  setSettingBool: (key: string, value: boolean) =>
    update((state) => void ((state.settings as unknown as Record<string, boolean>)[key] = value)),
  setLlmSetting: (key: string, value: string) =>
    update((state) => void ((state.settings as unknown as Record<string, string>)[key] = value)),
  cycleSetting: (key: string) => log('cycleSetting', key),
  selectSetting: (key: string, index: number) =>
    update((state) => {
      const selector = state.settings.selectors?.find((entry) => entry.key === key);
      if (!selector || index < 0 || index >= selector.options.length) return;
      selector.index = index;
      const option = selector.options[index];
      if (key === 'resolution') state.settings.resolution = option;
      else if (key === 'quality') state.settings.quality = option;
      else if (key === 'antialiasing') state.settings.antialiasing = option;
      else if (key === 'antialiasingQuality') state.settings.antialiasingQuality = option;
      else if (key === 'mode') state.settings.mode = option;
      else if (key === 'provider') state.settings.provider = option;
    }),

  shippingSetMode: (mode: string) =>
    update((state) => {
      const shipping = shipOf(state);
      if (!shipping) return;
      shipping.mode = mode || 'menu';
      if (shipping.mode === 'menu' || shipping.mode === 'ships') {
        shipping.selectedShip = -1;
        shipping.resourceId = '';
        shipping.resourceAdvanced = false;
        shipping.resourceAmount = 0;
        shipping.peopleAmount = 0;
        syncShipping(state);
      }
    }),
  shippingSelectShip: (index: number) =>
    update((state) => {
      const shipping = shipOf(state);
      if (!shipping) return;
      shipping.selectedShip = index;
      shipping.resourceId = '';
      shipping.resourceAdvanced = false;
      shipping.resourceAmount = 0;
      shipping.peopleAmount = 0;
      syncShipping(state);
    }),
  shippingRefuel: () =>
    update((state) => {
      const shipping = shipOf(state);
      const planet = state.selectedPlanet;
      const ship = shipping ? selectedShipEntry(shipping) : undefined;
      if (!shipping || !planet || !ship) return;
      const missing = Math.max(0, ship.requiredFuel - ship.fuel);
      if (missing <= 0) return;
      planet.fuel = Math.max(0, planet.fuel - missing);
      ship.fuel += missing;
      syncShipping(state);
    }),
  shippingRenameShip: (index: number, name: string) =>
    update((state) => {
      const shipping = shipOf(state);
      const ship = shipping?.ships.find((entry) => entry.index === index);
      if (ship) ship.name = name.trim().slice(0, 24);
    }),
  shippingSelectResource: (id: string, advanced: boolean) =>
    update((state) => {
      const shipping = shipOf(state);
      if (!shipping) return;
      shipping.resourceId = id;
      shipping.resourceAdvanced = advanced;
      shipping.resourceAmount = id ? 1 : 0;
      syncShipping(state);
    }),
  shippingSetResourceAmount: (value: number) =>
    update((state) => {
      const shipping = shipOf(state);
      if (shipping) shipping.resourceAmount = Math.max(0, Math.min(value, shipping.maxResource));
      syncShipping(state);
    }),
  shippingSetPeopleAmount: (value: number) =>
    update((state) => {
      const shipping = shipOf(state);
      if (shipping) shipping.peopleAmount = Math.max(0, Math.min(value, shipping.maxPeople));
      syncShipping(state);
    }),
  shippingLaunch: () => log('shippingLaunch'),
  shippingLaunchPresident: () => {
    log('shippingLaunchPresident');
    update((state) => {
      const shipping = shipOf(state);
      if (!shipping) return;
      shipping.ships = shipping.ships.filter((ship) => ship.index !== shipping.selectedShip);
      shipping.selectedShip = -1;
      shipping.mode = 'ships';
      syncShipping(state);
    });
  },
  shippingPlayerMove: () => log('shippingPlayerMove'),
  shippingLaunchPeople: () => log('shippingLaunchPeople'),
  shippingLaunchResource: () => log('shippingLaunchResource'),
  shippingReturn: () =>
    update((state) => {
      const shipping = shipOf(state);
      if (shipping) shipping.mode = 'ships';
    }),

  openUrl: (url: string) => log('openUrl', url),
  openSettings: () => log('openSettings'),
};
