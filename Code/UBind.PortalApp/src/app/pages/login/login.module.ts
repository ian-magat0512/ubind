import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { LoginPage } from './login/login.page';
import { RequestPasswordPage } from './request-password/request-password.page';
import { ResetPasswordPage } from './reset-password/reset-password.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { NotfoundComponent } from './notfound/notfound.component';
import { AuthenticationGuard } from '../../providers/guard/authentication.guard';
import { PasswordExpiredPage } from './password-expired/password-expired.page';
import { TypedRoutes } from '@app/routing/typed-route';
import { PageRouteIndentifier } from '@app/helpers/page-route-identifier.helper';

const routes: TypedRoutes = [
    {
        path: 'not-found',
        component: NotfoundComponent,
    },
    {
        path: '',
        component: LoginPage,
        canActivate: [AuthenticationGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            menuDisabled: true,
        },
    },
    {
        path: 'request-password',
        component: RequestPasswordPage,
        data: {
            menuDisabled: true,
            routeIdentifier: PageRouteIndentifier.RequestPasswordResetPage,
        },
    },
    {
        path: 'reset-password/:userId',
        component: ResetPasswordPage,
        data: {
            menuDisabled: true,
            routeIdentifier: PageRouteIndentifier.ResetPasswordPage,
        },
    },
    {
        path: 'password-expired',
        component: PasswordExpiredPage,
        canDeactivate: [BackNavigationGuard],
        data: {
            menuDisabled: true,
            routeIdentifier: PageRouteIndentifier.PasswordExpiredPage,
        },
    },
];

/**
 * Export login module class.
 * TODO: Write a better class header: Ng Module declaration of login.
 */
@NgModule({
    declarations: [
        LoginPage,
        RequestPasswordPage,
        ResetPasswordPage,
        PasswordExpiredPage,
        NotfoundComponent,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class LoginModule { }
