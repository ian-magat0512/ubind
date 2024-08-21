import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared.module';
import { RouterModule } from '@angular/router';
import { PermissionGuard } from '../../providers/guard/permission.guard';
import { Permission } from '../../helpers/permissions.helper';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import {
    AdditionalPropertyDefinitionListPage,
} from './additional-property-definition-list/additional-property-definition-list.page';
import {
    AdditionalPropertyDefinitionDetailsPage,
} from './additional-property-definition-detail/additional-property-definition-detail.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import {
    CreateEditAdditionalPropertyDefinitionPage,
} from './create-edit-additional-property-definition/create-edit-additional-property-definition.page';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: ':entityType',
        component: AdditionalPropertyDefinitionListPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewAdditionalPropertyValues, Permission.ViewTenants],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':entityType/create',
        component: CreateEditAdditionalPropertyDefinitionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.EditAdditionalPropertyValues, Permission.ManageTenants],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':entityType/:additionalPropertyId',
        component: AdditionalPropertyDefinitionDetailsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewAdditionalPropertyValues, Permission.ViewTenants],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':entityType/:additionalPropertyId/:mode',
        component: CreateEditAdditionalPropertyDefinitionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.EditAdditionalPropertyValues, Permission.ManageTenants],
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Module for additional property page.
 */
@NgModule({
    declarations: [
        AdditionalPropertyDefinitionListPage,
        AdditionalPropertyDefinitionDetailsPage,
        CreateEditAdditionalPropertyDefinitionPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class AdditionalPropertyDefinitionModule { }
