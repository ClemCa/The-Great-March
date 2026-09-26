import { useState } from 'react';
import { iconFor } from '../assets/icons';
import { actions } from '../bridge/actions';
import type { GameState } from '../bridge/types';
import { cn } from '../lib/cn';
import { PlanetGraph } from './PlanetGraph';
import { FacilityBuildMenu } from './FacilityBuildMenu';
import { PriorityMenu } from './PriorityMenu';
import { ShippingMenu } from './ShippingMenu';

/**
 * Right-hand planet panel. Mirrors the original `PlanetMenu`: it slides in while a planet is
 * selected and shows the planet name/people, the population graph, the facility slots, the
 * inventory and the shipping/consumption entries.
 */
export function PlanetPanel({ state }: { state: GameState }) {
  // JsonUtility serializes a null `selectedPlanet` as a default-valued object, so the
  // authoritative "is a planet selected" flag is `hasSelectedPlanet`.
  const planet = state.hasSelectedPlanet ? state.selectedPlanet : null;
  const [buildResource, setBuildResource] = useState<string | null>(null);
  const [wildcardOpen, setWildcardOpen] = useState(false);
  const [prioritiesOpen, setPrioritiesOpen] = useState(false);
  const [shippingOpen, setShippingOpen] = useState(false);

  const open = planet != null;

  const slotOptions = planet && buildResource
    ? planet.facilityOptions.filter((o) => !o.wildcard && o.slotResource === buildResource)
    : [];
  const wildcardOptions = planet ? planet.facilityOptions.filter((o) => o.wildcard) : [];

  return (
    <view className={cn('planet-panel', open && 'planet-panel--open')}>
      {planet && (
        <>
          <view className="planet-panel__title">
            <text className="planet-panel__title-text">{planet.name}</text>
          </view>

          <view className="planet-panel__baseinfo">
            <text className="planet-panel__baseinfo-text">{`Name: ${planet.name}`}</text>
            <text className="planet-panel__baseinfo-text">{`People: ${planet.people}`}</text>
          </view>

          <view className="planet-panel__graph">
            <PlanetGraph graph={planet.graph} />
          </view>

          <view className="planet-panel__section planet-panel__facilities">
            <view className="planet-panel__section-title">
              <text>Facilities</text>
            </view>
            <scroll className="planet-panel__scroll">
              <view className="facility-grid">
                {planet.facilitySlots.map((slot) => (
                  <button
                    key={slot.resourceId}
                    className="facility-slot"
                    onMouseEnter={() => actions.promptTarget('slot', slot.resourceId)}
                    onMouseLeave={actions.clearPrompt}
                    onClick={slot.built ? undefined : () => setBuildResource(slot.resourceId)}
                  >
                    <image
                      className="facility-slot__icon"
                      src={iconFor(slot.built ? slot.facilityIcon : slot.resourceIcon)}
                    />
                    {slot.built && slot.progression > 0 && (
                      <view
                        className="facility-slot__progress"
                        style={{ height: `${Math.round(70 * Math.min(1, slot.progression))}px` }}
                      />
                    )}
                  </button>
                ))}

                {planet.wildcards.map((wildcard, index) => (
                  <button
                    key={`wildcard-${index}`}
                    className="facility-slot"
                    onMouseEnter={() => actions.promptTarget('wildcard', wildcard.facilityId)}
                    onMouseLeave={actions.clearPrompt}
                  >
                    <image className="facility-slot__icon" src={iconFor(wildcard.facilityIcon)} />
                    {wildcard.progression > 0 && (
                      <view
                        className="facility-slot__progress"
                        style={{ height: `${Math.round(70 * Math.min(1, wildcard.progression))}px` }}
                      />
                    )}
                  </button>
                ))}

                {Array.from({ length: Math.max(0, planet.wildcardSlots) }).map((_, index) => (
                  <button
                    key={`wildcard-empty-${index}`}
                    className="facility-slot facility-slot--empty"
                    onMouseEnter={() => actions.promptTarget('wildcard', '')}
                    onMouseLeave={actions.clearPrompt}
                    onClick={() => setWildcardOpen(true)}
                  >
                    <image className="facility-slot__icon" src={iconFor('wildcard')} />
                  </button>
                ))}
              </view>
            </scroll>
          </view>

          <view className="planet-panel__section planet-panel__inventory">
            <view className="planet-panel__section-title">
              <text>Inventory</text>
            </view>
            <scroll className="planet-panel__scroll">
              <view className="inventory-grid">
                {planet.resources.map((resource) => (
                  <button
                    key={resource.id}
                    className="inventory-item"
                    onMouseEnter={() => actions.promptTarget(resource.advanced ? 'advancedresource' : 'resource', resource.id)}
                    onMouseLeave={actions.clearPrompt}
                  >
                    <image className="inventory-item__icon" src={iconFor(resource.icon)} />
                    <text className="inventory-item__count">{String(resource.amount)}</text>
                  </button>
                ))}
              </view>
            </scroll>
          </view>

          <view className="planet-panel__section planet-panel__shipping">
            <view className="planet-panel__section-title">
              <text>Shipping &amp; Consumption</text>
            </view>
            <button
              className="planet-panel__action"
              onClick={() => {
                actions.shippingSetMode('menu');
                setShippingOpen(true);
              }}
            >
              <text>Shipping</text>
            </button>
            <button className="planet-panel__action" onClick={() => setPrioritiesOpen(true)}>
              <text>Consumption Priorities</text>
            </button>
          </view>

          {buildResource && (
            <FacilityBuildMenu
              title={`Build - ${buildResource}`}
              options={slotOptions}
              onClose={() => setBuildResource(null)}
            />
          )}

          {wildcardOpen && (
            <FacilityBuildMenu
              title="Build Transformation"
              options={wildcardOptions}
              onClose={() => setWildcardOpen(false)}
            />
          )}

          {prioritiesOpen && <PriorityMenu planet={planet} onClose={() => setPrioritiesOpen(false)} />}

          {shippingOpen && <ShippingMenu planet={planet} onClose={() => setShippingOpen(false)} />}
        </>
      )}
    </view>
  );
}
