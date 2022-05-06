import { Component, HostBinding, OnInit } from '@angular/core';

@Component({
  selector: 'app-routes',
  templateUrl: './routes.component.html',
  styleUrls: ['./routes.component.scss']
})
export class RoutesComponent {
  @HostBinding('class.mx-auto')
  marginAuto = true;

  get smallScreen() {
    return window.innerWidth < 992;
  }
}
