import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: 'translate-dto',
  standalone: true
})
export class TranslateDtoPipe implements PipeTransform {
  transform(dto: any, property: string): any {
    if (!dto)
      return null;

    var cultureName = "en-US";

    var translation = dto?.translations.find((_: any) => _.cultureName == cultureName)[0];

    if (!translation)
      return null;

    return translation[property];
  }
}

