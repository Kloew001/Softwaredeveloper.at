import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit, AfterViewInit, Inject, ViewChild } from '@angular/core';
import { DateAdapter, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatIconRegistry } from '@angular/material/icon';
import { MatSidenav } from '@angular/material/sidenav';
import { DomSanitizer } from '@angular/platform-browser';
import { NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject, Observable, delay, filter, map, of } from 'rxjs';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {

  useSidebar = false;
  isCollapsed = false;

  @ViewChild(MatSidenav)
  sidenav!: MatSidenav;

  get appTitle(): Observable<string> {
    return of('Sample APP');
  }

  constructor(
    private observer: BreakpointObserver,
    private router: Router,
    private domSanitizer: DomSanitizer,
    private matIconRegistry: MatIconRegistry) {

    //this.matIconRegistry.addSvgIcon('app-logo',
    //  this.domSanitizer.bypassSecurityTrustResourceUrl('assets/logo.png'));
  }

  ngAfterViewInit() {
    this.observer
      .observe(['(max-width: 800px)'])
      .pipe(delay(1))
      .subscribe((res) => {
        if ((<any>res).matches) {
          this.sidenav.mode = 'over';
          this.sidenav.close();
        } else {
          this.sidenav.mode = 'side';
          this.sidenav.open();
        }
      });

    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd)
      )
      .subscribe(() => {
        if (this.sidenav.mode === 'over') {
          this.sidenav.close();
        }
      });
  }

  get layoutFooterText(): Observable<string> {
    var year = new Date().getFullYear().toString();

    return of('©{{CurrentYear}} Softwaredeveloper.at')
      .pipe(map(_ => _.replace('{{CurrentYear}}', year)));
  }
}

