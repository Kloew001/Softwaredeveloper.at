import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, map } from "rxjs";
import { plainToInstance } from "class-transformer";
import { PersonDto } from "./dtos";

@Injectable()
export class PersonService {

  constructor(protected http: HttpClient) {
  }

  getSingleById(id: string | null): Observable<PersonDto> {
    return this.http.get<PersonDto>('/api/person/getSingleById?id=' + id)
      .pipe(
        (map((data) => {
          var result = plainToInstance(PersonDto, data);
          console.log(result);
          return result;
        })));
  }

  quickCreate(): Observable<PersonDto> {
    return this.http.post<PersonDto>('/api/person/quickCreate', {})
      .pipe(
        (map((data) => {
          var result = plainToInstance(PersonDto, data);
          return result;
        })));
  }

  quickUpdate(dto: PersonDto): Observable<PersonDto> {
    return this.http.post<PersonDto>('/api/person/update', dto)
      .pipe(
        (map((data) => {
          var result = plainToInstance(PersonDto, data);
          return result;
        })));
  }

  validate(dto: PersonDto): Observable<void> {
    return this.http.post<void>('/api/person/validate', dto)
      .pipe(
        (map((data) => {
          return data;
        })));
  }
}
