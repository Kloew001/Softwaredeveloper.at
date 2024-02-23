import { ChangeDetectionStrategy, Component, HostBinding, Input } from '@angular/core';

//[mat-flat-button]="buttonStyle == 'flat'"
//[mat-raised-button]="buttonStyle == 'raised'"
//[mat-stroked-button]="buttonStyle == 'stroked'"

@Component({
  selector: 'async-button',
  template: `<button mat-raised-button

                    [color]="color"

                    [class.loading]="isloading"
                    [disabled]="isDisabled">

                    <mat-icon>{{icon}}</mat-icon>

                    {{text}}
                    <ng-content [class.loading]="isloading">
                    </ng-content>

                    <mat-spinner [mode]="'indeterminate'"
                                 [diameter]="19"
                                 *ngIf="isloading"
                                 [class.loading]="isloading"></mat-spinner>
               </button>`,

  styles: [`  :host 
                {
                    position: relative;
                }

                :host.disabled
                {
                    pointer-events: none;
                }

                mat-spinner 
                { 
                    margin: auto;
                    position: absolute;
                    top: 0;
                    right: 0;
                    bottom: 0;
                    left: 0;

                    opacity: 0;

                    transition: opacity .3s ease-in-out;
                }

                mat-spinner.loading 
                {
                    opacity: 1;
                }

                .content 
                {
                    opacity: 1;
                    transition: opacity .3s ease-in-out;
                }

                .content.loading 
                {
                    opacity: 0;
                }

                button ::ng-deep .mat-button-wrapper
                {
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
             `
  ],

  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AsyncButtonComponent {

  constructor() { }

  @Input("isloading")
  isloading = false;

  @Input("disabled")
  disabled = false;

  @Input("color")
  color: string = 'primary';

  @Input('buttonStyle')
  buttonStyle: 'flat' | 'raised' | 'stroked' = 'flat';

  @Input("icon")
  icon = null;

  @Input("text")
  text = null;

  @HostBinding('class.disabled')
  get isDisabled() {
    return this.isloading || this.disabled;
  }
}
