import type { AuditedEntityDto } from '@abp/ng.core';

export interface BaseStationDto extends AuditedEntityDto<string> {
  systemName?: string;
  displayName?: string;
  status: boolean;
  bsip?: string;
  hotel?: string;
  lotNo?: string;
}

export interface CreateUpdateBaseStationDto {
  systemName: string;
  displayName: string;
  status: boolean;
  bsip: string;
  hotel: string;
  lotNo: string;
}
