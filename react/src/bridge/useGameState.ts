import { useMemo } from 'react';
import { useGlobals } from '@reactunity/renderer';
import type { GameState } from './types';

/**
 * Reads the JSON snapshot published by `ReactGameBridge` and re-renders whenever
 * the Unity side pushes a new one.
 */
export function useGameState(): GameState | null {
  const globals = useGlobals();
  const raw = globals.state as string | undefined;

  return useMemo(() => {
    if (!raw) return null;
    try {
      return JSON.parse(raw) as GameState;
    } catch {
      return null;
    }
  }, [raw]);
}
