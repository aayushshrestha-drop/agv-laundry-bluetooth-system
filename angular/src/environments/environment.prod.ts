import { Environment } from '@abp/ng.core';

const baseUrl = 'https://drop-location-engine.server247.info';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'AGV Laundry App',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://drop-location-engine.server247.info',
    redirectUri: 'https://drop-location-engine.server247.info/client',
    clientId: 'AGVLaundry_App',
    responseType: 'code',
    scope: 'openid profile AGVLaundry email address phone role',
    postLogoutRedirectUri: 'https://drop-location-engine.server247.info/client'
  },
  apis: {
    default: {
      url: '',
      rootNamespace: 'AGV.Laundry',
    },
  },
} as Environment;
