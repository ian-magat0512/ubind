import { Component, OnInit, OnDestroy } from '@angular/core';
import { PopoverController } from '@ionic/angular';
import { Subject, SubscriptionLike } from 'rxjs';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { DkimSettingsService } from '@app/services/dkim-settings.service';
import { DkimSettingsResourceModel } from '@app/resource-models/dkim-settings.resource-model';
import { RouteHelper } from '@app/helpers/route.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * This is a popper used for deleting DKIM Settings.
 */
@Component({
    selector: 'app-popover-dkim-settings',
    templateUrl: './popover-dkim-settings.page.html',
})
export class PopoverDkimSettingPage implements OnInit, OnDestroy {
    public dkimSettingsId: string;
    public organisationId: string;
    public dkimSettingsResourceModel: DkimSettingsResourceModel;
    public permission: typeof Permission = Permission;
    public permissionModel: PermissionDataModel;
    public canDeleteDkimSettings: boolean = true;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    protected destroyed: Subject<void>;
    public constructor(
        private popOverCtrl: PopoverController,
        protected sharedAlertService: SharedAlertService,
        private dkimSettingsService: DkimSettingsService,
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
    ) {
    }

    public async ngOnInit(): Promise<void> {
        this.destroyed = new Subject<void>();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async delete(): Promise<void> {
        const shouldDelete: boolean = await this.confirmDeleteDKIMSettings();
        if (shouldDelete) {
            await this.dkimSettingsService.delete(
                this.dkimSettingsResourceModel.id,
                this.dkimSettingsResourceModel.organisationId,
                this.dkimSettingsResourceModel.domainName);
            const pathSegments: Array<string> = this.routeHelper.getPathSegments();
            pathSegments.pop();
            pathSegments.push('list');
            this.navProxy.navigate(pathSegments);
        }
        this.popOverCtrl.dismiss();
    }

    public sendTestEmail(): void {
        let pathSegments: Array<string> = this.routeHelper.appendPathSegment("send");
        this.popOverCtrl.dismiss();
        this.navProxy.navigate(pathSegments);
    }

    private confirmDeleteDKIMSettings(): Promise<boolean> {
        return new Promise((resolve: any): any => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Delete DKIM Configuration',
                subHeader:
                    `Please confirm that you want to delete this DKIM Configuration. This action cannot be undone`,
                buttons: [
                    {
                        text: 'Cancel',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Delete',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }
}
