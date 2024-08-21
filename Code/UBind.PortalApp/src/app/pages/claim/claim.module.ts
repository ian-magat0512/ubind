/* eslint-disable @typescript-eslint/explicit-function-return-type */
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { CreateClaimPage } from './create-claim/create-claim.page';
import { DetailClaimPage } from './detail-claim/detail-claim.page';
import { DetailCustomerClaimPage } from './detail-customer-claim/detail-customer-claim.page';
import { UpdateClaimPage } from './update-claim/update-claim.page';
import { ListClaimPage } from './list-claim/list-claim.page';
import { ListCustomerClaimPage } from './list-customer-claim/list-customer-claim.page';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { NotificationAcknowledgeClaimPage } from './notification-acknowledge-claim/notification-acknowledge-claim.page';
import { ReviewClaimPage } from './review-claim/review-claim.page';
import { AssessClaimPage } from './assess-claim/assess-claim.page';
import { NumberAssignClaimPage } from './number-assign-claim/number-assign-claim.page';
import { NumberUpdateClaimPage } from './number-update-claim/number-update-claim.page';
import { DetailClaimVersionPage } from './detail-claim-version/detail-claim-version.page';
import { SettleClaimPage } from './settle-claim/settle-claim.page';
import { UpdateClaimVersionPage } from './update-claim-version/update-claim-version.page';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import {
    ShowMasterComponentWhenNotSplit,
} from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';
import { ListClaimMessagePage } from './list-claim-message/list-claim-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { ClaimCommonModule } from './claim-common.module';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { ListClaimVersionPage } from './list-claim-version/list-claim-version.page';
import { TypedRoutes } from '@app/routing/typed-route';
import { AdditionalPropertyValueModule } from '../additional-property-values/additional-property-value.module';

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
            mustHavePermissions:
                [Permission.ViewClaims,
                    Permission.ViewAllClaims,
                    Permission.ViewAllClaimsFromAllOrganisations],
            masterComponent: ListClaimPage,
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
            mustHavePermissions:
                [Permission.ViewClaims,
                    Permission.ViewAllClaims,
                    Permission.ViewAllClaimsFromAllOrganisations],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId',
        component: DetailClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewClaims,
                    Permission.ViewAllClaims,
                    Permission.ViewAllClaimsFromAllOrganisations],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':claimId/update',
        component: UpdateClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'customer',
        component: ListCustomerClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewClaims,
                    Permission.ViewAllClaims,
                    Permission.ViewAllClaimsFromAllOrganisations],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/customer',
        component: DetailCustomerClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewClaims,
                    Permission.ViewAllClaims,
                    Permission.ViewAllClaimsFromAllOrganisations],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/number-assign',
        component: NumberAssignClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/number-update',
        component: NumberUpdateClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/notification-acknowledge',
        component: NotificationAcknowledgeClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations,
                    Permission.AcknowledgeClaimNotifications],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/review',
        component: ReviewClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations,
                    Permission.ReviewClaims],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/assess',
        component: AssessClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations,
                    Permission.AssessClaims],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/settle',
        component: SettleClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations,
                    Permission.SettleClaims],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
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
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [
                Permission.ManageClaims,
                Permission.ManageAllClaims,
                Permission.ManageAllClaimsForAllOrganisations,
            ],
            masterComponent: ListClaimVersionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/version/:versionnumber/update',
        component: UpdateClaimVersionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageClaims,
                    Permission.ManageAllClaims,
                    Permission.ManageAllClaimsForAllOrganisations],
            masterComponent: ListClaimPage,
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
    {
        path: ':entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':claimId/version/:entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListClaimPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export claim module class
 */
@NgModule({
    declarations: [],
    imports: [
        ClaimCommonModule,
        MessageSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class ClaimModule { }
