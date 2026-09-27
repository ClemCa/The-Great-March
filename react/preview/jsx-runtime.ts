import { jsx as reactJsx, jsxs as reactJsxs, Fragment } from 'react/jsx-runtime';
import { jsxDEV as reactJsxDEV } from 'react/jsx-dev-runtime';

/**
 * Browser preview only.
 *
 * ReactUnity's JSX runtime renders its own primitives (`view`, `text`, `scroll`, `image`, ...).
 * They are custom host components, not DOM tags: a `<image>` would be an inert unknown element in
 * a browser and would never show its sprite. This runtime maps each primitive onto the closest DOM
 * element and tags it with `data-rui` so the preview stylesheet can restore Yoga's defaults
 * (flex column, `flex-shrink: 0`, `border-box`). The app code itself is untouched.
 */
const TAG_MAP: Record<string, string> = {
  view: 'div',
  text: 'div',
  scroll: 'div',
  image: 'img',
  input: 'input',
  link: 'a',
};

/**
 * Unity dispatches `onDrag`/`onBeginDrag`/`onEndDrag` with a `PointerEventData` that carries
 * `delta` (screen pixels, y-up) and captures the pointer for the whole gesture. The DOM has no
 * such events, so synthesise them from pointer capture: the first move past the pointer-down fires
 * `onBeginDrag`, each move fires `onDrag` with the frame delta, and release fires `onEndDrag`.
 */
function withDrag(next: any) {
  if (!next.onDrag && !next.onBeginDrag && !next.onEndDrag) return;
  const { onBeginDrag: begin, onDrag: drag, onEndDrag: end } = next;
  const prevDown = next.onPointerDown;
  const prevMove = next.onPointerMove;
  const prevUp = next.onPointerUp;

  // The gesture state lives on the node, not in this closure: the app re-renders every bridge
  // frame, so a closure-scoped cursor would be thrown away mid-drag.
  const stateOf = (node: any) => (node.__ruiDrag ??= { last: null, started: false });

  next.onPointerDown = (event: any) => {
    prevDown?.(event);
    const state = stateOf(event.currentTarget);
    state.last = { x: event.clientX, y: event.clientY };
    state.started = false;
    event.currentTarget?.setPointerCapture?.(event.pointerId);
  };
  next.onPointerMove = (event: any) => {
    prevMove?.(event);
    const state = stateOf(event.currentTarget);
    if (!state.last) return;
    const payload = { delta: { x: event.clientX - state.last.x, y: state.last.y - event.clientY } };
    state.last = { x: event.clientX, y: event.clientY };
    if (!state.started) {
      state.started = true;
      begin?.(payload);
    }
    drag?.(payload);
  };
  next.onPointerUp = (event: any) => {
    prevUp?.(event);
    const state = stateOf(event.currentTarget);
    if (state.last && state.started) end?.({ delta: { x: 0, y: 0 } });
    state.last = null;
    state.started = false;
  };

  delete next.onBeginDrag;
  delete next.onDrag;
  delete next.onEndDrag;
}

function transform(type: string, props: any): [any, any] {
  const mapped = TAG_MAP[type];
  if (!mapped) return [type, props];

  const next: any = { ...props, 'data-rui': type };

  if (type === 'input') {
    // ReactUnity's input dispatches `onEndEdit`; the browser has no equivalent. Bridge it to
    // `onBlur` and keep the field editable by handing the initial `value` to `defaultValue`.
    if (typeof next.onEndEdit === 'function') {
      const onEndEdit = next.onEndEdit;
      next.onBlur = (event: any) => onEndEdit(event.currentTarget.value);
      // Unity also ends editing on Enter; the DOM input does not blur on its own.
      const prevKeyDown = next.onKeyDown;
      next.onKeyDown = (event: any) => {
        prevKeyDown?.(event);
        if (event.key === 'Enter') event.currentTarget.blur();
      };
      delete next.onEndEdit;
    }
    if (next.value !== undefined && next.onChange === undefined) {
      next.defaultValue = next.value;
      delete next.value;
    }
    // Unity-only input settings the DOM would reject as unknown attributes.
    for (const key of ['contentType', 'keyboardType', 'lineType', 'validation', 'characterLimit', 'lineLimit', 'richText']) {
      delete next[key];
    }
  }

  withDrag(next);

  return [mapped, next];
}

export function jsx(type: any, props: any, key?: any) {
  if (typeof type === 'string') {
    const [mapped, next] = transform(type, props);
    return reactJsx(mapped, next, key);
  }
  return reactJsx(type, props, key);
}

export function jsxs(type: any, props: any, key?: any) {
  if (typeof type === 'string') {
    const [mapped, next] = transform(type, props);
    return reactJsxs(mapped, next, key);
  }
  return reactJsxs(type, props, key);
}

export function jsxDEV(type: any, props: any, key?: any, isStatic?: boolean, source?: any, self?: any) {
  if (typeof type === 'string') {
    const [mapped, next] = transform(type, props);
    return reactJsxDEV(mapped, next, key, isStatic, source, self);
  }
  return reactJsxDEV(type, props, key, isStatic, source, self);
}

export { Fragment };
