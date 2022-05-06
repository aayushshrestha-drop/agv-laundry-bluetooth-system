import type { BaseStationDto, CreateUpdateBaseStationDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BaseStationService {
  apiName = 'Default';

  create = (input: CreateUpdateBaseStationDto) =>
    this.restService.request<any, BaseStationDto>({
      method: 'POST',
      url: '/api/app/base-station',
      body: input,
    },
    { apiName: this.apiName });

  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/base-station/${id}`,
    },
    { apiName: this.apiName });

  get = (id: string) =>
    this.restService.request<any, BaseStationDto>({
      method: 'GET',
      url: `/api/app/base-station/${id}`,
    },
    { apiName: this.apiName });

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<BaseStationDto>>({
      method: 'GET',
      url: '/api/app/base-station',
      params: { skipCount: input.skipCount, maxResultCount: input.maxResultCount, sorting: input.sorting },
    },
    { apiName: this.apiName });

  update = (id: string, input: CreateUpdateBaseStationDto) =>
    this.restService.request<any, BaseStationDto>({
      method: 'PUT',
      url: `/api/app/base-station/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
