import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers';
import { ListProductPage } from './list-product/list-product.page';
import { ListNoSelectionPage } from '@pages/list-no-selection/list-no-selection.page';
import { CreateEditProductPage } from './create-edit-product/create-edit-product.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { DetailProductPage } from './detail-product/detail-product.page';
import { ProductQuoteExpirySettingsPage } from './quote-expiry-setting/quote-expiry-setting.page';
import { RefundSettingsPage } from './refund-settings/refund-settings.page';
import { ProductDeploymentSettingsPage } from './deployment-setting/deployment-setting.page';
import { EditNumberPoolPage } from './edit-number-pool/edit-number-pool.page';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';
import { TransactionTypePage } from './transaction-type/renewal-transaction-type.page';
import { ListDataTableDefinitionPage } from '../data-table/list-data-table-definition/list-data-table-definition.page';
import { AdditionalPropertyDefinitionModule }
    from '../additional-property-definition/additional-property-definition.module';
import { SystemAlertModule } from '../system-alert/system-alert.module';
import { ReleaseModule } from '../release/release.module';
import { EditEmailTemplateModule } from '../edit-email-template/edit-email-template.module';
import { DataTableModule } from '../data-table/data-table.module';
import { TypedRoutes } from '@app/routing/typed-route';
import { ReleaseSelectionSettingsPage } from './release-selection-setting/release-selection-settings.page';
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
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a product to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateEditProductPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':productAlias',
        component: DetailProductPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':productAlias/edit',
        component: CreateEditProductPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':productAlias/quote-expiry-settings',
        component: ProductQuoteExpirySettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':productAlias/refund-settings',
        component: RefundSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':productAlias/deployment-settings/:environment/edit',
        component: ProductDeploymentSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':productAlias/transaction-type/:transactionType/edit',
        component: TransactionTypePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ":productAlias/number-pool/:numberPoolId/edit",
        component: EditNumberPoolPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':productAlias/email-template',
        loadChildren: () => EditEmailTemplateModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ":productAlias/release",
        loadChildren: () => ReleaseModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewReleases],
        },
    },
    {
        path: ':productAlias/system-alert',
        loadChildren: () => SystemAlertModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':contextId/additional-property-definition',
        loadChildren: () => AdditionalPropertyDefinitionModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewAdditionalPropertyValues],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':productAlias/data-table',
        loadChildren: () => DataTableModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewProducts, Permission.ViewDataTables],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':productAlias/release-selection-settings',
        component: ReleaseSelectionSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageProducts],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
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
        path: ':productAlias/select-product-release/:environment',
        component: SelectProductRelease,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageReleases],
            masterComponent: ListProductPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Product routing module.
 */
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class ProductRoutingModule { }
