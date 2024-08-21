import { AfterViewInit, Component, ElementRef, Injector } from '@angular/core';
import { getFilenameFromContentDisposition, Permission } from '@app/helpers';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { DataTableDefinitionViewModel } from "@app/viewmodels/data-table-definition.viewmodel";
import { contentAnimation } from '@assets/animations';
import { finalize } from 'rxjs/operators';
import { PopoverDataTableComponent } from '@pages/data-table/popover-data-table/popover-data-table.component';
import { DataTableDefinitionApiService } from '@app/services/api/data-table-definition-api.service';
import { DataTableDefinitionResourceModel } from '@app/resource-models/data-table-definition.resource-model';
import { DataTableContentApiService } from '@app/services/api/data-table-content-api.service';
import { saveAs } from 'file-saver';
import { HttpResponse } from '@angular/common/http';
import { ActionButton } from '@app/models/action-button';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';

/**
 * Component for data table detail page.
 */
@Component({
    selector: 'app-detail-data-table',
    templateUrl: './detail-data-table-definition.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class DetailDataTableDefinitionPage extends DetailPage implements AfterViewInit {

    public dataTableDefinition: DataTableDefinitionResourceModel;
    public dataTableViewModel: DataTableDefinitionViewModel;
    public detailsListItems: Array<DetailsListItem>;
    public isLoading: boolean;
    public permission: typeof Permission = Permission;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;

    private dataTableDefinitionId: string;
    private tenantAlias: string;
    private productAlias: string;
    private organisationId: string;
    private organisationAlias: string;

    public constructor(
        public layoutManager: LayoutManagerService,
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        public sharedPopoverService: SharedPopoverService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private sharedLoaderService: SharedLoaderService,
        private dataTableDefinitionApiService: DataTableDefinitionApiService,
        private dataTableContentApiService: DataTableContentApiService,
        private permissionService: PermissionService,
        private sharedAlertService: SharedAlertService,
    ) {
        super(eventService, elementRef, injector);
    }

    public async ngAfterViewInit(): Promise<void> {
        this.dataTableDefinitionId = this.routeHelper.getParam("dataTableDefinitionId");
        this.tenantAlias = this.routeHelper.getParam('tenantAlias')
            || this.routeHelper.getParam('portalTenantAlias');

        await this.loadDetails();
        this.initializeActionButtonList();
    }

    public async goBack(): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop(); // remove `definitionId` from the url.
        pathSegments.push('list-detail');
        await this.navProxy.navigate(pathSegments);
    }

    public async download(): Promise<void> {
        try {
            await this.sharedLoaderService.present("Preparing table data for download...");
            const response: HttpResponse<Blob> = await this.dataTableContentApiService
                .downloadDataTableContentCsv(this.tenantAlias, this.dataTableDefinition.id)
                .toPromise();
            const blobFilename: string = getFilenameFromContentDisposition(
                response.headers.get('content-disposition'),
            );
            saveAs(response.body, blobFilename);
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }

    public edit(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push("edit");
        this.navProxy.navigateForward(pathSegments, true);
    }

    public delete(): void {
        this.sharedLoaderService.present("Deleting data table...")
            .then((result: void) => {
                this.dataTableDefinitionApiService
                    .deleteDataTableDefinition(this.tenantAlias, this.dataTableDefinitionId)
                    .pipe(finalize(() => this.sharedLoaderService.dismiss()))
                    .subscribe(() => {
                        this.eventService.getEntityDeletedSubject('DataTableDefinition')
                            .next(this.dataTableDefinition);
                        this.sharedAlertService.showToast(
                            `${this.dataTableDefinition.name} data table was deleted`,
                        );
                        this.goBack();
                    });
            });
    }

    public async showMenu(): Promise<void> {
        this.flipMoreIcon = true;

        const popoverDismissAction = async (command: any): Promise<any> => {
            this.flipMoreIcon = false;
            if (command.data && command.data.action) {
                if (command.data.action === "download") {
                    this.download();
                }
                if (command.data.action === "edit") {
                    this.edit();
                }
                if (command.data.action === "delete") {
                    this.delete();
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverDataTableComponent,
                componentProps: {},
                cssClass: 'custom-popover',
                event: event,
            },
            'Data table option popover',
            popoverDismissAction,
        );
    }

    private initializeActionButtonList(): void {
        this.actionButtonList = [];

        this.actionButtonList.push(ActionButton.createActionButton(
            "Download",
            "download",
            IconLibrary.IonicV4,
            false,
            "Download table data as CSV",
            true,
            async (): Promise<void> => {
                return await this.download();
            },
        ));

        if (this.permissionService.hasPermission(Permission.ManageDataTables)) {
            this.actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Data Table",
                true,
                (): void => {
                    return this.edit();
                },
            ));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(this.actionButtonList);
    }

    private async loadDetails(): Promise<void> {
        this.isLoading = true;

        this.dataTableDefinitionId = this.routeHelper.getParam("dataTableDefinitionId");
        this.dataTableDefinitionApiService.getDataTableDefinitionById(this.tenantAlias, this.dataTableDefinitionId)
            .pipe(finalize(() => this.isLoading = false))
            .subscribe((dataTable: DataTableDefinitionResourceModel) => {
                this.dataTableDefinition = dataTable;
                this.dataTableViewModel = new DataTableDefinitionViewModel(dataTable);
                this.detailsListItems = this.dataTableViewModel.dataTableDetailList();
            });
    }
}
