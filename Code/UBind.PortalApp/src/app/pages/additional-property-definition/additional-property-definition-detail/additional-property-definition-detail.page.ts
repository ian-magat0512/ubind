import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { AdditionalPropertyDefinition } from '@app/models/additional-property-item-view.model';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import {
    PopoverAdditionalPropertyComponent,
} from '@app/components/additional-properties/popover-additional-property/popover-additional-property.component';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { AdditionalPropertyDefinitionViewModel } from '@app/viewmodels/additional-property-definition.viewmodel';
import { EventService } from '@app/services/event.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { AlertController, ToastController } from '@ionic/angular';
import { AdditionalPropertyDefinitionBasePage } from '../additional-property-definition-base.page';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { Subject } from 'rxjs';
import { PermissionService } from '@app/services/permission.service';
import { Permission } from '@app/helpers';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { ActivatedRoute } from '@angular/router';
import { PopoverCommand } from '@app/models/popover-command';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { AdditionalPropertyDefinitionSchemaTypeEnum } from '@app/models/additional-property-schema-type.enum';
import { AdditionalPropertyDefinitionTypeEnum } from '@app/models/additional-property-definition-types.enum';

/**
 * Additional property detail page component.
 */
@Component({
    selector: 'app-additional-property-detail',
    templateUrl: './additional-property-definition-detail.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
    styles: [
        scrollbarStyle,
    ],
})

export class AdditionalPropertyDefinitionDetailsPage
    extends AdditionalPropertyDefinitionBasePage implements OnInit, OnDestroy {
    public additionalPropertyId: string;
    public additionalProperty: AdditionalPropertyDefinition;
    public additionalPropertyDetailsListItems: Array<DetailsListItem>;
    protected canManageAdditionalProperties: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;

    public constructor(
        public layoutManager: LayoutManagerService,
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        public sharedPopoverService: SharedPopoverService,
        private alertControl: AlertController,
        private toastController: ToastController,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        protected tenantService: TenantService,
        protected productService: ProductService,
        protected organisationApiService: OrganisationApiService,
        private shareLoaderService: SharedLoaderService,
        public permissionService: PermissionService,
        appConfigService: AppConfigService,
        route: ActivatedRoute,
        protected additionalPropertyValueService: AdditionalPropertyValueService,
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
            this.additionalPropertyId = this.routeHelper.getParam('additionalPropertyId');
            this.load();
        });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public userDidTapReturnButton(): void {
        this.navProxy.navigateBackOne(true);
    }

    public userDidTapEditButton(): void {
        let newPathSegments: Array<string> = this.routeHelper.appendPathSegment('edit');
        this.navProxy.navigate(newPathSegments);
    }

    public async userDidTapMoreButton(): Promise<void> {
        this.flipMoreIcon = true;
        let popoverDissmissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                switch (command.data.action.actionName) {
                    case 'delete':
                        this.confirmDelete();
                        break;
                    default:
                        break;
                }
            }
        };
        await this.sharedPopoverService.show(
            {
                component: PopoverAdditionalPropertyComponent,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
            }
            , 'Additional property popover',
            popoverDissmissAction,
        );
    }

    private async confirmDelete(): Promise<void> {
        let entityTypeInPluralForm: string = this.additionalPropertiesService.getEntityDescriptionInPluralForm(
            this.entityType,
        );
        const alert: HTMLIonAlertElement = await this.alertControl.create({
            id: 'delete-property',
            header: 'Delete additional property',
            message: 'By deleting an additional property, all associated property values are also permanently '
                + 'deleted. Are you sure you want to delete this additional property from '
                + `${entityTypeInPluralForm} associated with ${this.contextName}?`,
            buttons: [{
                text: 'Cancel',
            },
            {
                text: 'Ok',
                handler: async (): Promise<any> => {
                    await this.shareLoaderService.presentWait();
                    // If parentContextTenantId is undefined the contextId will contain the tenantId.
                    let tenantId: string = this.parentContextId || this.contextId;
                    this.additionalPropertiesService.delete(tenantId, this.additionalPropertyId)
                        .pipe(
                            finalize(() => this.shareLoaderService.dismiss()),
                        )
                        .subscribe(() => {
                            this.showSnackbarOnSuccessfulDelete(this.additionalProperty)
                                .then(() => {
                                    this.userDidTapReturnButton();
                                });
                        });
                },
            }],
        });
        await alert.present();
    }

    private load(): void {
        this.additionalPropertiesService
            .getAdditionalPropertyDefinitionById(this.parentContextId, this.additionalPropertyId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe(
                async (additionalPropertyResult: AdditionalPropertyDefinition) => {
                    this.additionalProperty = additionalPropertyResult;
                    await this.loadDetails();
                },
                (err: any) => {
                    this.errorMessage = 'There was a problem loading the details of additional property definition';
                    throw err;
                },
            );
    }

    private async loadDetails(): Promise<void> {
        this.additionalProperty.contextName = this.contextName;
        this.additionalProperty.contextType = this.contextType;
        let schema: string;
        if (this.additionalProperty.type == AdditionalPropertyDefinitionTypeEnum.StructuredData
            && this.additionalProperty.schemaType != AdditionalPropertyDefinitionSchemaTypeEnum.None) {
            schema = (this.additionalProperty.schemaType == AdditionalPropertyDefinitionSchemaTypeEnum.Custom)
                ? this.additionalProperty.customSchema
                : await this.additionalPropertyValueService.getDefaultSchema(this.additionalProperty.schemaType);
        }

        this.additionalPropertyDetailsListItems = AdditionalPropertyDefinitionViewModel.createDetailListItem(
            this.additionalProperty, schema,
        );
    }

    private async showSnackbarOnSuccessfulDelete(additionalProperty: AdditionalPropertyDefinition): Promise<void> {
        let inPluralForm: string = this.additionalPropertiesService.getEntityDescriptionInPluralForm(this.entityType);
        const snackbar: HTMLIonToastElement = await this.toastController.create({
            id: additionalProperty.id,
            message: `${additionalProperty.name} property removed from ${inPluralForm} associated with `
                + `${this.contextName}`,
            duration: 3000,
        });
        return await snackbar.present();
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];

        if (this.canManageAdditionalProperties) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Additional Properties",
                true,
                (): void => {
                    return this.userDidTapEditButton();
                },
            ));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
