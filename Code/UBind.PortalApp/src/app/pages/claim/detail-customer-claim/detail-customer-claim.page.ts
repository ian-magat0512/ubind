import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ToastController, AlertController, PopoverController } from '@ionic/angular';
import { Subject } from 'rxjs';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { ClaimDetailViewModel } from '@app/viewmodels';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers/permissions.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { ClaimHelper } from '@app/helpers/claim.helper';
import { ClaimVersionListResourceModel, ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { takeUntil, finalize } from 'rxjs/operators';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { QuestionViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { AuthenticationService } from '@app/services/authentication.service';
import { Subscription } from 'rxjs';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { saveAs } from 'file-saver';
import { EntityType } from "@app/models/entity-type.enum";
import { PermissionService } from '@app/services/permission.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';

/**
 * Export detail customer claim page component class
 * TODO: Write a better class header: customer detials claims.
 */
@Component({
    selector: 'app-detail-customer-claim',
    templateUrl: './detail-customer-claim.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class DetailCustomerClaimPage extends DetailPage implements OnInit, OnDestroy {

    public model: ClaimDetailViewModel;
    public segment: string;
    public permission: typeof Permission = Permission;
    public displayFields: any = {};
    public isLoading: boolean = true;

    public isShowUpdateClaim: boolean;
    public versionsList: Array<ClaimVersionListResourceModel> = null;
    private rawData: ClaimResourceModel;
    private moreOptions: Array<ActionButtonPopover>;
    public displayType: string = QuestionViewModelGenerator.type.Claim;

    public questionItems: Array<QuestionViewModel>;
    public repeatingQuestionItems: Array<RepeatingQuestionViewModel>;

    public documentHeaders: Array<string> = [];

    public detailsListItems: Array<DetailsListItem>;

    private downloading: any = {};
    private downloadClaimDocumentSubscription: Subscription;
    public entityTypes: typeof EntityType = EntityType;
    public claimId: string;
    private isMutual: boolean;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        private route: ActivatedRoute,
        private popOverCtrl: PopoverController,
        private toastCtrl: ToastController,
        private claimApiService: ClaimApiService,
        private alertCtrl: AlertController,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        eventService: EventService,
        private sharedPopoverService: SharedPopoverService,
        private elementRef: ElementRef,
        public injector: Injector,
        private authService: AuthenticationService,
        protected documentApiService: DocumentApiService,
        private permissionService: PermissionService,
        private routeHelper: RouteHelper,
        private appConfigService: AppConfigService,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.isMutual = appConfig.portal.isMutual;
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.claimId = this.routeHelper.getParam('claimId');
        this.segment = this.routeHelper.getParam('segment') || 'Details';
        this.load();
        this.loadVersions();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public update(): void {
        this.navProxy.navigate(
            [this.userPath.claim, this.model.id, 'update'],
            {
                queryParams: {
                    productAlias: this.model.productId,
                },
            },
        );
    }

    public gotoVersionDetail(versionItem: ClaimVersionListResourceModel): void {
        const params: any = { queryParams: { selectedId: versionItem.claimId } };
        this.navProxy.navigateForward(
            [
                this.userPath.claim,
                this.claimId,
                'version',
                versionItem.claimVersionId],
            true,
            params,
        );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (data: any): void => {
            this.flipMoreIcon = false;
            if (data
                && data.data
                && data.data.action
                && this.moreOptions.filter((x: ActionButtonPopover) =>
                    x.actionName.includes(data.data.action))) {
                switch (data.data.action) {
                    case 'Resume Claim':
                        this.update();
                        break;
                    case 'Withdraw Claim':
                        this.confirmClaimWithdrawal();
                        break;
                    case 'Edit Claim Properties':
                        this.navProxy.goToAdditionalPropertyValues(EntityType.Claim);
                        break;
                    default:
                        break;
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                componentProps: { actions: this.moreOptions },
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
            },
            'Customer claim option popover',
            popoverDismissAction,
        );

    }

    public handleSegmentClick($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
        }
        switch (this.segment) {
            case 'Details':
                this.load();
                break;
            case 'Versions':
                this.loadVersions();
                break;
            default: break;
        }
    }

    public goBackOrClose(): void {
        let claimTab: string = ClaimHelper.getTab(this.model.status, this.authService.isCustomer()).toLowerCase();
        this.navProxy.navigate([this.userPath.claim], { queryParams: { segment: claimTab } });
    }

    public download(documentId: string, fileName: string): void {
        if (this.downloading[documentId]) {
            return;
        }

        this.downloading[documentId] = true;
        this.downloadClaimDocumentSubscription = this.documentApiService
            .downloadClaimDocument(documentId, this.model.id)
            .pipe(
                finalize(() => this.downloading[documentId] = false),
            )
            .subscribe(
                (blob: any) => {
                    saveAs(blob, fileName);
                },
            );
    }

    private load(): void {
        this.isLoading = true;
        this.model = null;
        const id: string = this.route.snapshot.paramMap.get('claimId');
        this.claimApiService.getById(id)
            .pipe(takeUntil(this.destroyed), finalize(() => this.isLoading = false))
            .subscribe(
                (dt: ClaimResourceModel) => {
                    this.rawData = dt;
                    this.model = new ClaimDetailViewModel(dt);
                    this.documentHeaders = Array.from(
                        new Set(this.model.documents.map((item: any) => item.dateGroupHeader)),
                    );
                    this.initializeDetailsListItems();
                    this.displayFields = dt.displayableFieldsModel;
                    this.moreOptions = [];

                    this.repeatingQuestionItems = QuestionViewModelGenerator.getRepeatingData(
                        this.model.formData,
                        dt.displayableFieldsModel,
                        dt.questionAttachmentKeys,
                    );
                    this.questionItems = QuestionViewModelGenerator.getFormDataQuestionItems(
                        this.model.formData,
                        this.model.questionAttachmentKeys,
                        dt.displayableFieldsModel,
                    );
                    this.isShowUpdateClaim = ClaimHelper.canShowUpdateButton(this.model.status);

                    if (ClaimHelper.status.Incomplete.toLowerCase() == this.model.status.toLowerCase()
                        && this.authService.isCustomer()) {
                        this.moreOptions.push({
                            actionName: 'Resume Claim',
                            actionIcon: "resume-quote",
                            iconLibrary: IconLibrary.AngularMaterial,
                            actionButtonLabel: "",
                            actionButtonPrimary: false,
                            includeInMenu: true,
                        });
                    }

                    if (ClaimHelper.status.Inactive.findIndex((x: string) =>
                        x.toLowerCase() == this.model.status.toLowerCase()) < 0) {
                        this.moreOptions.push({
                            actionName: 'Withdraw Claim',
                            actionIcon: "arrow-u-left-bottom-bold",
                            iconLibrary: IconLibrary.AngularMaterial,
                            actionButtonLabel: "",
                            actionButtonPrimary: false,
                            includeInMenu: true,
                        });
                    }

                    if (dt.additionalPropertyValues && dt.additionalPropertyValues.length > 0
                        && this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues)) {
                        this.moreOptions.push({
                            actionName: "Edit Claim Properties",
                            actionIcon: "brush",
                            iconLibrary: IconLibrary.IonicV4,
                            actionButtonLabel: "",
                            actionButtonPrimary: false,
                            includeInMenu: true,
                        });
                    }

                    this.initializeActionButtonList();
                },
            );
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.model.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.isMutual,
            this.authService.tenantAlias,
        );
    }

    private loadVersions(): void {
        this.isLoading = true;
        this.versionsList = null;
        const claimId: string = this.route.snapshot.paramMap.get('claimId');
        this.claimApiService.getClaimVersions(claimId)
            .pipe(takeUntil(this.destroyed), finalize(() => this.isLoading = false))
            .subscribe((dt: Array<ClaimVersionListResourceModel>) => {
                this.versionsList = dt;
            });
    }

    private async confirmClaimWithdrawal(): Promise<void> {
        const alert: HTMLIonAlertElement = await this.alertCtrl.create({
            id: 'claim-withdraw',
            header: 'Withdraw Claim',
            message: 'Are you sure you wish to withdraw this claim?',
            buttons: [{
                text: 'Cancel',
                role: 'cancel',
            },
            {
                text: 'Withdraw',
                handler: (): any => {
                    this.claimApiService.withdrawClaim(this.rawData.id)
                        .pipe(takeUntil(this.destroyed))
                        .subscribe((data: ClaimResourceModel) => {
                            return this.showWithdrawSnackbar().then(() => {
                                this.eventService.getEntityUpdatedSubject('Claim').next(data);
                                this.load();
                            });
                        });
                },
            }],
        });

        await alert.present();
    }

    private async showWithdrawSnackbar(): Promise<void> {
        const snackbar: HTMLIonToastElement = await this.toastCtrl.create({
            id: this.model.id,
            message: 'Claim successfully withdrawn',
            duration: 3000,
        });

        return await snackbar.present();
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (ClaimHelper.status.Inactive.findIndex((x: string) =>
            x.toLowerCase() == this.model.status.toLowerCase()) < 0) {
            actionButtonList.push(ActionButton.createActionButton(
                "Withdraw",
                "arrow-u-left-bottom-bold",
                IconLibrary.AngularMaterial,
                false,
                "Withdraw Claim",
                true,
                (): Promise<void> => {
                    return this.confirmClaimWithdrawal();
                },
            ));
        }

        let resume: boolean = this.model.status.toLowerCase() == ClaimHelper.status.Incomplete.toLowerCase()
            && this.authService.isCustomer();
        if (this.isShowUpdateClaim) {
            actionButtonList.push(ActionButton.createActionButton(
                "Resume",
                "resume-quote",
                IconLibrary.AngularMaterial,
                resume,
                "Resume Claim",
                true,
                (): void => {
                    return this.update();
                },
            ));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
