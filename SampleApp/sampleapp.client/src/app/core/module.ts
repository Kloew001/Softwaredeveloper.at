import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { BehaviourSubjectCacheResolverService } from './cache/behaviourSubjectCacheResolver.service';
import { CacheResolverService } from './cache/cacheResolver.service';
import { EnumMetadataToArrayPipe, EnumToArrayPipe } from './pipes/enumToArrayPipe';

import { MaterialModule } from './material.module';
import { NgLetModule } from 'ng-let';
import { FlexLayoutModule } from '@angular/flex-layout';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS } from '@angular/material/form-field';
import { AsyncButtonComponent } from './components/async-button-component';
import { ErrorInterceptor } from './interceptor/error.interceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { IconSnackBarComponent } from './components/icon-snackbar-component';
import { JwtInterceptor } from './interceptor/jwt.interceptor';


@NgModule({
  declarations: [
    EnumToArrayPipe,
    EnumMetadataToArrayPipe,
    AsyncButtonComponent,
    IconSnackBarComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,

    NgLetModule,

    FlexLayoutModule,
    MaterialModule
  ],
  exports: [
    CommonModule,
    RouterModule,
    FormsModule,

    NgLetModule,

    EnumToArrayPipe,
    EnumMetadataToArrayPipe,
    AsyncButtonComponent,
    IconSnackBarComponent,

    FlexLayoutModule,
    MaterialModule
  ],
  providers: [

    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },

    CacheResolverService,
    BehaviourSubjectCacheResolverService,

    {
      provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
      useValue: {
        appearance: 'outline',
       /* floatLabel: 'always',*/
        hideRequiredMarker: false
      }
    }
  ]
})
export class CoreModule { }
