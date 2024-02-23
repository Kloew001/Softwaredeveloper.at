import { Component, Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { IconSnackBarComponent } from '../components/icon-snackbar-component';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private snackBar: MatSnackBar) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(catchError(err => {
      console.error(err);

      //if (err.status === 401 && this.authenticationService.isAuthorized) {
      //  // auto logout if 401 response returned from api
      //  this.authenticationService.logout();
      //  location.reload();
      //}
      //else {
      //}

      var message = err.error.message || err.error.title || err.error.detail || err.statusText;

      const config = new MatSnackBarConfig();
      config.duration = 4000;
      config.panelClass = ['custom-snackbar', 'snackbar-error'];
      config.horizontalPosition = 'right';
      config.verticalPosition = 'top';
      config.data = {
        icon: 'error',
        message: message
      };

      this.snackBar.openFromComponent(IconSnackBarComponent, config);

      return throwError(err);
    }))
  }
}
