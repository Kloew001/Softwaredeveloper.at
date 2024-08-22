import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, distinctUntilChanged, filter, first, map, of, switchMap, take, tap, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviourSubjectCacheResolverService } from '../cache/behaviourSubjectCacheResolver.service';
import { ApplicationUserDto } from './dto';

export class LoginInfo {
  accessToken?: string;
  refreshToken?: string;
}


@Injectable({ providedIn: 'root' })
export class AuthenticationService {

  public loginInfo$ = new BehaviorSubject<LoginInfo>(null);

  public curentUser$ = new BehaviorSubject<ApplicationUserDto>(null);


  get isAuthorized$(): Observable<boolean> {
    return this.loginInfo$.pipe(map(loginInfo => loginInfo ? true : false));
  }

  get accessToken(): string {
    return this.loginInfo$.value?.accessToken;
  }

  get accessToken$(): Observable<string> {
    return this.loginInfo$.pipe(map(loginInfo => loginInfo?.accessToken));
  }

  constructor(
    private http: HttpClient,
    private router: Router,

    protected cacheResolverService: BehaviourSubjectCacheResolverService) {

    this.getLoginInfoFromStorage();

    window.addEventListener('storage', (event) => {
      if (event.key == null ||
        event.key === 'loginInfo') {
        this.getLoginInfoFromStorage();
      }
    });

    setTimeout(() => {
      this.isAuthorized$.pipe(
        distinctUntilChanged(),
        switchMap(isAuthorized => {
          if (isAuthorized) {
            return this.onAuthorized();
          }
          else {
            return this.onDeauthorized();
          }
        })
      )
        .subscribe(_ => {
        });
    });

    this.curentUser$.subscribe(user => {
    });
  }

  onAuthorized(): Observable<any> {

    return this.getCurrentUser();
  }

  onDeauthorized(): Observable<any> {
    return of(true);
  }

  getCurrentUser() {
    return this.http.get<ApplicationUserDto>('api/applicationUser/getCurrentUser')
      .pipe(
        catchError(error => {
          this.curentUser$.next(null);
          return of(null);
        }),
        tap(curentUser => {
          this.curentUser$.next(curentUser);
        }));
  }

  setPreferedCulture(cultureName: string) {
    this.http.post<void>('api/applicationUser/setPreferedCulture?cultureName=' + cultureName, {})
      .subscribe(_ => {
        var next = this.curentUser$.value;
        next.preferedCultureName = cultureName;
        this.curentUser$.next(next);
      });
  }

  isInRole$(...roleIds: string[]): Observable<boolean> {
    return this.curentUser$.pipe(map(user => user?.roles?.find(_ => roleIds.includes(_.id)) != null));
  }

  isInRoleByName$(...roleNames: string[]): Observable<boolean> {

    return this.curentUser$.pipe(map(user => user?.roles?.find(_ =>
      roleNames.includes(_.name)) != null));
  }

  attemptLogin(): Observable<LoginInfo> {
    return this.isAuthorized$.pipe(
      first(),
      switchMap(isAuthorized => {
        if (isAuthorized) {
          return of(this.loginInfo$.value);
        } else {
          return this.login();
        }
      }));
  }

  login(): Observable<LoginInfo> {
    return this.http.post<LoginInfo>('api/account/authenticate', {})
      .pipe(tap(loginInfo => {
        return this.handleAuthenticate(loginInfo);
      }));
  }

  logout(): void {

    this.revokeToken().subscribe({
      complete: () => {
        this.clearLocal();
      },
      error: () => {
        this.clearLocal();
      }
    });

    //this.router.navigate(['/auth/login'], { queryParamsHandling: 'preserve' });
  }

  clearLocal() {
    localStorage.removeItem('loginInfo');
    this.cacheResolverService.clearAll();
    this.loginInfo$.next(null);
  }

  refreshToken(): Observable<LoginInfo> {


    if (!this.loginInfo$.value) {
      debugger;
      return throwError(() => false);
    }

    return this.http.post<LoginInfo>('api/account/refreshToken', this.loginInfo$.value)
      .pipe(tap(loginInfo => {
        debugger;
        return this.handleAuthenticate(loginInfo);
      }),
        catchError((error: HttpErrorResponse) => {
          debugger;
          this.clearLocal();
          return throwError(() => error);
        })
      );
  }

  revokeToken(): Observable<any> {
    return this.http.post<any>('/api/account/revokeToken', {}).pipe(
      tap(() => this.clearLocal()),
      catchError((error: HttpErrorResponse) => {
        this.clearLocal();
        return throwError(() => error);
      })
    );
  }

  handleAuthenticate(loginInfo) {
    if (loginInfo && loginInfo.accessToken) {
      localStorage.setItem('loginInfo', JSON.stringify(loginInfo));
      this.loginInfo$.next(loginInfo);
    } else {
      this.loginInfo$.next(null);
    }
    return loginInfo;
  }

  getLoginInfoFromStorage() {

    var localStorageLoginInfo = localStorage.getItem('loginInfo');

    if (!localStorageLoginInfo) {
      this.loginInfo$.next(null);
    }
    else {
      try {
        var loginInfo = <LoginInfo>JSON.parse(localStorageLoginInfo);

        if (this.loginInfo$.value != loginInfo) {
          this.loginInfo$.next(loginInfo);
        }
      }
      catch (e) {
        console.error(e);
        this.loginInfo$.next(null);
      }
    }
  }
}
