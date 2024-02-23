import { Component, OnInit, inject } from "@angular/core";
import { ActivatedRoute, ActivatedRouteSnapshot, ResolveFn, Router } from "@angular/router";
import { PersonService } from "./service";
import { PersonDto } from "./dtos";
import { BehaviorSubject, Subject, debounceTime, map, switchMap, tap } from "rxjs";


export const personEditResolver: ResolveFn<PersonDto> =
  (route: ActivatedRouteSnapshot) => {
    var service = inject(PersonService);
    var router = inject(Router);

    var id = route.paramMap.get('id');

    if (id === null) {
      return service.quickCreate()
        .pipe(map(id => {
          router.navigate(["/app/person/edit/" + id]);
          return id;
        }));
    }

    return service.getSingleById(id)
  };

@Component({
  selector: 'person-edit',
  templateUrl: './edit.component.html'
})
export class PersonEditComponent implements OnInit {

  personId: string;

  model$ = new BehaviorSubject<PersonDto>(null);
  modelChanged$ = new Subject<void>();

  quickUpdating$ = new BehaviorSubject<boolean>(false);
  loading$ = new BehaviorSubject<boolean>(false);

  constructor(
    private router: Router,
    protected activatedRoute: ActivatedRoute,

    private personService: PersonService) {
  }

  ngOnInit() {

    this.quickUpdating$
      //.pipe(mergeMap(_ => iif(() => _ == true,
      //  this.quickUpdating$.pipe(delay(150)),
      //  this.quickUpdating$)))
      .subscribe(_ => {
        this.loading$.next(_);
      });


    this.personId = this.activatedRoute.snapshot.params.id;

    this.activatedRoute.data
      .subscribe(
        (data) => {
          if (data.model != null) {
            this.model$.next(data.model);
          }
        });

    this.modelChanged$.pipe(
      debounceTime(400),
      //distinctUntilChanged(),
      tap(_ => { this.quickUpdating$.next(true); }),
      switchMap(_ => this.personService.quickUpdate(this.model$.value)),
    )
      .subscribe(_ => {
        this.model$.next(_);
        this.quickUpdating$.next(false);
      });
  }

  onModelChange(event: any): void {
    this.modelChanged$.next();
  }
}
