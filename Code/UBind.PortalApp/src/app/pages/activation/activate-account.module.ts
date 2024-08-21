import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ActivateAccountPage } from './activate-account/activate-account.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: ':userId',
        component: ActivateAccountPage,
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export activate account module class
 */
@NgModule({
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
    declarations: [ActivateAccountPage],
})
export class ActivateAccountModule {}
