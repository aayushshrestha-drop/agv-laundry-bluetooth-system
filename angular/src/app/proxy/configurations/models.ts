import type { AuditedEntityDto } from '@abp/ng.core';

export interface ConfigurationDto extends AuditedEntityDto<string> {
  key?: string;
  value?: string;
}

export interface CreateUpdateConfigurationDto {
  key: string;
  value: string;
}
