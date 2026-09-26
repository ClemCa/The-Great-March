import { actions } from '../bridge/actions';
import { iconFor } from '../assets/icons';
import { cn } from '../lib/cn';
import type { PlanetSnapshot, ResourceEntry } from '../bridge/types';

/**
 * Full React port of the `ShippingMenu` / `ShippingSubMenu` / `ShipChoice` / `CargoChoice` flow.
 * The step machine lives in `ReactGameBridge` (shipping mode + selections); this component is a
 * thin renderer that dispatches the same actions the original uGUI handlers did.
 */
export function ShippingMenu({ planet, onClose }: { planet: PlanetSnapshot; onClose: () => void }) {
  const shipping = planet.shipping;
  if (!shipping) return null;

  const mode = shipping.mode || 'ships';
  const returnToMenu = () => actions.shippingSetMode('menu');

  return (
    <view className="hud-submenu hud-submenu--shipping">
      <view className="hud-submenu__title">
        <text>{titleFor(mode, planet)}</text>
      </view>

      <view className="shipping-body">
        {mode === 'menu' && (
          <view className="shipping-menu">
            <button
              className={cn('shipping-action', (!shipping.hasPlayer || shipping.leaderInTransit) && 'shipping-action--off')}
              onClick={
                shipping.hasPlayer && !shipping.leaderInTransit ? () => actions.shippingPlayerMove() : undefined
              }
            >
              <text>Player</text>
            </button>
            <button className="shipping-action" onClick={() => actions.shippingSetMode('people')}>
              <text>People</text>
            </button>
            <button className="shipping-action" onClick={() => actions.shippingSetMode('resources')}>
              <text>Resources</text>
            </button>
            <button className="shipping-action" onClick={() => actions.shippingSetMode('ships')}>
              <text>Ships</text>
            </button>
          </view>
        )}

        {mode === 'ships' && <ShipList planet={planet} />}

        {mode === 'people' && (
          <view className="shipping-column">
            <text className="shipping-note">{`${shipping.peopleAmount} of ${Math.min(planet.people, 5)} people`}</text>
            <view className="shipping-slider">
              <button className="settings-btn" onClick={() => actions.shippingSetPeopleAmount(Math.max(0, shipping.peopleAmount - 1))}>
                <text>-</text>
              </button>
              <text className="settings-value">{shipping.peopleAmount}</text>
              <button className="settings-btn" onClick={() => actions.shippingSetPeopleAmount(Math.min(Math.min(planet.people, 5), shipping.peopleAmount + 1))}>
                <text>+</text>
              </button>
            </view>
            <button
              className={cn('shipping-action', shipping.peopleAmount <= 0 && 'shipping-action--off')}
              onClick={shipping.peopleAmount > 0 ? () => actions.shippingLaunchPeople() : undefined}
            >
              <text>Send</text>
            </button>
          </view>
        )}

        {mode === 'resources' && <ResourcePicker planet={planet} />}

        {mode === 'cargo' && <CargoLoader planet={planet} />}

        {mode === 'president' && (
          <view className="shipping-column">
            <text className="shipping-note">Move the president with this ship.</text>
            <button className="shipping-action" onClick={() => actions.shippingLaunch()}>
              <text>Launch</text>
            </button>
          </view>
        )}
      </view>

      <button
        className="hud-submenu__return"
        onClick={mode === 'ships' || mode === 'menu' ? onClose : returnToMenu}
      >
        <text>Return</text>
      </button>
    </view>
  );
}

function ShipList({ planet }: { planet: PlanetSnapshot }) {
  const shipping = planet.shipping!;
  return (
    <view className="shipping-column">
      {shipping.ships.length === 0 && <text className="shipping-note">No ship here.</text>}
      <scroll className="shipping-ships">
        {shipping.ships.map((ship) => (
          <button
            key={ship.index}
            className={cn('shipping-ship', shipping.selectedShip === ship.index && 'shipping-ship--selected')}
            onMouseEnter={() => actions.promptTarget('ship', String(ship.index))}
            onMouseLeave={actions.clearPrompt}
            onClick={() => actions.shippingSelectShip(ship.index)}
          >
            <image className="shipping-ship__icon" src={iconFor(ship.icon)} />
            <text className="shipping-ship__label">{`Ship ${ship.index + 1}: ${ship.type}`}</text>
            <text className="shipping-ship__fuel">{`Fuel ${ship.fuel}/${ship.requiredFuel}`}</text>
          </button>
        ))}
      </scroll>

      {shipping.selectedShip >= 0 && (
        <button
          className={cn('shipping-action', !canAct(planet) && 'shipping-action--off')}
          onClick={canAct(planet) ? () => actions.shippingRefuel() : undefined}
        >
          <text>{actionLabel(planet)}</text>
        </button>
      )}
    </view>
  );
}

