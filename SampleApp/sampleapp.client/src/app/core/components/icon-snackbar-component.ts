import { Component, Inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
// ...

@Component({
  selector: 'icon-snackBar',
  template: `
  <div fxLayout="row" fxLayoutAlign="start center"  fxLayoutGap="5px">
    @if(data?.icon != null) {
      <mat-icon>{{data?.icon}}</mat-icon>
    }
    <div fxFlex>{{data?.message}}</div>

    <button mat-icon-button (click)="snackBarRef.dismiss()">
      <mat-icon>close</mat-icon>
    </button>
  </div>
`
})
export class IconSnackBarComponent {
  constructor(
    public snackBarRef: MatSnackBarRef<IconSnackBarComponent>,
    @Inject(MAT_SNACK_BAR_DATA) public data: any) {
  }
}
