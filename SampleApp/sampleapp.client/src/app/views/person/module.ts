import { NgModule } from '@angular/core';
import { PersonService } from './service';
import { CoreModule } from '../../core/module';
import { PersonEditComponent } from './edit.component';
import { PersonRoutingModule } from './routing.module';

@NgModule({
  declarations: [
    PersonEditComponent,
  ],
  imports: [
    CoreModule,

    PersonRoutingModule,
  ],
  exports: [],
  providers: [
    PersonService,
  ]
})
export class PersonModule { }
