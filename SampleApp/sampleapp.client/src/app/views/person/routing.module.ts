import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { PersonEditComponent, personEditResolver } from './edit.component';

const routes: Routes = [
  { path: "", redirectTo: "create", pathMatch: "full" },
  {
    path: 'create',
    component: PersonEditComponent,
    resolve: { model: personEditResolver }
  },
  {
    path: 'edit/:id',
    component: PersonEditComponent,
    resolve: { model: personEditResolver }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PersonRoutingModule { }
