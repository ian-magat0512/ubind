import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { ClaimResourceModel, ClaimVersionResourceModel } from '@app/resource-models/claim.resource-model';
import { Permission } from '@app/helpers';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { ClaimVersionApiService } from '@app/services/api/claim-version.api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { LoadDataService } from '@app/services/load-data.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ClaimVersionViewModel } from '@app/viewmodels/claim-version.viewmodel';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Component class for claim version list page.
 */
@Component({
    selector: 'app-list-claim-version',
    templateUrl: './list-claim-version.page.html',
    styles: [],
})
export class ListClaimVersionPage implements OnInit, OnDestroy {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<ClaimVersionViewModel, ClaimVersionResourceModel>;
    public title: string = 'Claim Version';
    public permission: typeof Permission = Permission;
    public viewModelConstructor: any = ClaimVersionViewModel;
    private destroyed: Subject<void> = new Subject<void>();

    public claimId: string;
    public claimVersionId: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        protected changeDetectorRef: ChangeDetectorRef,
        protected route: ActivatedRoute,
        protected router: Router,
        public claimApiService: ClaimApiService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        protected eventService: EventService,
        public claimVersionApiService: ClaimVersionApiService,
        protected routeHelper: RouteHelper,
        protected userPath: UserTypePathHelper,
        private authService: AuthenticationService,
    ) {
    }

    public ngOnInit(): void {
        this.claimId = this.routeHelper.getParam('claimId');
        this.claimVersionId = this.routeHelper.getParam('claimVersionId');
        this.title = this.routeHelper.getParam('title') ||
            this.authService.isCustomer() ? 'My Claim Versions' : 'Claim Versions';
        this.claimApiService.getById(this.claimId)
            .pipe(takeUntil(this.destroyed))
            .subscribe(
                (dt: ClaimResourceModel) => {
                    this.title = dt.claimReference === null
                        ? 'Claim Versions'
                        : 'Claim Versions for ' + dt.claimReference;
                },
            );
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('claimId', this.claimId);
        return params;
    }

    public itemSelected(item: ClaimVersionViewModel): void {
        this.navProxy.navigateForward([this.userPath.claim, this.claimId, 'version', item.id], true);
    }
}
