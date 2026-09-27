import * as React from 'react';
import { createRoot } from 'react-dom/client';
import { globals, getVersion, subscribe } from './globals';

/**
 * Browser preview only: the slice of `@reactunity/renderer` the app actually imports, backed by
 * the DOM instead of Unity. `render` mounts into the preview stage and `useGlobals` reads the
 * same `state` string the Unity bridge would publish.
 */
export function render(element: React.ReactElement) {
  const container = document.getElementById('app-root');
  if (!container) throw new Error('#app-root is missing from the preview page');
  const root = createRoot(container);
  root.render(element);
  return root;
}

export function useGlobals(): Record<string, unknown> {
  useSyncExternalStoreCompat();
  return globals;
}

export function useGlobalsContext(): Record<string, unknown> {
  return globals;
}

export function GlobalsProvider({ children }: { children?: React.ReactNode }) {
  return React.createElement(React.Fragment, null, children);
}

// `useSyncExternalStore` needs a changing snapshot; the version counter drives re-renders while
// `globals` itself stays referentially stable.
function useSyncExternalStoreCompat() {
  React.useSyncExternalStore(subscribe, getVersion, getVersion);
}

export default { render, useGlobals, useGlobalsContext, GlobalsProvider };
