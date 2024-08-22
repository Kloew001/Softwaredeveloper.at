
import { Injectable } from '@angular/core';

import { BehaviorSubject, Observable, catchError, filter, from, skip, switchMap, take, throwError } from 'rxjs';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { AuthenticationService, LoginInfo } from '../authentication/authentication-service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  constructor(private authService: AuthenticationService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const token = this.authService.accessToken;

    if (token) {
      request = this.addToken(request, token);
    }

    return next.handle(request).pipe(
      catchError((error) => {

        if (error instanceof HttpErrorResponse &&
          error.status === 401 &&
          request.url.toLowerCase().indexOf('api/account/') == -1) {
          return this.handle401Error(request, next);
        } else {
          return throwError(() => error);
        }
      })
    );
  }

  private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    debugger;

    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.authService.refreshToken().pipe(
        switchMap((token: any) => {
          debugger;
          this.refreshTokenSubject.next(token.accessToken);
          this.isRefreshing = false;
          var nextRequest = this.addToken(request, token.accessToken);

          return next.handle(nextRequest);
        }),
        catchError((error) => {
          debugger;

          this.authService.logout();

          this.isRefreshing = false;

          return throwError(() => error);
        })
      );
    } else {
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(jwt => {
          return next.handle(this.addToken(request, jwt!));
        })
      );
    }
  }
}

