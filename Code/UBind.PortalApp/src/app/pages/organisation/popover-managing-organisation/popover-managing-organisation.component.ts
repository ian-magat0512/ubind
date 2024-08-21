import { Component, OnDestroy, OnInit } from "@angular/core";
import { RouteHelper } from "@app/helpers/route.helper";
import { OrganisationResourceModel } from "@app/resource-models/organisation/organisation.resource-model";
import { OrganisationApiService } from "@app/services/api/organisation-api.service";
import { EventService } from "@app/services/event.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { PopoverController } from "@ionic/angular";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

/**
 * Popover menu to show against the managing organisation property on the detail page of
 * an organisation.
 */
@Component({
    selector: "app-popover-managing-organisation",
    templateUrl: "./popover-managing-organisation.component.html",
    styleUrls: ["./popover-managing-organisation.component.scss"],
})
export class PopoverManagingOrganisationComponent implements OnInit, OnDestroy {

    public organisationId: string;
    public organisationName: string;
    public managingOrganisationId: string;
    public managingOrganisationName: string;
    public destroyed: Subject<void>;

    public constructor(
        private navProxy: NavProxyService,
        private popoverController: PopoverController,
        private routeHelper: RouteHelper,
        private sharedAlertService: SharedAlertService,
        private organisationApiService: OrganisationApiService,
        private sharedLoaderService: SharedLoaderService,
        private eventService: EventService,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public assignManagingOrganisation(): void {
        const pathSegments: Array<string> = this.routeHelper.appendPathSegment('managing-organisation');
        this.navProxy.navigate(pathSegments);
        this.popoverController.dismiss();
    }

    public unassignManagingOrganisation(): void {
        this.sharedAlertService.showWithActionHandler({
            header: `Un-assign managing organisation`,
            message: `If you complete this action then users from ${this.managingOrganisationName} will no longer be `
                + `able to perform management functions on ${this.organisationName}. Are you sure you want to `
                + `proceed?`,
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): void => {
                        this.popoverController.dismiss();
                    },
                }, {
                    text: 'Un-assign',
                    handler: async (): Promise<void> => {
                        await this.sharedLoaderService.presentWithDelay();
                        try {
                            const updatedOrganisation: OrganisationResourceModel
                                = await this.organisationApiService.setManagingOrganisation(
                                    this.organisationId, null, this.routeHelper.getContextTenantAlias())
                                    .pipe(takeUntil(this.destroyed)).toPromise();
                            this.sharedAlertService.showToast(`${this.managingOrganisationName} was unassigned as `
                                + `managing organisation for ${this.organisationName}`);
                            this.eventService.getEntityUpdatedSubject('Organisation').next(updatedOrganisation);
                        } finally {
                            this.sharedLoaderService.dismiss();
                            this.popoverController.dismiss();
                        }
                    },
                },
            ],
        });
    }
}
