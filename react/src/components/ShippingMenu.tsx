import { useEffect, useRef, useState } from 'react';
import { actions } from '../bridge/actions';
import { iconFor } from '../assets/icons';
import { cn } from '../lib/cn';
import { HudMark } from './HudMark';
import { Panel } from './Panel';
import type { PlanetSnapshot, ShipEntry } from '../bridge/types';

type Stage = 'ships' | 'load';

const CATEGORIES = [
  { key: 'Cargo', label: 'Cargo', mark: 'block' },
  { key: 'People', label: 'People', mark: 'people' },
  { key: 'Presidential', label: 'Presidential', mark: 'star' },
] as const;

function categoryOf(type: string): (typeof CATEGORIES)[number]['key'] {
  if (type === 'Cargo') return 'Cargo';
  if (type === 'Presidential') return 'Presidential';
  return 'People';
}

function markForType(type: string) {
  if (type === 'Presidential') return 'star';
  if (type === 'Cargo') return 'block';
  return 'people';
}

function titleForType(type: string) {
  if (type === 'Presidential') return 'Presidential Ship';
  if (type === 'Cargo') return 'Load Cargo';
  return 'Send People';
}

/**
 * Ship-first shipping menu. The player picks a ship (grouped by what it carries), refuels it
 * inline or on the next screen, then loads cargo and sends — a single flow instead of the old
 * Player/People/Resources/Ships wizard. Refuelling is always its own action, and a quick-refuel
 * button is repeated on the load screen so fuel never blocks a send silently.
 */
export function ShippingMenu({ planet, onClose }: { planet: PlanetSnapshot; onClose: () => void }) {
  const [stage, setStage] = useState<Stage>('ships');
  const shipping = planet.shipping;
  const selected = shipping?.ships.find((ship) => ship.index === shipping.selectedShip);

  useEffect(() => {
    setStage('ships');
  }, [planet.name]);

  useEffect(() => {
    if (stage === 'load' && !selected) setStage('ships');
  }, [stage, selected]);

  useEffect(() => {
    if (!shipping) return;
    if (shipping.peopleAmount > shipping.maxPeople) actions.shippingSetPeopleAmount(shipping.maxPeople);
    else if (shipping.resourceAmount > shipping.maxResource) actions.shippingSetResourceAmount(shipping.maxResource);
  }, [shipping?.peopleAmount, shipping?.maxPeople, shipping?.resourceAmount, shipping?.maxResource]);

  if (!shipping) return null;

  const groups = CATEGORIES.map((category) => ({
    ...category,
    ships: shipping.ships.filter((ship) => categoryOf(ship.type) === category.key),
  })).filter((group) => group.ships.length > 0);

  const openShip = (ship: ShipEntry) => {
    actions.shippingSelectShip(ship.index);
    setStage('load');
  };

  return (
    <Panel
      className="hud-submenu hud-submenu--shipping"
      edge="left"
      surface="hud"
      title={
        stage === 'load'
          ? titleForType(shipping.shipType || selected?.type || '')
          : `Shipping (${shipping.ships.length})`
      }
      icon={stage === 'load' ? markForType(shipping.shipType || selected?.type || '') : 'arrow-r'}
      headerSize="sm"
      onClose={onClose}
      closePlacement="flush"
    >
      <view className="shipping-body">
        {stage === 'ships' ? (
          <scroll className="ship-groups">
            {groups.length === 0 && <text className="shipping-note">No ships on this planet.</text>}
            {groups.map((group) => (
              <view key={group.key} className="ship-group">
                <view className="ship-group__title">
                  <HudMark icon={group.mark} className="hud-mark--sm" />
                  <text>{group.label}</text>
                  <text className="ship-group__count">{group.ships.length}</text>
                </view>
                {group.ships.map((ship) => (
                  <ShipRow key={ship.index} ship={ship} onOpen={openShip} />
                ))}
              </view>
            ))}
          </scroll>
        ) : (
          selected && <LoadStage planet={planet} ship={selected} onBack={() => setStage('ships')} />
        )}
      </view>
    </Panel>
  );
}

function ShipRow({ ship, onOpen }: { ship: ShipEntry; onOpen: (ship: ShipEntry) => void }) {
  const pct = ship.requiredFuel > 0 ? Math.min(100, Math.round((ship.fuel / ship.requiredFuel) * 100)) : 100;
  const refuel = () => {
    actions.shippingSelectShip(ship.index);
    actions.shippingRefuel();
  };

  return (
    <view className="ship-row">
      <view className="ship-row__top">
        <image className="ship-row__icon" src={iconFor(ship.icon)} />
        <view className="ship-row__info">
          <ShipName ship={ship} className="ship-row__name" />
          <text className="ship-row__type">{ship.type}</text>
        </view>
        <view className="ship-row__fuel">
          <text className={cn('ship-row__fuel-text', ship.canLaunch ? 'ship-row__fuel-text--ok' : 'ship-row__fuel-text--low')}>
            {`Fuel ${ship.fuel}/${ship.requiredFuel}`}
          </text>
          <view className="ship-row__fuel-bar">
            <view
              className={cn('ship-row__fuel-fill', ship.canLaunch ? 'ship-row__fuel-fill--ok' : 'ship-row__fuel-fill--low')}
              style={{ width: `${pct}%` }}
            />
          </view>
        </view>
      </view>

      <view className="ship-row__actions">
        {!ship.canLaunch && (
          <button
            className={cn('ship-btn ship-btn--fuel', !ship.canRefuel && 'ship-btn--off')}
            onClick={ship.canRefuel ? refuel : undefined}
          >
            <text>{`Refuel ${ship.refuelAmount}`}</text>
          </button>
        )}
        <button className="ship-btn ship-btn--primary" onClick={() => onOpen(ship)}>
          <text>{ship.type === 'Presidential' ? 'Send' : 'Load'}</text>
        </button>
      </view>
    </view>
  );
}

