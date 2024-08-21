import { DetailPage } from "@app/pages/master-detail/detail.page";
import { AfterViewInit, Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { EventService } from "@app/services/event.service";
import { PersonApiService } from "@app/services/api/person-api.service";
import { RouteHelper } from "@app/helpers/route.helper";
import { finalize, takeUntil, mergeMap, map } from "rxjs/operators";
import { PersonResourceModel, UserStatus } from "@app/models";
import { PersonViewModel } from "@app/viewmodels";
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { PersonService } from "@app/services/person.service";
import { Subject, Subscription } from "rxjs";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { CustomerApiService } from "@app/services/api/customer-api.service";
import { CustomerResourceModel } from "@app/resource-models/customer.resource-model";
import { AuthenticationService } from "@app/services/authentication.service";
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { EntityType } from "@app/models/entity-type.enum";
import { ActionButtonPopover } from "@app/models/action-button-popover";
import { ActionButton } from "@app/models/action-button";
import { IconLibrary } from "@app/models/icon-library.enum";
import { ActionButtonHelper } from "@app/helpers/action-button.helper";
import { PopoverPersonComponent } from "@app/components/popover-person/popover-person.component";
import { PopoverOptions } from "@ionic/core";
import { SharedPopoverService } from "@app/services/shared-popover.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { PageType } from "@app/models/page-type.enum";
import { PopoverCommand } from "@app/models/popover-command";

/**
 * Export detail person page component class
 */
@Component({
    selector: 'app-detail-person',
    templateUrl: './detail-person.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class DetailPersonPage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {
    private personId: string;
    private isPrimaryPerson: boolean = false;
    public model: PersonViewModel;
    public personDetailsListItems: Array<DetailsListItem>;
    private customerId: string;
    private customerDisplayName: string;
    public entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    public actionButtonList: Array<ActionButton>;
    public personResourceModel: PersonResourceModel;
    public flipMoreIcon: boolean = false;

    public constructor(
        protected eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        public layoutManager: LayoutManagerService,
        private personApiService: PersonApiService,
        private customerApiService: CustomerApiService,
        private routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        private personService: PersonService,
        protected userPath: UserTypePathHelper,
        private authService: AuthenticationService,
        private portalExtensionService: PortalExtensionsService,
        private sharedPopoverService: SharedPopoverService,
        private sharedLoaderService: SharedLoaderService,
    ) {
        super(eventService, elementRef, injector);
        this.isLoading = false;
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.listenForPersonUpdates();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public ngAfterViewInit(): void {
        this.personId = this.routeHelper.getParam('personId');
        this.customerId = this.routeHelper.getParam('customerId');
        this.loadDetails();
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    private listenForPersonUpdates(): void {
        this.eventService.getEntityUpdatedSubject('Person')
            .pipe(takeUntil(this.destroyed))
            .subscribe((data: any) => {
                let fromEvent: string = data['fromEvent'];
                if (fromEvent && fromEvent === 'deleted') {
                    this.goBack();
                } else {
                    this.ngAfterViewInit();
                }
            });
    }

    private loadDetails(): Promise<void> {
        this.isLoading = true;
        return new Promise(async (resolve: any, _reject: any): Promise<void> => {
            if (this.customerId) {
                let customerResourceModel: CustomerResourceModel =
                    await this.customerApiService.getById(this.customerId).toPromise();
                if (customerResourceModel) {
                    this.customerDisplayName = customerResourceModel.displayName;
                }
            }

            let subscription: Subscription = this.personApiService.getById(this.personId)
                .pipe(
                    map((personResourceModel: PersonResourceModel) => {
                        this.model = new PersonViewModel(personResourceModel);
                        this.personResourceModel = personResourceModel;
                        this.personDetailsListItems = this.model.createDetailsList(this);
                        return personResourceModel;
                    }),
                    mergeMap((personResourceModel: PersonResourceModel) =>
                        this.customerApiService.getPrimaryPerson(personResourceModel.customerId)),
                    finalize(() => {
                        subscription.unsubscribe();
                        this.isLoading = false;
                    }),
                )
                .subscribe((primaryPersonData: PersonResourceModel) => {
                    this.isPrimaryPerson = primaryPersonData.id == this.model.id;
                    resolve();
                });
        });
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Person,
                PageType.Display,
                null,
            );
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        this.actions = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Person,
            PageType.Display,
            null,
            this.personId,
        );
    }

    public goBack(): void {
        if (this.model) {
            this.navProxy
                .navigate([this.userPath.customer, this.model.customerId], { queryParams: { segment: "People" } });
        }
    }

    public async didSelectEdit(): Promise<void> {
        this.navProxy
            .navigate([this.userPath.customer, this.model.customerId, this.userPath.person, this.model.id, 'edit']);
    }

    public async showPersonPopover(event: any): Promise<void> {
        let email: string = this.model.email;
        if (this.model.personRepeatingFields.emailAddresses.some((e: any) => e.default)) {
            email = this.model.personRepeatingFields.emailAddresses[0].emailAddress;
        }

        const personResourceModelParam: PersonResourceModel = {
            id: this.model.id,
            customerId: this.model.customerId,
            email: email,
            userStatus: this.model.status,
            fullName: this.model.fullName,
            tenantId: this.model.tenantId,
            blocked: false,
            organisationId: null,
            organisationName: null,
            environment: null,
            preferredName: this.model.preferredName,
            namePrefix: this.model.namePrefix,
            firstName: this.model.firstName,
            middleNames: this.model.middleNames,
            lastName: this.model.lastName,
            nameSuffix: this.model.nameSuffix,
            company: this.model.company,
            title: this.model.title,
            displayName: this.model.fullName,
            hasActivePolicies: this.model.hasActivePolicies,
            picture: null,
            userType: this.model.userType,
            ownerId: this.model.ownerId,
            ownerFullName: this.model.ownerFullName,
            pictureId: this.model.pictureId,
            createdDateTime: this.model.createdDate,
            lastModifiedDateTime: this.model.lastModifiedDate,
            emailAddresses: null,
            phoneNumbers: null,
            streetAddresses: null,
            websiteAddresses: null,
            messengerIds: null,
            socialMediaIds: null,
            properties: null,
        };

        this.showMenu(event, personResourceModelParam, this.isPrimaryPerson, this.actions);
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];

        actionButtonList.push(ActionButton.createActionButton(
            "Edit",
            "pencil",
            IconLibrary.AngularMaterial,
            false,
            "Edit Person",
            true,
            (): Promise<void> => {
                return this.didSelectEdit();
            },
        ));

        actionButtonList.push(ActionButton.createActionButton(
            "Delete",
            "trash",
            IconLibrary.IonicV4,
            false,
            "Delete Person",
            true,
            (): Promise<void> => {
                return this.personService.delete(this.model.id, this.personResourceModel, ['Person']);
            },
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
                    (): Promise<void> => {
                        return this.portalExtensionService.executePortalPageTrigger(
                            action.portalPageTrigger,
                            this.entityTypes.Person,
                            PageType.Display,
                            null,
                            this.model.id,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private async showMenu(
        event: any,
        model: PersonResourceModel,
        isPrimaryPerson: boolean,
        actions: Array<ActionButtonPopover>,
    ): Promise<void> {
        if (!model) {
            return;
        }
        this.flipMoreIcon = true;
        const id: string = model.id;
        const name: string = model.fullName;
        const email: string = model.email;
        const status: string = model.userStatus;
        const popoverActions: any = {
            activate: {
                condition: async (): Promise<any> => await this.personService.isValidEmailAddress(email)
                    && await this.personService.confirmCreateUserAccount(email, name),
                action: (): Promise<void> => this.personService.createUserAccount(id, name, ['Person']),
            },
            resendActivate: {
                condition: async (): Promise<any> => await this.personService.isValidEmailAddress(email),
                action: (): Promise<void> => this.personService.resendActivation(id, name),
            },
            disable: {
                condition: async (): Promise<any> => await this.personService.confirmDisableUserAccount(status, name),
                action: async (): Promise<any> => await this.personService.disable(id, name, ['Person']),
            },
            enable: {
                condition: (): any => true,
                action: async (): Promise<any> => await this.personService.enable(id, name, ['Person']),
            },
            delete: {
                condition: async (): Promise<any> => await this.personService.confirmDeletePerson(model),
                action: (): Promise<void> => this.personService.delete(id, model, ['Person']),
            },
            edit: {
                condition: async (): Promise<any> => true,
                action: (): Promise<void> => this.personService.edit(model),
            },
            setToPrimary: {
                condition: (): any => true,
                action: (): Promise<void> => this.personService.setToPrimary(id, model),
            },
        };

        const dismissAction = async (command: PopoverCommand): Promise<void> => {
            this.flipMoreIcon = false;
            if (!(command && command.data)) {
                // no selection was made
                return;
            }
            const actionName: string = command.data.action.actionName;
            if (Object.prototype.hasOwnProperty.call(popoverActions, actionName)
                && await popoverActions[actionName].condition()) {
                this.sharedLoaderService.presentWait().then(() => {
                    (popoverActions[actionName].action()).then(() => this.sharedLoaderService.dismiss());
                });
            } else {
                this.portalExtensionService.executePortalPageTrigger(
                    command.data.action.portalPageTrigger,
                    this.entityTypes.Person,
                    PageType.Display,
                    null,
                    model.id,
                );
            }
        };
        const options: PopoverOptions<any> = {
            component: PopoverPersonComponent,
            cssClass: 'custom-popover more-button-top-popover-positioning',
            componentProps: {
                status: status,
                newStatusTitle: 'Create user account',
                isDefaultOptionsEnabled: true,
                shouldShowPopOverEdit: true,
                shouldShowPopOverNewStatus: status === UserStatus.New,
                shouldShowPopOverResendStatus: status === UserStatus.Invited,
                shouldShowPopOverDisableStatus: status === UserStatus.Active || status === UserStatus.Invited,
                shouldShowPopOverEnableStatus: status === UserStatus.Deactivated || status === UserStatus.Disabled,
                shouldShowPopOverSetToPrimaryPerson: !isPrimaryPerson,
                shouldShowPopOverDeletePerson: true,
                actions: actions,
                entityType: EntityType.Person,
            },
            event: event,
        };

        await this.sharedPopoverService.show(options, 'Person option popover', dismissAction);
    }
}
