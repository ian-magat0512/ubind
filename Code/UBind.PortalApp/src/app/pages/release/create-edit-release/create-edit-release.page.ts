import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { AlertController } from '@ionic/angular';
import { ReleaseType } from '@app/models';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { ReleaseApiService } from '@app/services/api/release-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { ReleaseService } from '@app/services/release.service';
import { FormHelper } from '@app/helpers/form.helper';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { ReleaseDetailViewModel } from '@app/viewmodels/release-detail.viewmodel';
import { finalize } from 'rxjs/operators';

/**
 * Export create/edit release page component class.
 * This class manage creation and editing of release page.
 */
@Component({
    selector: 'app-create-edit-release',
    templateUrl: './create-edit-release.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditReleasePage
    extends CreateEditPage<ReleaseResourceModel> implements OnInit {
    public isEdit: boolean;
    public subjectName: string = "Release";
    private tenantId: string;
    private tenantAlias: string;
    private productAlias: string;
    public releaseId: string;

    public constructor(
        protected alertCtrl: AlertController,
        protected eventService: EventService,
        public navProxy: NavProxyService,
        protected sharedLoaderService: SharedLoaderService,
        protected releaseApiService: ReleaseApiService,
        protected releaseService: ReleaseService,
        private routeHelper: RouteHelper,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        protected tenantService: TenantService,
        protected productService: ProductService,
        elementRef: ElementRef,
        injector: Injector,
        public formHelper: FormHelper,
        protected appConfigService: AppConfigService,
    ) {
        super(eventService, elementRef, injector, formHelper);
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
            }
        });
        this.buildForm();
    }

    public ngOnInit(): void {
        this.tenantAlias = this.routeHelper.getContextTenantAlias();
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.releaseId = this.routeHelper.getParam('releaseId');
        this.isEdit = this.releaseId != null;
        super.ngOnInit();
        this.isLoading = this.isEdit;
        if (this.isEdit) {
            this.load();
        } else {
            this.detailList = ReleaseDetailViewModel.createDetailsListForEdit(!this.isEdit);
        }
    }

    protected buildForm(): FormGroup {
        this.form = this.formBuilder.group({
            id: '00000000-0000-0000-0000-000000000000',
            description: ['', [Validators.required]],
        });

        if (!this.isEdit) {
            this.form.addControl('releaseType', new FormControl('0', Validators.required));
            this.generateFormSelection();
        }

        return this.form;
    }

    public load(): void {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        this.releaseApiService.getById(this.releaseId, params)
            .pipe(finalize(() => this.isLoading = false))
            .subscribe((release: ReleaseResourceModel) => {
                this.model = release;
                const _release: any = {
                    id: this.model.id,
                    description: this.model.description,
                    releaseType: ReleaseType[this.model.type],
                };
                this.detailList = ReleaseDetailViewModel.createDetailsListForEdit(!this.isEdit);
                this.form.setValue(_release);
            });
    }

    public async create(value: any): Promise<void> {
        this.releaseService.createRelease(
            this.tenantAlias,
            this.productAlias,
            value.description,
            value.releaseType);
    }

    public async update(value: any): Promise<void> {
        let model: any = {
            id: this.model.id,
            tenantId: this.model.tenantId,
            productId: this.productAlias,
            type: value.releaseType,
            description: value.description,
        };

        this.releaseService.updateRelease(model, this.tenantAlias);
    }

    public returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        if (pathSegments[pathSegments.length - 1] == 'edit') {
            pathSegments.pop();
            this.navProxy.navigateForward(pathSegments);
        } else if (pathSegments[pathSegments.length - 1] == 'create') {
            pathSegments.pop();
            if (this.releaseId) {
                pathSegments.push(this.releaseId);
                this.navProxy.navigateForward(pathSegments);
            } else {
                pathSegments.pop();
                this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Releases' } });
            }
        }
    }

    private generateFormSelection(): void {
        const items: any = [
            { label: 'Minor', value: '0' },
            { label: 'Major', value: '1' },
        ];
        this.fieldOptions.push({ name: "releaseType", options: items, type: "option" });
    }
}