/**
 * The ship's name: plain text until clicked, then an inline text field. The name lives on the
 * Unity ship (`ShippingRenameShip` -> `Registry.Ship.Name`) so it survives saves and transit.
 */
function ShipName({ ship, className }: { ship: ShipEntry; className?: string }) {
  const [editing, setEditing] = useState(false);
  const inputRef = useRef<any>(null);
  const fallback = `Ship ${ship.index + 1}`;

  useEffect(() => {
    if (!editing) return;
    const field: any = inputRef.current;
    if (!field) return;
    if (typeof field.focus === 'function') {
      field.focus();
      field.select?.();
      return;
    }
    const inner = field.InputField;
    inner?.ActivateInputField?.();
    if (inner) {
      inner.selectionAnchorPosition = 0;
      inner.selectionFocusPosition = (inner.text ?? '').length;
    }
  }, [editing]);

  if (editing) {
    return (
      <input
        ref={inputRef}
        className={cn(className, 'ship-name--edit')}
        value={ship.name || fallback}
        characterLimit={24}
        onEndEdit={(text: string) => {
          actions.shippingRenameShip(ship.index, text);
          setEditing(false);
        }}
      />
    );
  }

  return (
    <text className={cn(className, 'ship-name')} onClick={() => setEditing(true)}>
      {ship.name || fallback}
    </text>
  );
}

function LoadStage({ planet, ship, onBack }: { planet: PlanetSnapshot; ship: ShipEntry; onBack: () => void }) {
  const shipping = planet.shipping!;
  const type = shipping.shipType || ship.type;
  const live = shipping.ships.find((entry) => entry.index === shipping.selectedShip) ?? ship;
  const presidential = type === 'Presidential';
  const cargo = type === 'Cargo' || type === 'Passenger';
  const resources = planet.resources.filter((resource) => resource.amount > 0);
  const canSend = live.canLaunch && (presidential ? shipping.hasPlayer : shipping.peopleAmount > 0);

  const send = () => {
    if (presidential) actions.shippingLaunchPresident();
    else actions.shippingLaunch();
    onBack();
  };

  return (
    <view className="ship-load">
      <view className="ship-load__head">
        <image className="ship-load__icon" src={iconFor(live.icon)} />
        <view className="ship-load__titles">
          <ShipName ship={live} className="ship-load__title" />
          <text className="ship-load__type">{type}</text>
          <text className={cn('ship-load__fuel', live.canLaunch ? 'ship-load__fuel--ok' : 'ship-load__fuel--low')}>
            {`Fuel ${live.fuel}/${live.requiredFuel}`}
          </text>
        </view>
        {!live.canLaunch && (
          <button
            className={cn('ship-btn ship-btn--fuel', !live.canRefuel && 'ship-btn--off')}
            onClick={live.canRefuel ? actions.shippingRefuel : undefined}
          >
            <text>{`Refuel ${live.refuelAmount}`}</text>
          </button>
        )}
      </view>

      {presidential && <text className="shipping-note">Move the president with this ship.</text>}

      {cargo && (
        <view className="ship-section">
          <text className="ship-section__label">Resources</text>
          <scroll className="ship-load__resources">
            <view className="hud-submenu__grid">
              {resources.map((resource) => (
                <button
                  key={`${resource.id}-${resource.advanced}`}
                  className={cn(
                    'ship-cell',
                    shipping.resourceId === resource.id &&
                      shipping.resourceAdvanced === resource.advanced &&
                      'ship-cell--selected',
                  )}
                  onMouseEnter={() => actions.promptTarget(resource.advanced ? 'advancedresource' : 'resource', resource.id)}
                  onMouseLeave={actions.clearPrompt}
                  onClick={() => actions.shippingSelectResource(resource.id, resource.advanced)}
                >
                  <image className="ship-cell__icon" src={iconFor(resource.icon)} />
                </button>
              ))}
            </view>
          </scroll>
          <Stepper
            label="Amount"
            value={shipping.resourceAmount}
            max={shipping.maxResource}
            onChange={actions.shippingSetResourceAmount}
          />
        </view>
      )}

      {!presidential && (
        <Stepper
          label="People"
          value={shipping.peopleAmount}
          max={shipping.maxPeople}
          onChange={actions.shippingSetPeopleAmount}
        />
      )}

      <view className="ship-load__footer">
        <button className="ship-btn" onClick={onBack}>
          <text>Back</text>
        </button>
        <button className={cn('ship-btn ship-btn--primary', !canSend && 'ship-btn--off')} onClick={canSend ? send : undefined}>
          <text>{presidential ? 'Send President' : 'Send'}</text>
        </button>
      </view>
    </view>
  );
}

function Stepper({
  label,
  value,
  max,
  onChange,
}: {
  label: string;
  value: number;
  max: number;
  onChange: (value: number) => void;
}) {
  return (
    <view className="ship-stepper">
      <text className="ship-stepper__label">{label}</text>
      <button
        className={cn('ship-stepper__btn', value <= 0 && 'ship-stepper__btn--off')}
        onClick={value > 0 ? () => onChange(value - 1) : undefined}
      >
        <text>-</text>
      </button>
      <text className="ship-stepper__value">{value}</text>
      <button
        className={cn('ship-stepper__btn', value >= max && 'ship-stepper__btn--off')}
        onClick={value < max ? () => onChange(value + 1) : undefined}
      >
        <text>+</text>
      </button>
      <text className="ship-stepper__max">{`/ ${max}`}</text>
    </view>
  );
}
