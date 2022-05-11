import { mapEnumToOptions } from '@abp/ng.core';

export enum TagLocationLogStatus {
  IN = 1,
  OUT = 2,
}

export const tagLocationLogStatusOptions = mapEnumToOptions(TagLocationLogStatus);
