import { Component, ElementRef, Injector, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { EntityEditFieldOption, FieldShowHideRule } from '@app/models/entity-edit-field-option';
import { EntityType } from '@app/models/entity-type.enum';
import { DataTableDefinitionResourceModel } from '@app/resource-models/data-table-definition.resource-model';
import { DataTableDefinitionApiService } from '@app/services/api/data-table-definition-api.service';
import { CsvValidatorService } from '@app/services/csv-validator.service';
import { EventService } from '@app/services/event.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { DataTableDefinitionViewModel } from "@app/viewmodels/data-table-definition.viewmodel";
import { Observable } from 'rxjs';
import { finalize, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import {
    DetailListItemsEditFormComponent,
} from '@app/components/detail-list-item-edit-form/detail-list-item-edit-form.component';
import { DataTableDefinitionService } from '@app/services/data-table-definition.service';
import { EditFormDataTablePage } from '@app/pages/data-table/edit-form-data-table.page';

/**
 * Component for creating data table.
 */
@Component({
    selector: 'app-create-data-table',
    templateUrl: './create-data-table.page.html',
})
export class CreateDataTablePage extends EditFormDataTablePage implements OnInit, OnDestroy {
    public title: string = "Create Data Table";
    public isLoading: boolean = false;
    public errorMessage: string;
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public fieldShowHideRules: Array<FieldShowHideRule> = [];
    private tenantAlias: string;
    private productAlias: string;
    private organisationId: string;
    @ViewChild(DetailListItemsEditFormComponent) private editFormComponent: DetailListItemsEditFormComponent;

    public constructor(
        public eventService: EventService,
        public elementRef: ElementRef,
        public injector: Injector,
        protected routeHelper: RouteHelper,
        private sharedAlertService: SharedAlertService,
        protected navProxy: NavProxyService,
        private formBuilder: FormBuilder,
        private csvValidatorService: CsvValidatorService,
        private sharedLoaderService: SharedLoaderService,
        private dataTableDefinitionApiService: DataTableDefinitionApiService,
        private dataTableDefinitionService: DataTableDefinitionService,
    ) {
        super(routeHelper, navProxy, eventService, elementRef, injector);
    }

    public async ngOnInit(): Promise<void> {
        this.tenantAlias = this.routeHelper.getContextTenantAlias();
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.detailList = DataTableDefinitionViewModel.createFormDetailsList(
            this.csvValidatorService.csvValidator({
                size: null,
            }),
            await this.dataTableDefinitionService.jsonTableSchemaValidator());
        this.buildForm();
    }

    protected buildForm(): void {
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        this.form = this.formBuilder.group(controls);
        this.setFormBehavior();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async userDidTapSaveButton(): Promise<void> {
        await this.sharedLoaderService.presentWithDelay("Creating data table...", null, 450);
        const request: Observable<DataTableDefinitionResourceModel> = await this.sendCreateRequest();
        request.pipe(
            catchError((error: any) => {
                this.dataTableDefinitionService.highlightCsvError(
                    this.form, error);
                return throwError(error);
            }),
            finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe((result: DataTableDefinitionResourceModel) => {
                this.eventService.getEntityUpdatedSubject('DataTableDefinition').next(result);
                this.sharedAlertService.showToast(
                    `${this.form.get("name").value} data table was created`,
                );
                this.navigateToDetail(result.id);
            });
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.form.dirty) {
            await this.sharedAlertService.showWithActionHandler({
                header: 'Unsaved Changes',
                subHeader: 'You have unsaved changes, are you sure you wish to close the current view without saving '
                    + 'them?',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            return;
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            this.navigateBasedOnTheMode();
                        },
                    },
                ],
            });
        } else {
            this.navigateBasedOnTheMode();
        }
    }

    private async sendCreateRequest(): Promise<Observable<DataTableDefinitionResourceModel>> {
        let tenantAlias: string = this.tenantAlias;
        const formItemAlias: { [key: string]: string } = DataTableDefinitionViewModel.dataTableFormItemAlias;
        const requestPayload: {
            entityIdOrAlias: string;
            entityType: EntityType;
        } = this.productAlias ? {
            entityIdOrAlias: this.productAlias,
            entityType: EntityType.Product,
        } : this.organisationId ?
        {
            entityIdOrAlias: this.organisationId,
            entityType: EntityType.Organisation,
        } : {
            entityIdOrAlias: tenantAlias,
            entityType: EntityType.Tenant,
        };
        return this.dataTableDefinitionApiService.createDataTableFromCsv(
            tenantAlias,
            requestPayload.entityType,
            requestPayload.entityIdOrAlias,
            {
                name: this.form.get(formItemAlias.name).value,
                alias: this.form.get(formItemAlias.alias).value,
                tableSchema: JSON.parse(this.form.get(formItemAlias.tableSchemaJson).value),
                csvData: this.form.get(formItemAlias.csvData).value,
                memoryCachingEnabled: this.form.get(formItemAlias.memoryCachingEnabled).value,
                cacheExpiryInSeconds: this.form.get(formItemAlias.cacheExpiryInSeconds).value,
            },
        );
    }

    protected navigateEitherFromCreateOrEdit(mode: string): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments = pathSegments.filter((segment: string) => segment !== mode);
        pathSegments.push("list-detail");
        this.navProxy.navigate(pathSegments);
    }

    private async navigateToDetail(dataTableId: string): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop(); // remove 'create' from url
        pathSegments.push(dataTableId);
        await this.navProxy.navigate(pathSegments);
    }
}
