import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CreateQuotePage } from './create-quote/create-quote.page';
import { EditQuotePage } from './edit-quote/edit-quote.page';
import { DetailQuotePage } from './detail-quote/detail-quote.page';
import { DetailQuoteVersionPage } from './detail-quote-version/detail-quote-version.page';
import { EditQuoteVersionPage } from './edit-quote-version/edit-quote-version.page';
import { ListQuotePage } from './list-quote/list-quote.page';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { QuoteCommonModule } from './quote-common.module';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { ListQuoteVersionPage } from './list-quote-version/list-quote-version.page';
import { SetExpiryQuotePage } from './set-expiry-quote/set-expiry-quote.page';
import { QuoteFilterComponent } from '@app/components/filter/quote-filter.component';
import { ListQuoteMessagePage } from './list-quote-message/list-quote-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { ListQuoteVersionMessagePage } from './list-quote-version-message/list-quote-version-message.page';
import { AdditionalPropertyValueModule } from '../additional-property-values/additional-property-value.module';
import { TypedRoutes } from '@app/routing/typed-route';
import { ReviewQuotePage } from './edit-quote/review-quote.page';
import { EndorseQuotePage } from './edit-quote/endorse-quote.page';
import { IssuePolicyPage } from '../policy/issue-policy/issue-policy.page';

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
                [Permission.ViewQuotes,
                    Permission.ViewAllQuotes,
                    Permission.ViewAllQuotesFromAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a quote to view details',
        },
    },
    {
        path: 'filter',
        component: QuoteFilterComponent,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewQuotes,
                    Permission.ViewAllQuotes,
                    Permission.ViewAllQuotesFromAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateQuotePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageQuotes,
                    Permission.ManageAllQuotes,
                    Permission.ManageAllQuotesForAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/set-expiry',
        component: SetExpiryQuotePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageQuotes,
                    Permission.ManageAllQuotes,
                    Permission.ManageAllQuotesForAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId',
        component: DetailQuotePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewQuotes,
                    Permission.ViewAllQuotes,
                    Permission.ViewAllQuotesFromAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':quoteId/edit',
        component: EditQuotePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManageQuotes,
                    Permission.ManageAllQuotes,
                    Permission.ManageAllQuotesForAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/review',
        component: ReviewQuotePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ReviewQuotes,
                    Permission.ManageQuotes,
                    Permission.ManageAllQuotes,
                    Permission.ManageAllQuotesForAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/endorse',
        component: EndorseQuotePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.EndorseQuotes,
                    Permission.ManageQuotes,
                    Permission.ManageAllQuotes,
                    Permission.ManageAllQuotesForAllOrganisations],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/version/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewQuoteVersions],
            masterComponent: ListQuoteVersionPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a quote version to view details',
        },
    },
    {
        path: ':quoteId/version/:quoteVersionId',
        component: DetailQuoteVersionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewQuoteVersions],
            masterComponent: ListQuoteVersionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':quoteId/version/:entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListQuoteVersionPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/version/:quoteVersionId/edit',
        component: EditQuoteVersionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageQuoteVersions],
            masterComponent: ListQuoteVersionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/message/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListQuoteMessagePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a message to view details',
        },
    },
    {
        path: ':quoteId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListQuoteMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':quoteId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListQuoteMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':quoteId/version/:quoteVersionId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListQuoteVersionMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':quoteId/version/:quoteVersionId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListQuoteVersionMessagePage,
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
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/issue-policy',
        component: IssuePolicyPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManagePolicies, Permission.ManageQuotes, Permission.ManagePolicyNumbers],
            masterComponent: ListQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export Quote Module class.
 * This class manage Ng Module declarations of quote.
 */
@NgModule({
    declarations: [
        DetailQuotePage,
        ListQuotePage,
        SetExpiryQuotePage,
    ],
    imports: [
        QuoteCommonModule,
        MessageSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class QuoteModule { }
