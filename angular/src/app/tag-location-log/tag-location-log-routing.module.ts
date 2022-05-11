import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { TagLocationLogComponent } from './tag-location-log.component';

const routes: Routes = [{ path: '', component: TagLocationLogComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TagLocationLogRoutingModule { }
