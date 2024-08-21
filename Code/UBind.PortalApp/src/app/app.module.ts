import { NgModule, APP_INITIALIZER, COMPILER_OPTIONS, CompilerFactory } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouteReuseStrategy } from '@angular/router';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { JwtModule, JWT_OPTIONS } from '@auth0/angular-jwt';
import { ErrorHandler } from '@angular/core';
import { SplashScreen } from '@ionic-native/splash-screen/ngx';
import { StatusBar } from '@ionic-native/status-bar/ngx';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { AppConfigService } from '@app/services/app-config.service';
import {
    PopoverEnvironmentSelectionComponent,
} from './components/popover-environment-selection/popover-environment-selection.component';
import { SharedModule } from './shared.module';
import { GlobalErrorHandler } from './providers/global-error-handler';
import { BackdropComponent } from './components/backdrop/backdrop.component';
import { SharedComponentsModule } from './components/shared-components.module';
import { MainMenuComponent } from './components/main-menu/main-menu.component';
import { BearerTokenService } from '@app/services/bearer-token.service';
import { AppStartupService } from './services/app-startup.service';
import { BlobErrorHttpInterceptor } from './interceptors/blob-error-http.interceptor';
import { PathRedirectGuard } from './providers/guard/path-redirect.guard';
import { PortalRedirectGuard } from './providers/guard/portal-redirect.guard';
import { IonicModule, IonicRouteStrategy } from '@ionic/angular';
import { FormsAppClaimPage } from './pages/forms-app/forms-app-claim.page';
import { FormsAppQuotePage } from './pages/forms-app/forms-app-quote.page';
import { JitCompilerFactory } from '@angular/platform-browser-dynamic';
import { serviceProvidersToken } from './injection-tokens/services-injection-tokens';

export const jwtOptionsFactory = (appConfigService: AppConfigService): any => {
    const getToken = (): string => {
        let bearerTokenService: BearerTokenService = new BearerTokenService(appConfigService);
        return bearerTokenService.getToken();
    };
    let apiHost: string = location.host;
    const whiteListedDomainsArray: Array<string> = [
        apiHost,
        'ubind-application-qa.aptiture.com',
        'ubind-application-uat.aptiture.com',
        'app.ubind.com.au',
        'app.ubind.io',
        'bs-local.com',
    ];
    return {
        tokenGetter: getToken,
        whitelistedDomains: whiteListedDomainsArray,
    };
};

// @dynamic
export const appStartupFactory = (appStartupService: AppStartupService): any => {
    const result = (): Promise<boolean> => appStartupService.load();
    return result;
};

/**
 * Defines what components are loaded for the main app module, which are therefore always loaded (ie not lazy).
 */
@NgModule({
    declarations: [
        AppComponent,
        MainMenuComponent,
        PopoverEnvironmentSelectionComponent,
        BackdropComponent,
        FormsAppClaimPage,
        FormsAppQuotePage,
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        HttpClientModule,
        JwtModule.forRoot({
            jwtOptionsProvider: {
                provide: JWT_OPTIONS,
                useFactory: jwtOptionsFactory,
                deps: [AppConfigService],
            },
        }),
        IonicModule.forRoot({ mode: 'md', scrollAssist: false }),
        AppRoutingModule,
        SharedModule,
        SharedComponentsModule,
    ],
    providers: [
        StatusBar,
        SplashScreen,
        { provide: RouteReuseStrategy, useClass: IonicRouteStrategy },
        {
            provide: APP_INITIALIZER,
            useFactory: appStartupFactory,
            deps: [AppStartupService],
            multi: true,
        },
        { provide: ErrorHandler, useClass: GlobalErrorHandler },
        // { provide: RouteReuseStrategy, useClass: CustomRouteReuseStrategy }
        { provide: HTTP_INTERCEPTORS, useClass: BlobErrorHttpInterceptor, multi: true },
        {
            provide: COMPILER_OPTIONS,
            useValue: {},
            multi: true,
        },
        {
            provide: CompilerFactory,
            useClass: JitCompilerFactory,
            deps: [COMPILER_OPTIONS],
        },
        ...serviceProvidersToken,
        PathRedirectGuard,
        PortalRedirectGuard,
    ],
    bootstrap: [AppComponent],

})
export class AppModule { }
