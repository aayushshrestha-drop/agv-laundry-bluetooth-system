import { Component, OnInit } from '@angular/core';
import { LogoComponent } from './logo/logo.component';
import { ReplaceableComponentsService, RestService } from '@abp/ng.core';
import { eThemeBasicComponents } from '@abp/ng.theme.basic';
import { RoutesComponent } from './routes/routes.component';
import * as signalR from "@aspnet/signalr";
import { OAuthService } from 'angular-oauth2-oidc';
import { Toaster, ToasterService } from '@abp/ng.theme.shared';
import { environment } from "../environments/environment"
@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
  `,
})
export class AppComponent implements OnInit {
  private hubConnection: signalR.HubConnection;
  get hasLoggedIn(): boolean {
    return this.oAuthService.hasValidAccessToken();
  }
  get accessToken(): string {
    return this.oAuthService.getAccessToken();
  }
  get toasterOptions(): Partial<Toaster.ToastOptions> {
    return {
      life: -1,
      sticky: true,
      closable: true,
      tapToDismiss: false
    }
  };
  alarm: any = null;
  constructor(private replaceableComponents: ReplaceableComponentsService,
    private oAuthService: OAuthService,
    private toaster: ToasterService,
    private restService: RestService) {
    this.alarm = new Audio();
    this.alarm.src = `${environment.oAuthConfig.redirectUri}/assets/audio/alarm.wav`;
    this.alarm.load();
  }
  ngOnInit() {
    console.log(environment.apis.default.url)
    this.replaceableComponents.add({
      component: LogoComponent,
      key: eThemeBasicComponents.Logo,
    });
    this.replaceableComponents.add({
      component: RoutesComponent,
      key: eThemeBasicComponents.Routes,
    });
    if (this.hasLoggedIn) {
      //this.initSignalR();
    }

  }

  initSignalR() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apis.default.url}/signalr-hubs/chat`, {
        accessTokenFactory: () => this.accessToken
      })
      .build();
    this.hubConnection.on("ReceiveMessage", (message) => {
      this.toaster.info(message, 'New Message', this.toasterOptions);
      this.playAlarm();
    });
    this.hubConnection
      .start()
      .then(() => {
        console.log('Connection started')
      })
      .catch(err => console.log('Error while starting connection: ' + err))
  }
  playAlarm() {
    this.alarm.play();
  }
  stopAlarm() {
    this.alarm.pause();
    this.alarm.currentTime = 0;
  }
}
