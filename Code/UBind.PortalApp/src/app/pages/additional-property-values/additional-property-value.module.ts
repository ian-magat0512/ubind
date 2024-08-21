import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { Permission } from '@app/helpers';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { SharedModule } from '@app/shared.module';
import { AdditionalPropertyValueEdit } from './edit-additional-property-values/edit-additional-property-values.page';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: ':entityType',
        component: AdditionalPropertyValueEdit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewAdditionalPropertyValues],
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Module for additional property value page.
 */
@NgModule({
    declarations: [
        AdditionalPropertyValueEdit,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class AdditionalPropertyValueModule { }
