import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'AGV Laundry App',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44361',
    redirectUri: baseUrl,
    clientId: 'AGVLaundry_App',
    responseType: 'code',
    scope: 'offline_access openid profile role email phone AGVLaundry',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://localhost:44361',
      rootNamespace: 'AGV.Laundry',
    },
  },
} as Environment;
