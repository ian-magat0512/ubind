import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CreateQuotePage } from './create-quote/create-quote.page';
import { EditQuotePage } from './edit-quote/edit-quote.page';
import { DetailCustomerQuotePage } from './detail-customer-quote/detail-customer-quote.page';
import { EditQuoteVersionPage } from './edit-quote-version/edit-quote-version.page';
import { ListCustomerQuotePage } from './list-customer-quote/list-customer-quote.page';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { QuoteCommonModule } from './quote-common.module';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { DetailQuoteVersionPage } from './detail-quote-version/detail-quote-version.page';
import { ListQuoteVersionPage } from './list-quote-version/list-quote-version.page';
import { CustomerQuoteAssociatePage } from './customer-quote-associate/customer-quote-associate.page';
import { EnvironmentGuard } from '@app/providers/guard/environment.guard';
import { QuoteFilterComponent } from '@app/components/filter/quote-filter.component';
import { ListQuoteMessagePage } from './list-quote-message/list-quote-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { ListQuoteVersionMessagePage } from './list-quote-version-message/list-quote-version-message.page';
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
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewQuotes],
            masterComponent: ListCustomerQuotePage,
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
            mustHavePermissions: [Permission.ViewQuotes],
            masterComponent: ListCustomerQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateQuotePage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageQuotes],
            masterComponent: ListCustomerQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId',
        component: DetailCustomerQuotePage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewQuotes],
            masterComponent: ListCustomerQuotePage,
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
            masterComponent: ListCustomerQuotePage,
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/associate/:associateInvitationId',
        component: CustomerQuoteAssociatePage,
        data: {
            mustHavePermissions: [Permission.ManageQuotes],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/edit',
        component: EditQuotePage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageQuotes],
            masterComponent: ListCustomerQuotePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':quoteId/version/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [EnvironmentGuard, PermissionGuard],
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
        canActivate: [EnvironmentGuard, PermissionGuard],
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
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListQuoteVersionPage,
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':quoteId/version/:quoteVersionId/edit',
        component: EditQuoteVersionPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
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
];

/**
 * Export customer quote module class.
 * This class manage Ng Module declaratios of Customer Quote.
 */
@NgModule({
    declarations: [
        DetailCustomerQuotePage,
        ListCustomerQuotePage,
        CustomerQuoteAssociatePage,
    ],
    imports: [
        QuoteCommonModule,
        MessageSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class CustomerQuoteModule { }
