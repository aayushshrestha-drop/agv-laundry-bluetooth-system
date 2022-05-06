import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { BaseStationComponent } from './basestation.component';

const routes: Routes = [{ path: '', component: BaseStationComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BaseStationRoutingModule { }
