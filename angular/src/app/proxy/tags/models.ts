import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateTagDto {
  systemName: string;
  displayName: string;
  status: boolean;
  tagId?: string;
  cartNo?: string;
}

export interface TagDto extends AuditedEntityDto<string> {
  systemName?: string;
  displayName?: string;
  status: boolean;
  tagId?: string;
  cartNo?: string;
}
