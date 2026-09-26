export interface Credit {
  name: string;
  description: string;
  url: string;
}

// Mirrors the `CreditSpawner` data on the MainMenu scene's Credits / Developpers panels.
export const CREDITS: Credit[] = [
  { name: 'Secret Labs', description: 'Yarn Spinner', url: 'https://yarnspinner.dev/' },
  {
    name: 'Unluck Software',
    description: 'Animated sun',
    url: 'https://assetstore.unity.com/packages/3d/environments/sci-fi/sun-2990',
  },
  {
    name: 'One Potato Kingdom',
    description: 'Stylized planet pack',
    url: 'https://assetstore.unity.com/packages/3d/environments/stylized-planet-pack-full-148233',
  },
  { name: 'Zintoki', description: 'Space Breaker', url: 'https://zintoki.itch.io/space-breaker' },
  {
    name: 'cakeslice',
    description: 'Outline effect',
    url: 'https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/outline-effect-78608',
  },
  { name: 'CO.AG Music', description: 'Music', url: 'https://www.youtube.com/watch?v=GpGhwDrzDZ4' },
  {
    name: 'Kenney',
    description: 'Most Sounds & Icons from Kenney Aame Assets 3',
    url: 'https://kenney.itch.io/kenney-game-assets-3',
  },
  {
    name: 'Cadson Demak',
    description: 'Font - Chakra Petch',
    url: 'https://fonts.google.com/specimen/Chakra+Petch',
  },
  {
    name: 'SoftPolyStudios',
    description: 'Low Poly Crown',
    url: 'https://softpolystudios.itch.io/low-poly-crown-free',
  },
];

export const TEAM: Credit[] = [
  {
    name: 'ClemCa',
    description: 'Main Developer, Game Design & More',
    url: 'https://clementcatorc.com',
  },
  { name: 'Bluenix', description: 'Gamejam Helper, the Assets Guy', url: 'https://github.com/Bluenix2' },
  {
    name: 'Lack of sleep',
    description: "Some of the worst gamejam code I've ever made",
    url: 'https://clementcatorc.com',
  },
];
