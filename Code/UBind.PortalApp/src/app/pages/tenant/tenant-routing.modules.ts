import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers';
import { ListTenantPage } from './tenant.index';
import { ListNoSelectionPage } from '@pages/list-no-selection/list-no-selection.page';
import { CreateEditTenantPage } from './create-edit-tenant/create-edit-tenant.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { DetailTenantPage } from './detail-tenant/detail-tenant.page';
import { TenantSessionSettingsPage } from './session-settings/session-settings.page';
import { CreateEditProductPage } from '@pages/product/create-edit-product/create-edit-product.page';
import { ListTenantProductPage } from './list-tenant-product/list-tenant-product.page';
import { DetailProductPage } from '@pages/product/detail-product/detail-product.page';
import { ProductDeploymentSettingsPage } from '@pages/product/deployment-setting/deployment-setting.page';
import { ProductQuoteExpirySettingsPage } from '@pages/product/quote-expiry-setting/quote-expiry-setting.page';
import { EditNumberPoolPage } from '@pages/product/edit-number-pool/edit-number-pool.page';
import { FilterSortPage } from '../filter-sort/filter-sort-page';
import { TransactionTypePage } from '@pages/product/transaction-type/renewal-transaction-type.page';
import { RefundSettingsPage } from '@pages/product/refund-settings/refund-settings.page';
import { TenantPasswordExpirySettingsPage } from './password-expiry-settings/password-expiry-settings.page';
import { ListDataTableDefinitionPage } from '../data-table/list-data-table-definition/list-data-table-definition.page';
import { ReleaseModule } from '../release/release.module';
import { SystemAlertModule } from '../system-alert/system-alert.module';
import { EditEmailTemplateModule } from '../edit-email-template/edit-email-template.module';
import { AdditionalPropertyDefinitionModule }
    from '../additional-property-definition/additional-property-definition.module';
import { PortalModule } from '../portal/portal.module';
import { DataTableModule } from '../data-table/data-table.module';
import { TypedRoutes } from '@app/routing/typed-route';
import { OrganisationModule } from '../organisation/organisation.module';
import { ReleaseSelectionSettingsPage } from '../product/release-selection-setting/release-selection-settings.page';
import { SelectProductRelease } from '../release/product-release/select-product-release.page';

const routes: TypedRoutes = [
    {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
    },
    {
        path: 'list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a tenant to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateEditTenantPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias',
        component: DetailTenantPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/edit',
        component: CreateEditTenantPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias/session-settings',
        component: TenantSessionSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias/password-expiry-settings',
        component: TenantPasswordExpirySettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ":tenantAlias/organisation",
        loadChildren: () => OrganisationModule,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
        },
    },
    {
        path: ":tenantAlias/portal",
        loadChildren: () => PortalModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPortals],
        },
    },
    {
        path: ':tenantAlias/system-alert',
        loadChildren: () => SystemAlertModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/email-template',
        loadChildren: () => EditEmailTemplateModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/product/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a product to view details',
        },
    },
    {
        path: ':tenantAlias/product/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: ':tenantAlias/product/list',
        },
    },
    {
        path: ':tenantAlias/product/create',
        component: CreateEditProductPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias/product/:productAlias/refund-settings',
        component: RefundSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/product/:productAlias',
        component: DetailProductPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/product/:productAlias/edit',
        component: CreateEditProductPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias/product/:productAlias/deployment-settings/:environment/edit',
        component: ProductDeploymentSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias/product/:productAlias/transaction-type/:transactionType/edit',
        component: TransactionTypePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias/product/:productAlias/quote-expiry-settings',
        component: ProductQuoteExpirySettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ":tenantAlias/product/:productAlias/number-pool/:numberPoolId/edit",
        component: EditNumberPoolPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/product/:productAlias/email-template',
        loadChildren: () => EditEmailTemplateModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ":tenantAlias/product/:productAlias/release",
        loadChildren: () => ReleaseModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewReleases],
        },
    },
    {
        path: ':tenantAlias/product/:productAlias/system-alert',
        loadChildren: () => SystemAlertModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/product/:contextId/additional-property-definition',
        loadChildren: () => AdditionalPropertyDefinitionModule,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewAdditionalPropertyValues, Permission.ViewTenants],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/product/:productAlias/data-table',
        loadChildren: () => DataTableModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts, Permission.ViewDataTables],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':productAlias/data-table/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewDataTables],
            masterComponent: ListDataTableDefinitionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':tenantAlias/additional-property-definition',
        loadChildren: () => AdditionalPropertyDefinitionModule,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewAdditionalPropertyValues, Permission.ViewTenants],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/data-table',
        loadChildren: () => DataTableModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewTenants, Permission.ViewDataTables],
            masterComponent: ListTenantPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':tenantAlias/product/:productAlias/release-selection-settings',
        component: ReleaseSelectionSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':tenantAlias/product/:productAlias/select-product-release/:environment',
        component: SelectProductRelease,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageReleases],
            masterComponent: ListTenantProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Tenant routing module.
 */
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class TenantRoutingModule { }
