export class Dto {
  id: string;
}
export enum YesNoType {
  yes,
  no,
}

export const YesNoTypeMetadata: Record<YesNoType, any> = {
  [YesNoType.yes]: { value: true, displayName: "Ja" },
  [YesNoType.no]: { value: false, displayName: "Nein" }
};
