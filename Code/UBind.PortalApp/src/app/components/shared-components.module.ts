import { NgModule } from '@angular/core';
import { LoaderComponent } from './loader/loader.component';
import { SearchComponent } from './search/search.component';
import { SharedModule } from '../shared.module';
import { PopoverPersonComponent } from './popover-person/popover-person.component';
import { PopoverAssignPortalComponent } from './popover-assign-portal/popover-assign-portal.component';
import { PopoverMyAccountComponent } from './popover-my-account/popover-my-account.component';
import { DonutChartComponent } from './chart/donut-chart/donut-chart.component';
import { ColumnChartComponent } from './chart/column-chart/column-chart.component';
import { LineChartComponent } from './chart/line-chart/line-chart.component';
import { ProductFilterComponent } from './product-filter/product-filter.component';
import { ChartWidgetComponent } from './chart-widget/chart-widget.component';
import { EmailViewComponent } from './email-view/email-view.component';
import { QuoteVersionViewComponent } from './quote-version-view/quote-version-view.component';
import { QuoteDocumentViewComponent } from './quote-document-view/quote-document-view.component';
import { PremiumPolicyViewComponent } from './premium-policy-view/premium-policy-view.component';
import { DocumentPolicyViewComponent } from './document-policy-view/document-policy-view.component';
import { HistoryPolicyViewComponent } from './history-policy-view/history-policy-view.component';
import { ClaimPolicyViewComponent } from './claim-policy-view/claim-policy-view.component';
import { ExpandableItemComponent } from './expandable-item/expandable-item.component';
import { IonSelectSmallerArrowDirective } from '../directives/ion-select-smaller-arrow.directive';
import { ScrollHorizontalDirective } from '@app/directives/scroll-horizontal.directive';
import { ScrollMouseDragDirective } from '@app/directives/scroll-mousedrag.directive';
import { ScrollBlockComponent } from './scroll-block/scroll-block.component';
import { BreakdownPolicyViewComponent } from './breakdown-policy-view/breakdown-policy-view.component';
import { QuestionsViewComponent } from './questions-view/questions-view.component';
import { PopoverViewComponent } from './popover-view/popover-view.component';
import { EntityDetailsListComponent } from './entity-details-list/entity-details-list.component';
import { EntityDetailsListItemComponent } from './entity-details-list-item/entity-details-list-item.component';
import { PopoverRoleComponent } from './popover-role/popover.role.component';
import { QuoteFilterComponent } from './filter/quote-filter.component';
import { PopoverPolicyPage } from '@app/pages/policy/popover-policy/popover-policy.page';
import { PopoverListPolicyPage } from '../pages/policy/popover-list-policy/popover-list-policy.page';
import { PopoverQuotePage } from '@app/pages/quote/popover-quote/popover-quote.page';
import { PopoverTenantPage } from '@app/pages/tenant/popover-tenant/popover-tenant.page';
import { PopoverProductPage } from '@app/pages/product/popover-product/popover-product.page';
import { PopoverReleasePage } from '@app/pages/release/popover-release/popover-release.page';
import { PopoverPortalPage } from '@app/pages/portal/popover-portal/popover-portal.page';
import { AddressComponent } from './address/address.component';
import {
    AdditionalPropertiesViewComponent,
} from './additional-properties/additional-properties-settings-view/additional-properties-settings-view.component';
import {
    PopoverAdditionalPropertyComponent,
} from './additional-properties/popover-additional-property/popover-additional-property.component';
import { FilterChipsComponent } from './filter-chips/filter-chips.component';
import { PopoverTransactionComponent } from './popover-transaction/popover-transaction.component';
import { DetailListItemsEditFormComponent } from './detail-list-item-edit-form/detail-list-item-edit-form.component';
import { FilterSortPage } from '@app/pages/filter-sort/filter-sort-page';
import { FilterComponent } from '@app/components/filter/filter.component';
import { UserRoleViewComponent } from '@app/pages/user/user-role-view/user-role-view.component';
import {
    PopoverAgentMoreSelectionComponent,
} from './popover-agent-more-selection/popover-agent-more-selection.component';
import { SegmentSelectedScrollIntoViewDirective } from '@app/directives/segment-selected-scroll-Into-view.directive';
import { PopoverQuoteVersionPage } from '@app/pages/quote/popover-quote/popover-quote-version.page';
import { PopoverPolicyTransactionPage } from '@app/pages/policy/popover-policy/popover-policy-transaction.page';
import {
    PopoverContactDetailsActionsComponent,
} from './popover-contact-details-actions/popover-contact-details-actions.component';
import { ShowMasterComponentWhenNotSplit } from './show-master-when-not-split/show-master-when-not-split.component';
import { PolicyFilterComponent } from './filter/policy-filter.component';
import { PopoverActionsComponent } from './popover-actions/popover-actions.component';
import { DialogComponent } from './dialog/dialog.component';
import { PopoverDkimSettingPage } from '@app/pages/dkim-settings/popover-dkim-settings.page';
import { PopoverDataTableComponent } from '@app/pages/data-table/popover-data-table/popover-data-table.component';
import {
    EntityActionButtonListComponent,
} from './entity-action-button-list/entity-action-button-list.component';
import { LibraryIconComponent } from './library-icon/library-icon.component';
import { EntityDetailSegmentListComponent } from './entity-detail-segment-list/entity-detail-segment-list.component';
import {
    PopoverManagingOrganisationComponent,
} from '@app/pages/organisation/popover-managing-organisation/popover-managing-organisation.component';
import { StaticItemFilterSelectComponent } from './item-filter-select/static-item-filter-select.component';
import { ItemFilterSelectComponent } from './item-filter-select/item-filter-select.component';
import {
    PopoverSsoConfigurationPage,
} from '@app/pages/organisation/authentication-methods/popover-sso-configuration/popover-sso-configuration';
import {
    ProductReleaseSelectionComponent,
} from './product-release-selection-view/product-release-selection-view.component';
import {
    AssignPortalComponent,
} from '@app/components/assign-portal/assign-portal.component';
import {
    AssignUserPortalComponent,
} from '@pages/user/assign-user-portal/assign-user-portal.component';
import { EmailContentComponent } from './email-content/email-content.component';

