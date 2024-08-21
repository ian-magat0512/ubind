import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { ListCustomerClaimPage } from './list-customer-claim/list-customer-claim.page';
import { CreateClaimPage } from './create-claim/create-claim.page';
import { DetailClaimPage } from './detail-claim/detail-claim.page';
import { EnvironmentGuard } from '@app/providers/guard/environment.guard';
import { UpdateClaimPage } from './update-claim/update-claim.page';
import { DetailClaimVersionPage } from './detail-claim-version/detail-claim-version.page';
import { UpdateClaimVersionPage } from './update-claim-version/update-claim-version.page';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import {
    ShowMasterComponentWhenNotSplit,
} from '../../components/show-master-when-not-split/show-master-when-not-split.component';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';
import { ListClaimMessagePage } from './list-claim-message/list-claim-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { ClaimCommonModule } from './claim-common.module';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { ListClaimVersionPage } from './list-claim-version/list-claim-version.page';
import { TypedRoutes } from '@app/routing/typed-route';
import { BlankModule } from '../blank/blank.module';
import { AdditionalPropertyValueModule } from '../additional-property-values/additional-property-value.module';

const routes: TypedRoutes = [
    {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
    },
    {
        path: 'list',
        loadChildren: () => BlankModule,
        canActivate: [EnvironmentGuard, PermissionGuard],
        component: ShowMasterComponentWhenNotSplit,
        data: {
            mustHavePermissions: [Permission.ViewClaims],
            masterComponent: ListCustomerClaimPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a claim to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewClaims],
            masterComponent: ListCustomerClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateClaimPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageClaims],
            masterComponent: ListCustomerClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/update',
        component: UpdateClaimPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageClaims],
            masterComponent: ListCustomerClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId',
        component: DetailClaimPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewClaims],
            masterComponent: ListCustomerClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListCustomerClaimPage,
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':claimId/version/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [
                Permission.ManageClaims,
                Permission.ManageAllClaims,
                Permission.ManageAllClaimsForAllOrganisations,
            ],
            masterComponent: ListClaimVersionPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a quote version to view details',
        },
    },
    {
        path: ':claimId/version/:claimVersionId',
        component: DetailClaimVersionPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageClaims],
            masterComponent: ListClaimVersionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/version/:entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageClaims],
            masterComponent: ListCustomerClaimPage,
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/version/:versionnumber/update',
        component: UpdateClaimVersionPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageClaims],
            masterComponent: ListCustomerClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/message/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListClaimMessagePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a message to view details',
        },
    },
    {
        path: ':claimId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListClaimMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':claimId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListClaimMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':claimId/version/:versionId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListClaimMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':claimId/version/:versionId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListClaimMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
];

/**
 * Export customer claim module class
 */
@NgModule({
    declarations: [],
    imports: [
        ClaimCommonModule,
        MessageSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class CustomerClaimModule { }
