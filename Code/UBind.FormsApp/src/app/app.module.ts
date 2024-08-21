import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { sharedConfig } from './app.module.shared';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgSelectModule } from '@ng-select/ng-select';
import { JwtModule, JWT_OPTIONS } from '@auth0/angular-jwt';
import { ApplicationService } from './services/application.service';
import { BearerTokenService } from './services/bearer-token.service';

// eslint-disable-next-line prefer-arrow/prefer-arrow-functions
export function jwtOptionsFactory(applicationService: ApplicationService): any {
    const getToken: () => string = (): string => {
        let bearerTokenService: BearerTokenService = new BearerTokenService(applicationService);
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
}

/**
 * The main app module for this application.
 */
@NgModule({
    bootstrap: sharedConfig.bootstrap,
    declarations: sharedConfig.declarations,
    imports: [
        BrowserModule,
        FormsModule,
        NgSelectModule,
        BrowserAnimationsModule,
        JwtModule.forRoot({
            jwtOptionsProvider: {
                provide: JWT_OPTIONS,
                useFactory: jwtOptionsFactory,
                deps: [ApplicationService],
            },
        }),
        ...sharedConfig.imports,
    ],
    providers: [
        { provide: 'ORIGIN_URL', useValue: location.origin },
        ...sharedConfig.providers,
    ],
})
export class AppModule {
}
