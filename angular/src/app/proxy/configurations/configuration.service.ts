import type { ConfigurationDto, CreateUpdateConfigurationDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ConfigurationService {
  apiName = 'Default';

  create = (input: CreateUpdateConfigurationDto) =>
    this.restService.request<any, ConfigurationDto>({
      method: 'POST',
      url: '/api/app/configuration',
      body: input,
    },
    { apiName: this.apiName });

  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/configuration/${id}`,
    },
    { apiName: this.apiName });

  get = (id: string) =>
    this.restService.request<any, ConfigurationDto>({
      method: 'GET',
      url: `/api/app/configuration/${id}`,
    },
    { apiName: this.apiName });

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<ConfigurationDto>>({
      method: 'GET',
      url: '/api/app/configuration',
      params: { skipCount: input.skipCount, maxResultCount: input.maxResultCount, sorting: input.sorting },
    },
    { apiName: this.apiName });

  update = (id: string, input: CreateUpdateConfigurationDto) =>
    this.restService.request<any, ConfigurationDto>({
      method: 'PUT',
      url: `/api/app/configuration/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
