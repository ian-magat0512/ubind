import { Component, Injector, ElementRef, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ReportApiService } from '@app/services/api/report-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PopoverController } from '@ionic/angular';
import { saveAs } from 'file-saver';
import moment from 'moment';
import {
    ReportCreateModel, ReportResourceModel, ReportFileResourceModel,
} from '@app/resource-models/report.resource-model';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { contentAnimation } from '@assets/animations';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { ReportDetailViewModel } from '@app/viewmodels/report-detail.viewmodel';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers';
import { Observable, Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { AuthenticationService } from '@app/services/authentication.service';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { EntityType } from '@app/models/entity-type.enum';
import { ActionButton } from '@app/models/action-button';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { ReportFileViewModel } from '@app/viewmodels/report-file.viewmodel';
import {
    EntityDetailSegmentListComponent,
} from '@app/components/entity-detail-segment-list/entity-detail-segment-list.component';

/**
 * Export detail report page component class.
 * This class manage displaying the report details.
 */
@Component({
    selector: 'app-detail-report',
    templateUrl: './detail-report.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-report.page.scss',
        '../../../../assets/css/scrollbar-detail.css'],
    styles: [scrollbarStyle],
})
export class DetailReportPage extends DetailPage implements OnInit, OnDestroy {

    @ViewChild(EntityDetailSegmentListComponent)
    public reportFileListComponent: EntityDetailSegmentListComponent;
    protected reportId: string;
    public report: ReportDetailViewModel;
    public permission: typeof Permission = Permission;
    public reportProducts: string;
    public segment: string;
    public moment: any = moment;
    public title: string = 'Report';
    public detailsListItems: Array<DetailsListItem>;
    public actions: Array<ActionButtonPopover> = [];
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    private entityTypes: typeof EntityType = EntityType;
    public hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public reportFileViewModel: typeof ReportFileViewModel = ReportFileViewModel;

    private reportActions: any = {
        generate: "Generate Report",
        edit: "Edit Report",
        delete: "Delete Report",
    };

