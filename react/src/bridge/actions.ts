/**
 * Typed wrappers over the action delegates that `ReactGameBridge` registers on the
 * shared `Globals` object. Each call is a no-op when the Unity side has not
 * registered the action (e.g. while the app is not connected).
 */

function call<T>(name: string, ...args: unknown[]): T | undefined {
  const target = (Globals as Record<string, unknown>)[name];
  if (typeof target !== 'function') {
    if (target === undefined) console.warn(`[bridge] action '${name}' is not registered`);
    return undefined;
  }
  return (target as (...args: unknown[]) => T)(...args);
}

export const actions = {
  ping: (message: string) => call<string>('ping', message),
  newGame: () => call<void>('newGame'),
  loadGame: (slot: number) => call<void>('loadGame', slot),
  tutorial: () => call<void>('tutorial'),
  exit: () => call<void>('exit'),
  restart: () => call<void>('restart'),
  goToMainMenu: () => call<void>('goToMainMenu'),
  saveGame: (slot: number) => call<void>('saveGame', slot),
  slotUsed: (slot: number) => call<boolean>('slotUsed', slot),
  setPaused: (paused: boolean) => call<void>('setPaused', paused),
  togglePause: () => call<void>('togglePause'),
  setPrompt: (enabled: boolean) => call<void>('setPrompt', enabled),
  click: () => call<void>('click'),
  buildFacility: (facilityId: string) => call<void>('buildFacility', facilityId),
  movePriority: (display: number, index: number, delta: number, global: boolean) =>
    call<void>('movePriority', display, index, delta, global),
  dialogSelect: (index: number) => call<void>('dialogSelect', index),
  dialogFlip: () => call<void>('dialogFlip'),
  startDialogue: (dialogue: string) => call<void>('startDialogue', dialogue),
  promptTarget: (kind: string, id: string) => call<void>('promptTarget', kind, id),
  clearPrompt: () => call<void>('clearPrompt'),
  setSettingFloat: (key: string, value: number) => call<void>('setSettingFloat', key, value),
  setSettingBool: (key: string, value: boolean) => call<void>('setSettingBool', key, value),
  setLlmSetting: (key: string, value: string) => call<void>('setLlmSetting', key, value),
  cycleSetting: (key: string) => call<void>('cycleSetting', key),
  shippingSetMode: (mode: string) => call<void>('shippingSetMode', mode),
  shippingSelectShip: (index: number) => call<void>('shippingSelectShip', index),
  shippingRefuel: () => call<void>('shippingRefuel'),
  shippingSelectResource: (id: string, advanced: boolean) =>
    call<void>('shippingSelectResource', id, advanced),
  shippingSetResourceAmount: (value: number) => call<void>('shippingSetResourceAmount', value),
  shippingSetPeopleAmount: (value: number) => call<void>('shippingSetPeopleAmount', value),
  shippingLaunch: () => call<void>('shippingLaunch'),
  shippingPlayerMove: () => call<void>('shippingPlayerMove'),
  shippingLaunchPeople: () => call<void>('shippingLaunchPeople'),
  shippingLaunchResource: () => call<void>('shippingLaunchResource'),
  shippingReturn: () => call<void>('shippingReturn'),
  openUrl: (url: string) => call<void>('openUrl', url),
  openSettings: () => call<void>('openSettings'),
};
