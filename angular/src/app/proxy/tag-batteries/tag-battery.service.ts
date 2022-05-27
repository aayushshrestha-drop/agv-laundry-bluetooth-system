import type { TagBatteryDto, TagBatteryRequestDto } from './models';
import { RestService } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TagBatteryService {
  apiName = 'Default';

  tagBatteryByModel = (model: TagBatteryRequestDto) =>
    this.restService.request<any, TagBatteryDto>({
      method: 'POST',
      url: '/api/app/tag-battery/tag-battery',
      body: model,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