    public constructor(
        protected route: ActivatedRoute,
        protected router: Router,
        protected reportApiService: ReportApiService,
        public navProxy: NavProxyService,
        protected popoverCtrl: PopoverController,
        protected sharedAlertService: SharedAlertService,
        public layoutManager: LayoutManagerService,
        protected eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private sharedPopoverService: SharedPopoverService,
        private authService: AuthenticationService,
        private portalExtensionService: PortalExtensionsService,
        private permissionService: PermissionService,
    ) {
        super(eventService, elementRef, injector);
        this.segment = this.route.snapshot.queryParamMap.get('previous') || 'History';

        this.eventService.getEntityUpdatedSubject('Report').subscribe((model: ReportResourceModel) => {
            if (model.id == this.report.id && !model.isDeleted) {
                this.load();
            }
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.load();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private load(): void {
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        this.reportId = this.route.snapshot.paramMap.get('reportId');
        this.loadDetails();
    }

    private async loadDetails(): Promise<void> {
        this.isLoading = true;
        return this.reportApiService.getById(this.reportId)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
            )
            .toPromise()
            .then((report: ReportResourceModel) => {
                this.report = new ReportDetailViewModel(report);
                this.initializeDetailsListItems();
                this.title = report.name;
                this.reportProducts = this.report.products.map((p: ProductResourceModel) => p.name).join(',');
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the report details';
                throw err;
            });
    }

    public getSegmentHistoryList(
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<ReportFileResourceModel>> {
        return this.reportApiService.getReportFileList(this.reportId, params);
    }

    public refreshHistoryList(): void {
        this.reportFileListComponent?.reload();
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.report.createDetailsList();
    }

    public closeButtonClicked(): void {
        this.returnToPrevious();
    }

    public editButtonClicked(): void {
        this.navProxy.navigateForward(['report', this.reportId, 'edit']);
    }

    public generateButtonClicked(): void {
        this.navProxy.navigateForward(['report', this.reportId, 'generate']);
    }

    public showModalButtonClicked(event: any): void {
        this.presentPopover(event);
    }

    public reportFileClicked(reportFile: ReportFileViewModel): void {
        this.reportApiService.getReportFile(this.report.id, reportFile.id)
            .pipe(takeUntil(this.destroyed))
            .subscribe(
                (blob: any) => {
                    saveAs(blob, reportFile.filename);
                },
            );
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        }
    }

    public formatBytes(bytes: any, decimals: number = 2): string {
        if (bytes === 0) {
            return '0 B';
        }

        const k: number = 1024;
        const dm: number = decimals < 0 ? 0 : decimals;
        const sizes: Array<string> = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

        const i: number = Math.floor(Math.log(bytes) / Math.log(k));

        return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Claim,
                PageType.Display,
                this.segment,
            );
    }

    private generatePopoverLinks(): void {
        this.actions = [];
        this.actions.push({
            actionName: this.reportActions.generate,
            actionIcon: "play-circle",
            iconLibrary: IconLibrary.IonicV4,
            actionButtonLabel: "",
            actionButtonPrimary: false,
            includeInMenu: true,
        });
        if (this.segment == "Details") {
            this.actions.push({
                actionName: this.reportActions.edit,
                actionIcon: "pencil",
                iconLibrary: IconLibrary.AngularMaterial,
                actionButtonLabel: "",
                actionButtonPrimary: false,
                includeInMenu: true,
            });
            this.actions.push({
                actionName: this.reportActions.delete,
                actionIcon: "trash",
                iconLibrary: IconLibrary.IonicV4,
                actionButtonLabel: "",
                actionButtonPrimary: false,
                includeInMenu: true,
            });
        }
        const actionButtons: Array<ActionButtonPopover> =
            this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.actions = this.actions.concat(actionButtons);
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    private async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                switch (command.data.action.actionName) {
                    case this.reportActions.generate:
                        this.generateButtonClicked();
                        break;
                    case this.reportActions.edit:
                        this.editButtonClicked();
                        break;
                    case this.reportActions.delete:
                        this.deleteReport();
                        break;
                    default:
                        if (command.data.action.portalPageTrigger) {
                            this.portalExtensionService.executePortalPageTrigger(
                                command.data.action.portalPageTrigger,
                                this.entityTypes.Report,
                                PageType.Display,
                                this.segment,
                                this.reportId,
                            );
                        }
                        break;
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                componentProps: {
                    actions: this.actions,
                },
                event,
            },
            'Report option popover',
            popoverDismissAction,
        );
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Report,
            PageType.Display,
            this.segment,
            this.reportId,
        );
    }

    private returnToPrevious(): void {
        this.navProxy.navigateBack(['report', 'list']);
    }

    protected async deleteReport(): Promise<void> {
        await this.sharedAlertService.showWithActionHandler({
            header: 'Delete Report',
            subHeader: 'By deleting this report all associated saved generated reports will no longer be accessible. '
                + 'Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'Yes',
                    role: 'yes',
                    handler: (): any => {
                        this.report.isDeleted = true;

                        const reportUpdateResourceModel: ReportCreateModel = {
                            id: this.report.id,
                            tenantId: this.report.tenantId,
                            name: this.report.name,
                            description: this.report.description,
                            createdDateTime: this.report.createdDateTime,
                            productIds: this.report.products.map((v: ProductResourceModel, i: number) => v.id),
                            sourceData: this.report.sourceData,
                            mimeType: this.report.mimeType,
                            filename: this.report.filename,
                            body: this.report.body,
                            isDeleted: this.report.isDeleted,
                        };

                        this.reportApiService.update(this.report.id, reportUpdateResourceModel)
                            .pipe(takeUntil(this.destroyed))
                            .subscribe(
                                (report: ReportResourceModel) => {
                                    this.eventService.getEntityDeletedSubject('Report').next(report);
                                    this.sharedAlertService.showToast(`${report.name} report was deleted`);
                                    this.returnToPrevious();
                                },
                            );
                    },
                },
                {
                    text: 'No',
                    role: 'no',
                },
            ],
        });
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (this.segment === 'Details' && this.permissionService.hasPermission(Permission.ManageReports)) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Report",
                true,
                (): void => {
                    return this.editButtonClicked();
                },
            ));
        } else {
            actionButtonList.push(ActionButton.createActionButton(
                "Refresh",
                "refresh",
                IconLibrary.IonicV4,
                false,
                "Refresh List",
                true,
                (): void => {
                    return this.refreshHistoryList();
                },
                this.segment != 'Details' ? 2 : null,
            ));
        }

        actionButtonList.push(ActionButton.createActionButton(
            "Generate",
            "play-circle",
            IconLibrary.IonicV4,
            false,
            "Generate Report",
            true,
            (): void => {
                return this.generateButtonClicked();
            },
            this.segment != 'Details' ? 1 : null,
        ));

        for (let action of this.actions) {
            if (action.actionButtonLabel) {
                actionButtonList.push(ActionButton.createActionButton(
                    action.actionButtonLabel ? action.actionButtonLabel : action.actionName,
                    action.actionIcon,
                    IconLibrary.IonicV4,
                    action.actionButtonPrimary,
                    action.actionName,
                    action.actionButtonLabel ? true : false,
                    (): void => {
                        this.portalExtensionService.executePortalPageTrigger(
                            action.portalPageTrigger,
                            this.entityTypes.Report,
                            PageType.Display,
                            this.segment,
                            this.reportId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
