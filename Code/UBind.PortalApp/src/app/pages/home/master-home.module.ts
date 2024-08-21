import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { MasterHomePage } from './master-home/master-home.page';
import { ListModule } from "@app/list.module";
import { AuthenticationGuard } from '@app/providers/guard/authentication.guard';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        component: MasterHomePage,
        canActivate: [AuthenticationGuard],
    },
];

/**
 * Export tenant home module class.
 * TODO: Write a better class header: Ng Module declarations of
 * Tenant home.
 */
@NgModule({
    declarations: [MasterHomePage],
    imports: [
        SharedModule,
        ListModule,
        RouterModule.forChild(routes),
    ],
})
export class MasterHomeModule { }
