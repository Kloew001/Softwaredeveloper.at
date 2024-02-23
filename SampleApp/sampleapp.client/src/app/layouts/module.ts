import { NgModule } from '@angular/core';
import { LayoutComponent } from './layout.component';
import { CoreModule } from '../core/module';

@NgModule({
  declarations: [LayoutComponent],
  imports: [
    CoreModule,
  ],
  providers: [
  ]
})
export class LayoutsModule { }
