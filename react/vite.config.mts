import { defineConfig, type Plugin } from 'vite';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import tailwindcss from 'tailwindcss';
import tailwindConfig from './tailwind.config.js';

/**
 * `index.scss` declares Chakra Petch with a Unity-only `resource:` URL. The browser cannot resolve
 * that scheme, but it still registers a face at the default weight, which shadows the real preview
 * face and makes everything fall back to serif. Drop those blocks in the preview pipeline.
 */
function stripUnityFontFace(): Plugin {
  return {
    name: 'preview:strip-unity-font-face',
    enforce: 'pre',
    transform(code, id) {
      if (!id.includes('.scss') && !id.includes('.css')) return null;
      if (!code.includes('resource:')) return null;
      return code.replace(/@font-face\s*\{[^}]*resource:[^}]*\}/g, '');
    },
  };
}

const root = path.dirname(fileURLToPath(import.meta.url));
const preview = path.resolve(root, 'preview');
const jsxShim = path.resolve(preview, 'jsx-runtime.ts');
const rendererShim = path.resolve(preview, 'reactunity-shim.ts');

/**
 * Standalone browser preview for the ReactUnity UI.
 *
 * It never touches the Unity build: `react-unity-scripts start` serves the bundle to a Unity
 * WebGL player, which is painful to iterate on visually. This config instead renders the same
 * `src/` app against real DOM (see `preview/jsx-runtime.ts` for the primitive mapping and
 * `preview/globals.ts` for the mocked bridge) so a normal browser can be used for design work.
 *
 * Run with: `npm run preview`
 */
export default defineConfig({
  root: preview,
  publicDir: false,
  plugins: [stripUnityFontFace()],
  esbuild: {
    jsx: 'automatic',
    jsxImportSource: '@preview-jsx',
  },
  resolve: {
    alias: {
      // JSX automatic runtime -> the primitive-to-DOM shim.
      '@preview-jsx/jsx-runtime': jsxShim,
      '@preview-jsx/jsx-dev-runtime': jsxShim,
      '@reactunity/renderer/ugui/jsx-runtime': jsxShim,
      '@reactunity/renderer/ugui/jsx-dev-runtime': jsxShim,
      // Any remaining ReactUnity import (useGlobals, render) -> the DOM shim.
      '@reactunity/renderer/ugui': rendererShim,
      '@reactunity/renderer': rendererShim,
    },
  },
  css: {
    postcss: {
      plugins: [
        tailwindcss({
          ...tailwindConfig,
          content: [
            path.resolve(root, 'src/**/*.{js,jsx,ts,tsx,html}'),
            path.resolve(preview, '**/*.{js,jsx,ts,tsx,html}'),
          ],
        }),
      ],
    },
  },
  server: {
    port: 5173,
    strictPort: false,
    fs: {
      // The Chakra Petch font lives in the Unity project, one level above `react/`.
      allow: [root, path.resolve(root, '..')],
    },
  },
  build: {
    outDir: path.resolve(preview, 'dist'),
    emptyOutDir: true,
  },
});
