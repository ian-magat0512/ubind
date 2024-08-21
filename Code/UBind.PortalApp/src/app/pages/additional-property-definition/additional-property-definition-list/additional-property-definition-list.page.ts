import { Component, Injector, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import {
    AdditionalPropertyDefinition,
} from '@app/models/additional-property-item-view.model';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import {
    additionalPropertyCategories,
    AdditionalPropertyCategoryConfig,
} from '@app/helpers/additional-property-categories';
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { finalize, takeUntil, map } from 'rxjs/operators';
import { Subject, SubscriptionLike } from 'rxjs';
import { AdditionalPropertyDefinitionBasePage } from '../additional-property-definition-base.page';
import * as ChangeCase from 'change-case';
import { titleCase } from 'title-case';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { Permission } from '@app/helpers';
import { PermissionService } from '@app/services/permission.service';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { ActivatedRoute } from '@angular/router';
import { AdditionalPropertyDefinitionTypeEnum } from '@app/models/additional-property-definition-types.enum';
import { AdditionalPropertyDefinitionViewModel } from '@app/viewmodels/additional-property-definition.viewmodel';

/**
 * Customer resource model
 */
interface ChipModel {
    icon: string;
    iconLibrary: string;
    isRoundIcon: boolean;
    label: string;
}

/**
 * Additional properties list page component.
 */
@Component({
    selector: 'app-additional-properties',
    templateUrl: './additional-property-definition-list.page.html',
    styleUrls: [
        './additional-property-definition-list.page.scss',
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
    styles: [
        scrollbarStyle,
    ],
})

export class AdditionalPropertyDefinitionListPage
    extends AdditionalPropertyDefinitionBasePage implements OnInit, OnDestroy {
    public additionalPropertyDefinitions: Array<AdditionalPropertyDefinitionViewModel>;
    public contextChip: ChipModel;
    public entityChip: ChipModel;
    public subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    protected canManageAdditionalProperties: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public actionButtonList: Array<ActionButton>;

    public constructor(
        public layoutManager: LayoutManagerService,
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        private additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        protected tenantService: TenantService,
        protected productService: ProductService,
        protected organisationApiService: OrganisationApiService,
        public permissionService: PermissionService,
        appConfigService: AppConfigService,
        route: ActivatedRoute,
    ) {
        super(
            eventService,
            elementRef,
            injector,
            routeHelper,
            tenantService,
            productService,
            organisationApiService,
            navProxy,
            appConfigService,
            route,
        );
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.canManageAdditionalProperties =
            this.permissionService.hasOneOfPermissions([
                Permission.EditAdditionalPropertyValues, Permission.ManageTenants]);
        this.destroyed = new Subject<void>();
        this.initializeActionButtonList();
        this.loadAllBaseParametersBeforeLoadingOtherData(() => {
            this.load();
        });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public userDidTapReturnButton(): void {
        let params: any = {
            "segment": 'Settings',
        };
        this.navProxy.navigateBackN(2, true, { queryParams: params });
    }

    public userDidTapAddButton(): void {
        let newPathSegments: Array<string> = this.routeHelper.appendPathSegment('create');
        this.navProxy.navigate(newPathSegments);
    }

    public additionalPropertyItemClicked(
        additionalPropertyDefinitionViewModel: AdditionalPropertyDefinitionViewModel): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push(additionalPropertyDefinitionViewModel.id);
        this.navProxy.navigateForward(pathSegments, true);
    }

    private getIconByContextType(contextType: AdditionalPropertyDefinitionContextType): string {
        let icon: string = 'none';

        switch (contextType) {
            case AdditionalPropertyDefinitionContextType.Product:
                icon = 'cube';
                break;
            case AdditionalPropertyDefinitionContextType.Tenant:
                icon = 'cloud-circle';
                break;
            case AdditionalPropertyDefinitionContextType.Organisation:
                icon = 'business';
                break;
        }

        return icon;
    }

    private load(): void {
        const errorDescription: string
            = "There was a problem loading the list of additional properties for this entity ";
        if (this.parentContextId) {
            this.additionalPropertiesService.
                getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextIdAndParentContextId(
                    this.routeHelper.getContextTenantAlias(),
                    this.contextType,
                    this.entityType,
                    this.contextId,
                    this.parentContextId,
                )
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.isLoading = false),
                    map((additionalPropertyResults: Array<AdditionalPropertyDefinition>) => {
                        return additionalPropertyResults.map((item: AdditionalPropertyDefinition) => {
                            return this.formatTypeDisplayName(item);
                        });
                    }),
                )
                .subscribe(
                    (additionalPropertyResults: Array<AdditionalPropertyDefinitionViewModel>) => {
                        this.additionalPropertyDefinitions = additionalPropertyResults;
                    },
                    (err: any) => {
                        this.errorMessage = errorDescription;
                        throw err;
                    },
                );
        } else {
            this.additionalPropertiesService.
                getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextId(
                    this.routeHelper.getContextTenantAlias(),
                    this.contextType,
                    this.entityType,
                    this.contextId,
                )
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.isLoading = false),
                    map((additionalPropertyResults: Array<AdditionalPropertyDefinition>) => {
                        return additionalPropertyResults.map((item: AdditionalPropertyDefinition) => {
                            return this.formatTypeDisplayName(item);
                        });
                    }),
                )
                .subscribe(
                    (additionalPropertyResults: Array<AdditionalPropertyDefinitionViewModel>) => {
                        this.additionalPropertyDefinitions = additionalPropertyResults;
                    },
                    (err: any) => {
                        this.errorMessage = errorDescription;
                        throw err;
                    },
                );
        }

        this.contextChip = {
            icon: this.getIconByContextType(this.contextType),
            isRoundIcon: false,
            iconLibrary: IconLibrary.IonicV4,
            label: this.contextName,
        };

        const config: AdditionalPropertyCategoryConfig =
            additionalPropertyCategories.filter((apc: AdditionalPropertyCategoryConfig) =>
                apc.entityType === this.entityType)[0];
        this.entityChip = {
            icon: config.icon,
            iconLibrary: config.iconLibrary,
            isRoundIcon: config.isRoundIcon,
            label: titleCase(ChangeCase.sentenceCase(this.entityType)),
        };
    }

    private formatTypeDisplayName(item: AdditionalPropertyDefinition): AdditionalPropertyDefinitionViewModel {
        const viewModel: AdditionalPropertyDefinitionViewModel = new AdditionalPropertyDefinitionViewModel(item);
        if (viewModel.type) {
            viewModel.typeDisplayName = item.type.replace(/([a-z])([A-Z])/g, '$1 $2');
        }

        return viewModel;
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (this.canManageAdditionalProperties) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Additional Properties",
                true,
                (): void => {
                    return this.userDidTapAddButton();
                },
            ));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    public mapEnumToDisplay(enumValue: string): string {
        switch (enumValue) {
            case AdditionalPropertyDefinitionTypeEnum.Text:
            case AdditionalPropertyDefinitionTypeEnum.StructuredData:
                return this.formatEnumValue(AdditionalPropertyDefinitionTypeEnum[enumValue]);
            default:
                return '';
        }
    }

    private formatEnumValue(inputString: string): string {
        return inputString.replace(/([a-z])([A-Z])/g, '$1 $2');
    }
}
