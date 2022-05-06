import { Component } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-logo',
  template: 
  `<a class="navbar-brand" routerLink="/">
    <img [src]="logoUrl"  />
  </a>`,
})
export class LogoComponent { 
  logoUrl: string = `${environment.oAuthConfig.redirectUri}/assets/images/logo.png`
}

