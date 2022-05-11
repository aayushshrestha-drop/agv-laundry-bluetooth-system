import type { TagLocationLogDto } from './models';
import { RestService } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { TagDto } from '../tags/models';

@Injectable({
  providedIn: 'root',
})
export class TagLocationLogService {
  apiName = 'Default';

  getTagLocationLogsByTagId = (tagId: string) =>
    this.restService.request<any, TagLocationLogDto[]>({
      method: 'GET',
      url: `/api/app/tag-location-log/tag-location-logs/${tagId}`,
    },
    { apiName: this.apiName });

  getTags = () =>
    this.restService.request<any, TagDto[]>({
      method: 'GET',
      url: '/api/app/tag-location-log/tags',
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
