import { createRoot } from 'react-dom/client';
import { HudMark, HUD_MARK_NAMES } from '../src/components/HudMark';
import '../src/index.scss';

/**
 * Browser preview only: renders every `HudMark` glyph so headings can be assigned a fitting
 * icon. Reachable at `?gallery=marks` (the toolbar's "glyphs" button links here).
 */
export function mountMarkGallery() {
  document.body.classList.add('gallery-open');

  const host = document.createElement('div');
  host.className = 'gallery';
  document.body.append(host);

  createRoot(host).render(
    <>
      <p className="gallery__head">
        HudMark glyphs — {HUD_MARK_NAMES.length} variants. Reference one by name in HudMark.
      </p>
      {HUD_MARK_NAMES.map((name) => (
        <div className="gallery__item" key={name}>
          <div className="gallery__mark">
            <HudMark icon={name} />
          </div>
          <span className="gallery__name">{name}</span>
        </div>
      ))}
    </>,
  );
}
