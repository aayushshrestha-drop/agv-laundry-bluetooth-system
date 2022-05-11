import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TagLocationLogRoutingModule } from './tag-location-log-routing.module';
import { TagLocationLogComponent } from './tag-location-log.component';
import { SharedModule } from '../shared/shared.module';


@NgModule({
  declarations: [TagLocationLogComponent],
  imports: [
    CommonModule,
    TagLocationLogRoutingModule,
    SharedModule
  ]
})
export class TagLocationLogModule { }
