import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { LayoutsModule } from './layouts/module';
import { CoreModule } from './core/module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@NgModule({ declarations: [
        AppComponent
    ],
    bootstrap: [AppComponent], imports: [BrowserModule,
        BrowserAnimationsModule,
        AppRoutingModule,
        CoreModule,
        LayoutsModule], providers: [provideHttpClient(withInterceptorsFromDi())] })
export class AppModule { }
