export type LegStyle = 'triangle' | 'needle' | 'paddle' | 'blade';
export type CharacterDefinition = {
  id: string;
  name: string;
  tagline: string;
  cost: number;
  color: string;
  eyeColor: string;
  faceColor: string;
  shellStroke: string;
  shape: 'circle' | 'roundedSquare' | 'hexagon' | 'capsule' | 'diamond';
  eyeStyle: 'pill' | 'round' | 'visor' | 'mono';
  legCount: number;
  legStyle: LegStyle;
};
/** Cosmetics share one outer contact radius, jump speed and collection radius. */
export const CHARACTERS: CharacterDefinition[] = [
  {
    id: 'mektra',
    name: 'Mektra',
    tagline: 'The original explorer',
    cost: 0,
    color: '#FFFFFF',
    eyeColor: '#2585FF',
    faceColor: '#152236',
    shellStroke: '#8DA0B8',
    shape: 'circle',
    eyeStyle: 'pill',
    legCount: 15,
    legStyle: 'triangle',
  },
  {
    id: 'tide',
    name: 'Tide',
    tagline: 'A softer kind of motion',
    cost: 25,
    color: '#7EDBCD',
    eyeColor: '#165CDF',
    faceColor: '#16343D',
    shellStroke: '#3B9E94',
    shape: 'capsule',
    eyeStyle: 'round',
    legCount: 12,
    legStyle: 'paddle',
  },
  {
    id: 'ember',
    name: 'Ember',
    tagline: 'A little spark of courage',
    cost: 75,
    color: '#FFAF80',
    eyeColor: '#FFD9A8',
    faceColor: '#3E2730',
    shellStroke: '#B96346',
    shape: 'roundedSquare',
    eyeStyle: 'visor',
    legCount: 15,
    legStyle: 'blade',
  },
  {
    id: 'volt',
    name: 'Volt',
    tagline: 'Bright ideas. Sharp steps.',
    cost: 125,
    color: '#F5D668',
    eyeColor: '#6EEBFF',
    faceColor: '#283342',
    shellStroke: '#B18C39',
    shape: 'hexagon',
    eyeStyle: 'mono',
    legCount: 18,
    legStyle: 'needle',
  },
  {
    id: 'nova',
    name: 'Nova',
    tagline: 'Made for distant orbits',
    cost: 180,
    color: '#C8B4F3',
    eyeColor: '#E1C7FF',
    faceColor: '#302B4D',
    shellStroke: '#8570B5',
    shape: 'roundedSquare',
    eyeStyle: 'round',
    legCount: 12,
    legStyle: 'triangle',
  },
  {
    id: 'frost',
    name: 'Frost',
    tagline: 'Cool, calm, collected',
    cost: 250,
    color: '#A8DCFA',
    eyeColor: '#2585FF',
    faceColor: '#173447',
    shellStroke: '#609CBF',
    shape: 'hexagon',
    eyeStyle: 'visor',
    legCount: 20,
    legStyle: 'paddle',
  },
  {
    id: 'bloom',
    name: 'Bloom',
    tagline: 'A bright little daydream',
    cost: 350,
    color: '#F0BAD2',
    eyeColor: '#FF94D5',
    faceColor: '#392A41',
    shellStroke: '#B9789A',
    shape: 'circle',
    eyeStyle: 'pill',
    legCount: 24,
    legStyle: 'needle',
  },
  {
    id: 'onyx',
    name: 'Onyx',
    tagline: 'Quietly unstoppable',
    cost: 500,
    color: '#404A60',
    eyeColor: '#78F5DA',
    faceColor: '#111E30',
    shellStroke: '#7B89A3',
    shape: 'diamond',
    eyeStyle: 'mono',
    legCount: 16,
    legStyle: 'blade',
  },
];
export const DEFAULT_CHARACTER = CHARACTERS[0];
export function getCharacter(id: string) {
  return CHARACTERS.find(c => c.id === id) ?? DEFAULT_CHARACTER;
}
