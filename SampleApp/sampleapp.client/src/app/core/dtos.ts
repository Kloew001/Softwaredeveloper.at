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

export class PageFilter {
  page: number;
  pageSize: number;
}

export class PageResult<TItem> {
  page: number;
  pageSize: number;
  totalCount: number;
  pageItems: TItem[];
}
