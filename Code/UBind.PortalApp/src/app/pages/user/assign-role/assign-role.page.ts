import { Component, OnInit, OnDestroy, ElementRef, Injector, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RoleResourceModel } from '@app/resource-models/role.resource-model';
import { mergeMap, map, finalize, takeUntil } from 'rxjs/operators';
import { RoleType } from '@app/models/role-type.enum';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { FormHelper } from '@app/helpers/form.helper';
import { RoleApiService } from '@app/services/api/role-api.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { HttpErrorResponse } from '@angular/common/http';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { EventService } from '@app/services/event.service';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { OrganisationModel } from '@app/models/organisation.model';

/**
 * Export assign role page component class.
 * This class manage for assigning of roles.
 */
@Component({
    selector: 'app-create-role',
    templateUrl: 'assign-role.page.html',
    styleUrls: [
        './assign-role.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class AssignRolePage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {

    private user: UserResourceModel;
    private userId: any;
    public tenantRoles: Array<RoleResourceModel>;
    public userRoles: Array<RoleResourceModel>;
    public availableTenantRoles: Array<RoleResourceModel>;
    public roleForm: FormGroup;
    public formHasError: boolean = false;
    public userFullName: string;
    public isLoading: boolean = true;
    public errorMessage: string;
    protected destroyed: Subject<void>;
    protected organisationId: string;
    protected performingUserOrganisationId: string;

    public constructor(
        public layoutManager: LayoutManagerService,
        private formHelper: FormHelper,
        private navProxy: NavProxyService,
        private route: ActivatedRoute,
        private roleApiService: RoleApiService,
        private userApiService: UserApiService,
        private formBuilder: FormBuilder,
        private sharedAlert: SharedAlertService,
        private sharedLoaderService: SharedLoaderService,
        private routeHelper: RouteHelper,
        protected eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.performingUserOrganisationSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((organisation: OrganisationModel) => {
                this.performingUserOrganisationId = organisation.id;
            });
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.organisationId = params['organisationId'];
            this.userId = params['userId'];
            this.loadUser();
        });
        this.initializeRoleForm();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    private loadUser(): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        this.userApiService.getById(this.userId, params).pipe(takeUntil(this.destroyed))
            .subscribe((user: UserResourceModel) => {
                this.userFullName = user.fullName;
                this.user = user;
            });
    }

    public ngAfterViewInit(): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        params.set('assignable', 'true');
        const organisationId: string = this.organisationId || this.performingUserOrganisationId;
        if (organisationId) {
            params.set('organisation', organisationId);
        }
        this.roleApiService.getList(params)
            .pipe(
                takeUntil(this.destroyed),
                mergeMap(
                    (tenantRoles: Array<RoleResourceModel>) =>
                        this.userApiService.getUserRoles(this.userId, this.routeHelper.getContextTenantAlias())
                            .pipe(map((userRoles: Array<RoleResourceModel>) => ({ userRoles, tenantRoles }))),
                ),
                finalize(() => this.isLoading = false),
            )
            .subscribe(
                ({ userRoles, tenantRoles }: any) => {
                    this.tenantRoles = tenantRoles.filter((tr: RoleResourceModel) => tr.type === RoleType.Master ||
                        tr.type === RoleType.Client);
                    this.userRoles = userRoles;
                    this.availableTenantRoles =
                        this.tenantRoles.filter(
                            (tr: RoleResourceModel) => !userRoles.map((ur: RoleResourceModel) =>
                                ur.id).includes(tr.id),
                        );
                    if (this.availableTenantRoles.length < 1) {

                        this.sharedAlert.showWithActionHandler({
                            header: 'All available roles assigned',
                            subHeader: 'This user has already been assigned all available roles.',
                            buttons: [{
                                text: 'OK',
                                handler: (): any => {
                                    this.returnToPrevious();
                                },
                            }],
                        });
                    }
                },
                (err: HttpErrorResponse) => {
                    this.errorMessage = 'There was a problem loading roles.';
                    throw err;
                },
            );
    }

    public initializeRoleForm(): void {
        this.roleForm = this.formBuilder.group({
            roleId: ['', [Validators.required]],
        });
    }

    public async didSelectClose(): Promise<void> {
        if (this.hasUnsavedChanges()) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToPrevious();
    }

    public async didSelectSave(): Promise<void> {
        if (!this.roleForm.valid) {
            this.formHasError = true;
            return;
        }

        this.formHasError = false;
        const selectedRole: RoleResourceModel = this.tenantRoles.filter((tr: RoleResourceModel) =>
            tr.id === this.roleForm.value.roleId)[0];

        await this.sharedLoaderService.presentWithDelay();
        const subscription: Subscription = this.userApiService.assignRoleToUser(
            this.userId,
            this.roleForm.value.roleId,
            this.routeHelper.getContextTenantAlias())
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe(
                async () => {
                    await this.sharedAlert.showToast(`${selectedRole.name} role was assigned to ${this.userFullName}`);
                    this.eventService.getEntityUpdatedSubject('User').next(this.user);
                    this.returnToPrevious();
                },
            );
    }

    public returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.pop();
        this.navProxy.navigateBack(pathSegments);
    }

    public requiredFieldHasBeenTouchedAndHasError(controlName: string): boolean {
        return this.roleForm.get(controlName).hasError('required')
            && (this.roleForm.get(controlName).touched || this.formHasError);
    }

    public hasUnsavedChanges(): boolean {
        const formValue: any = this.roleForm.value;
        return formValue.roleId !== '';
    }
}