function ResourcePicker({ planet }: { planet: PlanetSnapshot }) {
  const shipping = planet.shipping!;
  const selected = findResource(planet, shipping.resourceId, shipping.resourceAdvanced);
  const max = Math.min(selected?.amount ?? 0, 10);

  return (
    <view className="shipping-column">
      <scroll className="shipping-resources">
        <view className="hud-submenu__grid">
          {planet.resources
            .filter((resource) => resource.amount > 0)
            .map((resource) => (
              <button
                key={resource.id}
                className={cn(
                  'hud-submenu__cell',
                  shipping.resourceId === resource.id && shipping.resourceAdvanced === resource.advanced && 'shipping-cell--selected',
                )}
                onMouseEnter={() => actions.promptTarget(resource.advanced ? 'advancedresource' : 'resource', resource.id)}
                onMouseLeave={actions.clearPrompt}
                onClick={() => actions.shippingSelectResource(resource.id, resource.advanced)}
              >
                <image className="hud-submenu__icon" src={iconFor(resource.icon)} />
              </button>
            ))}
        </view>
      </scroll>

      <view className="shipping-slider">
        <button className="settings-btn" onClick={() => actions.shippingSetResourceAmount(Math.max(0, shipping.resourceAmount - 1))}>
          <text>-</text>
        </button>
        <text className="settings-value">{shipping.resourceAmount}</text>
        <button className="settings-btn" onClick={() => actions.shippingSetResourceAmount(Math.min(max, shipping.resourceAmount + 1))}>
          <text>+</text>
        </button>
      </view>

      <button
        className={cn('shipping-action', shipping.resourceAmount <= 0 && 'shipping-action--off')}
        onClick={shipping.resourceAmount > 0 ? () => actions.shippingLaunchResource() : undefined}
      >
        <text>Send</text>
      </button>
    </view>
  );
}

function CargoLoader({ planet }: { planet: PlanetSnapshot }) {
  const shipping = planet.shipping!;
  const selected = findResource(planet, shipping.resourceId, shipping.resourceAdvanced);
  const max = Math.min(selected?.amount ?? 0, 10);

  return (
    <view className="shipping-column">
      <scroll className="shipping-resources">
        <view className="hud-submenu__grid">
          {planet.resources
            .filter((resource) => resource.amount > 0)
            .map((resource) => (
              <button
                key={resource.id}
                className={cn(
                  'hud-submenu__cell',
                  shipping.resourceId === resource.id && shipping.resourceAdvanced === resource.advanced && 'shipping-cell--selected',
                )}
                onMouseEnter={() => actions.promptTarget(resource.advanced ? 'advancedresource' : 'resource', resource.id)}
                onMouseLeave={actions.clearPrompt}
                onClick={() => actions.shippingSelectResource(resource.id, resource.advanced)}
              >
                <image className="hud-submenu__icon" src={iconFor(resource.icon)} />
              </button>
            ))}
        </view>
      </scroll>

      <view className="shipping-slider">
        <text className="shipping-slider__label">Cargo</text>
        <button className="settings-btn" onClick={() => actions.shippingSetResourceAmount(Math.max(0, shipping.resourceAmount - 1))}>
          <text>-</text>
        </button>
        <text className="settings-value">{`${shipping.resourceAmount}/${max}`}</text>
        <button className="settings-btn" onClick={() => actions.shippingSetResourceAmount(Math.min(max, shipping.resourceAmount + 1))}>
          <text>+</text>
        </button>
      </view>

      <view className="shipping-slider">
        <text className="shipping-slider__label">People</text>
        <button className="settings-btn" onClick={() => actions.shippingSetPeopleAmount(Math.max(0, shipping.peopleAmount - 1))}>
          <text>-</text>
        </button>
        <text className="settings-value">{`${shipping.peopleAmount}/${shipping.maxPeople}`}</text>
        <button className="settings-btn" onClick={() => actions.shippingSetPeopleAmount(Math.min(shipping.maxPeople, shipping.peopleAmount + 1))}>
          <text>+</text>
        </button>
      </view>

      <button
        className={cn('shipping-action', shipping.peopleAmount <= 0 && 'shipping-action--off')}
        onClick={shipping.peopleAmount > 0 ? () => actions.shippingLaunch() : undefined}
      >
        <text>Launch</text>
      </button>
    </view>
  );
}

function titleFor(mode: string, planet: PlanetSnapshot) {
  switch (mode) {
    case 'people':
      return 'Send People';
    case 'resources':
      return 'Send Resources';
    case 'cargo':
      return 'Load Cargo';
    case 'president':
      return 'Presidential Ship';
    default:
      return `Ships (${planet.ships})`;
  }
}

function findResource(planet: PlanetSnapshot, id: string, advanced: boolean): ResourceEntry | undefined {
  return planet.resources.find((resource) => resource.id === id && resource.advanced === advanced);
}

function selectedShip(planet: PlanetSnapshot) {
  const shipping = planet.shipping!;
  return shipping.ships.find((ship) => ship.index === shipping.selectedShip);
}

function canAct(planet: PlanetSnapshot) {
  const ship = selectedShip(planet);
  return !!ship && (ship.canLaunch || ship.canRefuel);
}

function actionLabel(planet: PlanetSnapshot) {
  const ship = selectedShip(planet);
  if (!ship) return 'Select a ship';
  if (ship.canLaunch) return ship.type === 'Presidential' ? 'Launch' : 'Load';
  if (ship.canRefuel) return `Refuel (${ship.refuelAmount})`;
  return 'Missing fuel';
}
