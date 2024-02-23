import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layouts/layout.component';

const routes: Routes = [

  /*  { path: "", redirectTo: "auth/login", pathMatch: "full" },*/
  { path: "", redirectTo: "app", pathMatch: "full" },
  //{
  //  path: 'auth',
  //  loadChildren: () => import('./account/account.module').then(m => m.AccountModule)
  //},
  {
    path: 'app',
    component: LayoutComponent,
    children: [
      {
        path: "", redirectTo: "person", pathMatch: "full"
      },
      {
        path: 'person',
        data: { preload: true },
        loadChildren: () => import('./views/person/module')
          .then(m => m.PersonModule),
      }
    ]
  }

  //{
  //  path: '404',
  //  component: Page404Component
  //},
  //{
  //  path: '500',
  //  component: Page500Component
  //},
  //{
  //  path: '**',
  //  component: Page404Component
  //},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
