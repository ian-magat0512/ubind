import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { LogOutPage } from './logout/logout.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        component: LogOutPage,
        canDeactivate: [BackNavigationGuard],
        data: {
            menuDisabled: true,
        },
    },
];
/**
 * Export logout module class.
 * This class manage Ng Module declaration of Logout.
 */
@NgModule({
    declarations: [
        LogOutPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class LogoutModule { }
