import { AfterViewInit, Component, ElementRef, Injector } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { RouteHelper } from '@app/helpers/route.helper';
import { EntityEditFieldOption, FieldShowHideRule } from '@app/models/entity-edit-field-option';
import { CsvValidatorService } from '@app/services/csv-validator.service';
import { EventService } from '@app/services/event.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { DataTableDefinitionViewModel } from "@app/viewmodels/data-table-definition.viewmodel";
import { finalize, catchError } from 'rxjs/operators';
import { DataTableDefinitionApiService } from '@app/services/api/data-table-definition-api.service';
import { DataTableDefinitionResourceModel } from '@app/resource-models/data-table-definition.resource-model';
import { DataTableContentApiService } from '@app/services/api/data-table-content-api.service';
import { throwError, forkJoin, Observable } from 'rxjs';
import { HttpResponse } from '@angular/common/http';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { TenantService } from '@app/services/tenant.service';
import { DataTableCreateFromCsvModel } from '@app/resource-models/data-table-create-from-csv.resource-model';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { DataTableDefinitionService } from '@app/services/data-table-definition.service';
import { EditFormDataTablePage } from '@app/pages/data-table/edit-form-data-table.page';

/**
 * Component for editing data table component.
 */
@Component({
    selector: 'app-edit-data-table',
    templateUrl: './edit-data-table.page.html',
})
export class EditDataTablePage extends EditFormDataTablePage implements AfterViewInit {
    public title: string = "Edit Data Table";
    public isLoading: boolean = false;
    public errorMessage: string;
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public fieldShowHideRules: Array<FieldShowHideRule> = [];
    public dataTable: DataTableDefinitionViewModel;
    private tenantAlias: string;

    public constructor(
        public eventService: EventService,
        public elementRef: ElementRef,
        public injector: Injector,
        protected routeHelper: RouteHelper,
        private sharedAlertService: SharedAlertService,
        protected navProxy: NavProxyService,
        private formBuilder: FormBuilder,
        private dataTableDefinitionApiService: DataTableDefinitionApiService,
        private dataTableContentApiService: DataTableContentApiService,
        private csvValidatorService: CsvValidatorService,
        private sharedLoaderService: SharedLoaderService,
        private tenantService: TenantService,
        private dataTableDefinitionService: DataTableDefinitionService,
    ) {
        super(routeHelper, navProxy, eventService, elementRef, injector);
    }

    public async ngAfterViewInit(): Promise<void> {
        this.dataTableDefinitionId = this.routeHelper.getParam("dataTableDefinitionId");
        this.tenantAlias = this.routeHelper.getParam('tenantAlias')
            || this.routeHelper.getParam('portalTenantAlias');

        const dataTableDefinition$: Observable<DataTableDefinitionResourceModel> =
            this.dataTableDefinitionApiService.getDataTableDefinitionById(this.tenantAlias, this.dataTableDefinitionId);
        const dataTableContent$: Observable<HttpResponse<Blob>> =
            this.dataTableContentApiService.downloadDataTableContentCsv(this.tenantAlias, this.dataTableDefinitionId);

        this.isLoading = true;
        forkJoin([dataTableDefinition$, dataTableContent$])
            .pipe(finalize(() => this.isLoading = false))
            .subscribe({
                next: async ([dataTableDefinition, dataTableContent]:
                    [DataTableDefinitionResourceModel, HttpResponse<Blob>]) => {
                    await this.readHttpResponse(dataTableDefinition, dataTableContent);
                },
                error: (error: any) => {
                    this.errorMessage = "There was an error loading the data table details";
                },
            },
            );
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.form.dirty) {
            await this.sharedAlertService.showWithActionHandler({
                header: 'Unsaved Changes',
                subHeader: 'You have unsaved changes, are you sure you wish to close the current view without saving'
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

    public buildForm(dataTable: DataTableDefinitionResourceModel, csvData: string): void {
        let formValue: any = {
            ...{ csvData: csvData },
            ...dataTable,
            tableSchemaJson: JSON.stringify(dataTable.tableSchema, this.jsonStringifyReplacer, 2),
        };
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        this.form = this.formBuilder.group(controls);
        this.setFormBehavior();
        this.form.patchValue(formValue);
    }

    public async userDidTapSaveButton(): Promise<void> {
        await this.sharedLoaderService.presentWithDelay("Updating data table...");
        const formItemAlias: { [key: string]: string } = DataTableDefinitionViewModel.dataTableFormItemAlias;
        let tenantId: string = this.tenantAlias
            ? await this.tenantService.getTenantIdFromAlias(this.tenantAlias) : '';
        let data: DataTableCreateFromCsvModel = {
            definitionId: this.dataTable.id,
            name: this.form.get(formItemAlias.name).value,
            alias: this.form.get(formItemAlias.alias).value,
            tableSchema: JSON.parse(this.form.get(formItemAlias.tableSchemaJson).value),
            csvData: this.form.get(formItemAlias.csvData).value,
            memoryCachingEnabled: this.form.get(formItemAlias.memoryCachingEnabled).value,
            cacheExpiryInSeconds: this.form.get(formItemAlias.cacheExpiryInSeconds).value,
        };
        this.dataTableDefinitionApiService.updateDataTableFromCsvData(tenantId, data)
            .pipe(
                catchError((error: any) => {
                    this.dataTableDefinitionService.highlightCsvError(this.form, error);
                    return throwError(error);
                }),
                finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe((result: DataTableDefinitionResourceModel) => {
                this.eventService.getEntityUpdatedSubject('DataTableDefinition').next(result);
                this.sharedAlertService.showToast(
                    `${this.form.get("name").value} data table was updated`,
                );
                this.navigateToDetail(result.id);
            });
    }

    private jsonStringifyReplacer(key: string, value: any): any {
        if (value === null) {
            return undefined;
        }
        return value;
    }

    private readHttpResponse(
        dataTableDefinition: DataTableDefinitionResourceModel,
        dataTableContent: HttpResponse<Blob>,
    ): Promise<void> {
        return new Promise((resolve: any) => {
            const reader: FileReader = new FileReader();
            reader.readAsText(dataTableContent.body);
            reader.onload = async (ev: ProgressEvent): Promise<void> => {
                const csvText: string = <string>reader.result;
                this.dataTable = new DataTableDefinitionViewModel(dataTableDefinition);
                this.detailList = DataTableDefinitionViewModel
                    .createFormDetailsList(this.csvValidatorService.csvValidator({ size: null }),
                        await this.dataTableDefinitionService.jsonTableSchemaValidator());
                this.buildForm(dataTableDefinition, csvText);
                resolve();
            };
        });
    }

    private async navigateToDetail(dataTableId: string): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop(); // remove 'edit' from url
        await this.navProxy.navigate(pathSegments);
    }
}
