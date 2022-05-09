import { Environment } from '@abp/ng.core';

const baseUrl = 'https://jen.drop.com';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'AGV Laundry App',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://jen.drop.com',
    redirectUri: 'https://jen.drop.com/agv',
    clientId: 'AGVLaundry_App',
    responseType: 'code',
    scope: 'openid profile AGVLaundry email address phone role',
    postLogoutRedirectUri: 'https://jen.drop.com/agv'
  },
  apis: {
    default: {
      url: '',
      rootNamespace: 'AGV.Laundry',
    },
  },
} as Environment;
