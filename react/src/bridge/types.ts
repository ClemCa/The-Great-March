export interface ResourceEntry {
  id: string;
  name: string;
  amount: number;
  advanced: boolean;
  icon: string;
  value: number;
}

export interface SlotEntry {
  index: number;
  used: boolean;
}

export interface GraphSnapshot {
  people: number[];
  resources: number[];
  facilities: number[];
}

export interface FacilitySlotEntry {
  resourceId: string;
  resourceName: string;
  resourceIcon: string;
  built: boolean;
  facilityId: string;
  facilityName: string;
  facilityIcon: string;
  progression: number;
}

export interface WildcardEntry {
  empty: boolean;
  facilityId: string;
  facilityName: string;
  facilityIcon: string;
  progression: number;
}

export interface FacilityOption {
  id: string;
  name: string;
  description: string;
  icon: string;
  wildcard: boolean;
  slotResource: string;
  canBuild: boolean;
  built: boolean;
}

export interface ShippingSnapshot {
  mode: string;
  selectedShip: number;
  hasPlayer: boolean;
  leaderInTransit: boolean;
  resourceId: string;
  resourceAdvanced: boolean;
  resourceAmount: number;
  peopleAmount: number;
  people: number;
  shipType: string;
  maxResource: number;
  maxPeople: number;
  availableFuel: number;
  ships: ShipEntry[];
}

export interface ShipEntry {
  index: number;
  type: string;
  icon: string;
  fuel: number;
  requiredFuel: number;
  canLaunch: boolean;
  canRefuel: boolean;
  refuelAmount: number;
}

export interface PlanetSnapshot {
  name: string;
  people: number;
  hasPlayer: boolean;
  temperate: string;
  fuel: number;
  ships: number;
  resources: ResourceEntry[];
  facilities: string[];
  graph: GraphSnapshot;
  facilitySlots: FacilitySlotEntry[];
  wildcards: WildcardEntry[];
  wildcardSlots: number;
  facilityOptions: FacilityOption[];
  prioritiesFood: number[];
  prioritiesFuel: number[];
  globalPrioritiesFood: number[];
  globalPrioritiesFuel: number[];
  shipping: ShippingSnapshot;
}

export interface QueueEntry {
  planet: string;
  type: string;
  progress: number;
  assigned: number;
  maxPeople: number;
  length: number;
  lengthLeft: number;
  speed: number;
}

export interface DialogSnapshot {
  visible: boolean;
  name: string;
  text: string;
  choices: string[];
}

export interface QuestSnapshot {
  visible: boolean;
  text: string;
}

export interface TooltipSnapshot {
  visible: boolean;
  title: string;
  description: string;
  info1: string;
  info2: string;
  x: number;
  y: number;
}

export interface SettingsSnapshot {
  master: number;
  ui: number;
  sfx: number;
  music: number;
  backgroundSound: boolean;
  showPrompt: boolean;
  vsync: boolean;
  fullscreen: boolean;
  resolution: string;
  quality: string;
  antialiasing: string;
  antialiasingQuality: string;
  mode: string;
  provider: string;
  baseUrl: string;
  model: string;
  apiKey: string;
  temperature: number;
  verbatim: number;
  summarized: number;
  thoughts: boolean;
}

export interface GameState {
  scene: string;
  isGameScene: boolean;
  gameOver: boolean;
  tutorial: boolean;
  paused: boolean;
  prompt: boolean;
  survivalTime: number;
  totalTime: number;
  systems: number;
  naturalResourcesUnits: number;
  advancedResourcesUnits: number;
  facilitiesCount: number;
  transformativeFacilitiesCount: number;
  slots: SlotEntry[];
  hasSelectedPlanet: boolean;
  selectedPlanet: PlanetSnapshot | null;
  queue: QueueEntry[];
  dialog: DialogSnapshot;
  quest: QuestSnapshot;
  tooltip: TooltipSnapshot;
  settings: SettingsSnapshot;
}
