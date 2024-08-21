import { NgModule } from "@angular/core";
import { SystemAlertPage } from "./system-alert.page";
import { SharedModule } from "@app/shared.module";
import { RouterModule } from "@angular/router";
import { PermissionGuard } from "@app/providers/guard/permission.guard";
import { Permission } from "@app/helpers/permissions.helper";
import { BackNavigationGuard } from "@app/providers/guard/back-navigation.guard";
import { TypedRoutes } from "@app/routing/typed-route";

const routes: TypedRoutes = [
    {
        path: 'create',
        component: SystemAlertPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageProducts, Permission.ManageTenants],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':systemAlertId/edit',
        component: SystemAlertPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageProducts, Permission.ManageTenants],
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export system alert module class.
 * This class manage Ng module declaration of system alert.
 */
@NgModule({
    declarations: [
        SystemAlertPage,
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routes),
    ],
})
export class SystemAlertModule { }
