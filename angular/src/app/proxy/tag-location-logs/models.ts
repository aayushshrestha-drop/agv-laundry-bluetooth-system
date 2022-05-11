import type { TagLocationLogStatus } from '../enums/tag-location-log-status.enum';

export interface TagLocationLogDto {
  basestationId?: string;
  basestationIp?: string;
  hotel?: string;
  lot?: string;
  tagId?: string;
  tagMac?: string;
  cart?: string;
  status: TagLocationLogStatus;
  statusString?: string;
  creationTime?: string;
}
