import { AfterViewInit, Component, ElementRef, Injector } from '@angular/core';
import { Permission } from '@app/helpers';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { contentAnimation } from '@assets/animations';
import { DataTableDefinitionApiService } from '@app/services/api/data-table-definition-api.service';
import { EntityType } from '@app/models/entity-type.enum';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { finalize } from 'rxjs/operators';
import { DataTableDefinitionResourceModel } from '@app/resource-models/data-table-definition.resource-model';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PermissionService } from '@app/services/permission.service';

/**
 * Page class for data table definition list to be displayed 
 * in the associated entity detail pane.
 */
@Component({
    selector: 'app-data-table-definition-detail-list',
    templateUrl: './data-table-definition-list-detail.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class DataTableDefinitionListDetailPage extends DetailPage implements AfterViewInit {

    public dataTables: Array<DataTableDefinitionResourceModel>;
    public permission: typeof Permission = Permission;
    public actionButtonList: Array<ActionButton>;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected eventService: EventService,
        public elementRef: ElementRef,
        public injector: Injector,
        public layoutManager: LayoutManagerService,
        private routeHelper: RouteHelper,
        private navProxy: NavProxyService,
        private dataTableDefinitionApiService: DataTableDefinitionApiService,
        private tenantService: TenantService,
        private productService: ProductService,
        private permissionService: PermissionService,
    ) {
        super(eventService, elementRef, injector);
    }

    public async ngAfterViewInit(): Promise<void> {
        await this.loadItems();
        this.initializeActionButtonList();
    }

    public userDidTapBackButton(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop(); // remove 'list-detail' from url
        pathSegments.pop(); // remove 'data-table' from url
        let params: any = {
            "segment": 'Settings',
        };
        this.navProxy.navigateBack(pathSegments, true, { queryParams: params });
    }

    public createDataTable(): void {
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push('create');
        this.navProxy.navigate(pathSegments);
    }

    public itemSelected(dataTableDefinition: DataTableDefinitionResourceModel): void {
        if (dataTableDefinition) {
            const pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
            pathSegments.push(dataTableDefinition.id);
            this.navProxy.navigate(pathSegments);
        }
    }

    private initializeActionButtonList(): void {
        this.actionButtonList = [];

        if (this.permissionService.hasPermission(Permission.ManageDataTables)) {
            this.actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Data Table",
                true,
                (): void => {
                    return this.createDataTable();
                },
            ));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(this.actionButtonList);
    }

    private async loadItems(): Promise<void> {
        this.isLoading = true;
        const tenantAlias: string = this.routeHelper.getContextTenantAlias();
        const tenantId: string =
            tenantAlias ? await this.tenantService.getTenantIdFromAlias(tenantAlias) : '';
        const productAlias: string = this.routeHelper.getParam('productAlias');
        const productId: string =
            productAlias ? await this.productService.getProductIdFromAlias(tenantAlias, productAlias) : '';
        const organisationId: string = this.routeHelper.getParam('organisationId');

        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenantId);

        params = productAlias
            ? params.set("entityType", EntityType.Product).set("entityId", productId)
            : organisationId
                ? params.set("entityType", EntityType.Organisation).set("entityId", organisationId)
                : params.set("entityType", EntityType.Tenant).set("entityId", tenantId);

        this.dataTableDefinitionApiService
            .getDataTableDefinitions(params)
            .pipe(finalize(() => this.isLoading = false))
            .subscribe((dataTables: Array<DataTableDefinitionResourceModel>) => {
                this.dataTables = dataTables;
            });
    }
}