/**
 * Export shared components module class
 * This is the ng modules for the components.
 */
@NgModule({
    declarations: [
        FilterSortPage,
        FilterComponent,
        LoaderComponent,
        SearchComponent,
        PopoverPersonComponent,
        PopoverAssignPortalComponent,
        PopoverTransactionComponent,
        PopoverMyAccountComponent,
        DonutChartComponent,
        ColumnChartComponent,
        LineChartComponent,
        ProductFilterComponent,
        ChartWidgetComponent,
        EmailViewComponent,
        QuoteVersionViewComponent,
        QuoteDocumentViewComponent,
        PremiumPolicyViewComponent,
        DocumentPolicyViewComponent,
        HistoryPolicyViewComponent,
        ClaimPolicyViewComponent,
        ExpandableItemComponent,
        IonSelectSmallerArrowDirective,
        ScrollHorizontalDirective,
        ScrollMouseDragDirective,
        SegmentSelectedScrollIntoViewDirective,
        ScrollBlockComponent,
        BreakdownPolicyViewComponent,
        EntityActionButtonListComponent,
        EntityDetailsListComponent,
        EntityDetailsListItemComponent,
        EntityDetailSegmentListComponent,
        QuestionsViewComponent,
        PopoverViewComponent,
        PopoverRoleComponent,
        AddressComponent,
        PopoverContactDetailsActionsComponent,
        QuoteFilterComponent,
        PolicyFilterComponent,
        PopoverPolicyPage,
        PopoverDkimSettingPage,
        PopoverListPolicyPage,
        PopoverPolicyTransactionPage,
        PopoverQuotePage,
        PopoverQuoteVersionPage,
        PopoverTenantPage,
        PopoverProductPage,
        PopoverReleasePage,
        PopoverPortalPage,
        PopoverSsoConfigurationPage,
        PopoverRoleComponent,
        AddressComponent,
        AdditionalPropertiesViewComponent,
        PopoverAdditionalPropertyComponent,
        FilterChipsComponent,
        FilterChipsComponent,
        DetailListItemsEditFormComponent,
        UserRoleViewComponent,
        PopoverAgentMoreSelectionComponent,
        ShowMasterComponentWhenNotSplit,
        PopoverActionsComponent,
        DialogComponent,
        PopoverDataTableComponent,
        LibraryIconComponent,
        PopoverManagingOrganisationComponent,
        ItemFilterSelectComponent,
        StaticItemFilterSelectComponent,
        ProductReleaseSelectionComponent,
        AssignPortalComponent,
        AssignUserPortalComponent,
        EmailContentComponent,
    ],
    imports: [
        SharedModule,
    ],
    exports: [
        FilterSortPage,
        LoaderComponent,
        SearchComponent,
        DonutChartComponent,
        ColumnChartComponent,
        LineChartComponent,
        ProductFilterComponent,
        ChartWidgetComponent,
        EmailViewComponent,
        QuoteVersionViewComponent,
        QuoteDocumentViewComponent,
        PremiumPolicyViewComponent,
        DocumentPolicyViewComponent,
        HistoryPolicyViewComponent,
        ClaimPolicyViewComponent,
        ExpandableItemComponent,
        ChartWidgetComponent,
        ScrollHorizontalDirective,
        ScrollMouseDragDirective,
        SegmentSelectedScrollIntoViewDirective,
        ScrollBlockComponent,
        BreakdownPolicyViewComponent,
        QuestionsViewComponent,
        PopoverViewComponent,
        AddressComponent,
        EntityActionButtonListComponent,
        EntityDetailsListComponent,
        EntityDetailsListItemComponent,
        BreakdownPolicyViewComponent,
        AdditionalPropertiesViewComponent,
        PopoverContactDetailsActionsComponent,
        QuoteFilterComponent,
        PolicyFilterComponent,
        PopoverPolicyPage,
        PopoverDkimSettingPage,
        PopoverListPolicyPage,
        PopoverPolicyTransactionPage,
        PopoverQuotePage,
        PopoverQuoteVersionPage,
        PopoverTenantPage,
        PopoverProductPage,
        PopoverReleasePage,
        PopoverPortalPage,
        PopoverSsoConfigurationPage,
        DetailListItemsEditFormComponent,
        EntityDetailsListComponent,
        EntityDetailsListItemComponent,
        EntityDetailSegmentListComponent,
        PopoverAdditionalPropertyComponent,
        FilterChipsComponent,
        UserRoleViewComponent,
        PopoverAgentMoreSelectionComponent,
        ShowMasterComponentWhenNotSplit,
        PopoverActionsComponent,
        DialogComponent,
        LibraryIconComponent,
        ItemFilterSelectComponent,
        StaticItemFilterSelectComponent,
        ProductReleaseSelectionComponent,
        AssignPortalComponent,
        AssignUserPortalComponent,
        EmailContentComponent,
    ],
})
export class SharedComponentsModule { }
