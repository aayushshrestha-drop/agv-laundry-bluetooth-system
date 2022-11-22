import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'AGV Laundry App',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'http://localhost',
    redirectUri: 'http://localhost/agv',
    clientId: 'AGVLaundry_App',
    responseType: 'code',
    scope: 'openid profile AGVLaundry email address phone role',
    postLogoutRedirectUri: 'http://localhost/agv'
  },
  apis: {
    default: {
      url: '',
      rootNamespace: 'AGV.Laundry',
    },
  },
} as Environment;
