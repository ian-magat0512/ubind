import { Injectable } from "@angular/core";
import { AppConfig } from "@app/models/app-config";
import {
    PageResourceModel,
    PortalPageTriggerResourceModel,
} from "@app/resource-models/portal-page-trigger.resource-model";
import { PortalExtensionsApiService } from "./api/portal-extensions-api.service";
import { AppConfigService } from "./app-config.service";
import { Observable, Subject } from "rxjs";
import { map, finalize, takeUntil } from "rxjs/operators";
import { SharedLoaderService } from "./shared-loader.service";
import { SharedAlertService } from "./shared-alert.service";
import { saveAs } from 'file-saver';
import { StringHelper, getFilenameFromContentDisposition } from "@app/helpers";
import { AuthenticationService } from "./authentication.service";
import { ActionButtonPopover } from "@app/models/action-button-popover";
import { IconLibrary } from "@app/models/icon-library.enum";
import { PageType } from "@app/models/page-type.enum";

/**
 * Portal extensions service class.
 * This class manages the portal extensions service.
 * 
 * It caches the value for 60 seconds and loads lazily.
 */
@Injectable({ providedIn: 'root' })
export class PortalExtensionsService {
    private static maximumAgeSeconds: number = 60;
    private portalExtensions: Array<PortalPageTriggerResourceModel>;
    private lastLoadedTimestamp: number = 0;
    protected destroyed: Subject<void> = new Subject<void>();

    public constructor(
        private stringHelper: StringHelper,
        private portalExtensionApiService: PortalExtensionsApiService,
        private appConfigService: AppConfigService,
        private sharedLoaderService: SharedLoaderService,
        private sharedAlertService: SharedAlertService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (appConfig.portal.tenantName) {
                this.portalExtensions = null;
            }
        });
    }

    private async loadPortalPageTriggers(tenantId: string): Promise<void> {
        this.portalExtensions =
            await this.portalExtensionApiService.getPortalPageTriggers(tenantId).toPromise();
    }

    public async getPortalPageTriggers(tenantId: string): Promise<Array<PortalPageTriggerResourceModel>> {
        let now: number = Date.now();
        let lastLoadedAgeSeconds: number = (now - this.lastLoadedTimestamp) / 1000;
        let hasExpired: boolean = lastLoadedAgeSeconds > PortalExtensionsService.maximumAgeSeconds;

        if (!this.portalExtensions || hasExpired) {
            this.lastLoadedTimestamp = now;
            await this.loadPortalPageTriggers(tenantId);
        }
        return this.portalExtensions;
    }

    public async getEnabledPortalPageTriggers(
        authService: AuthenticationService,
        entityType: string,
        pageType: PageType,
        segment: string = null,
    ): Promise<Array<PortalPageTriggerResourceModel>> {
        if (authService.isCustomer()) {
            return new Array<PortalPageTriggerResourceModel>();
        }

        const tenantId: string = authService.tenantId;
        const triggers: Array<PortalPageTriggerResourceModel> = await this.getPortalPageTriggers(tenantId);
        const enabledTriggers: Array<PortalPageTriggerResourceModel> =
            triggers.filter((s: PortalPageTriggerResourceModel) =>
                this.isPortalPageTriggerEnabled(s, entityType, pageType, segment));
        return enabledTriggers;
    }

    public getActionButtonPopovers(triggers: Array<PortalPageTriggerResourceModel>): Array<ActionButtonPopover> {
        let actions: Array<ActionButtonPopover> = [];
        triggers.forEach((trigger: PortalPageTriggerResourceModel) => {
            actions.push({
                actionName: trigger.actionName ? trigger.actionName : trigger.actionButtonLabel,
                actionIcon: trigger.actionIcon ? trigger.actionIcon : "play-circle",
                iconLibrary: trigger.actionIconLibrary ? trigger.actionIconLibrary : IconLibrary.IonicV4,
                actionButtonLabel: trigger.actionButtonLabel,
                actionButtonPrimary: trigger.actionButtonPrimary,
                includeInMenu: trigger.includeInMenu,
                portalPageTrigger: trigger,
            });
        });
        return actions;
    }

    public async executePortalPageTrigger(
        trigger: PortalPageTriggerResourceModel,
        entityType: string,
        pageType: PageType,
        tab: string = null,
        entityId: string = '',
        filters: Map<string, string | Array<string>> = new Map(),
    ): Promise<void> {
        const message: string = trigger.spinnerAlertText || 'Automation is running, please wait...';
        this.sharedLoaderService.present(message)
            .then((result: void) => {
                this.sendPortalPageTriggerRequest(trigger, entityType, pageType, tab, entityId, filters)
                    .pipe(
                        finalize(() => this.sharedLoaderService.dismiss()),
                        takeUntil(this.destroyed),
                    )
                    .subscribe(
                        (response: any) => {
                            const filename: string =
                                getFilenameFromContentDisposition(response.headers.get('content-disposition'));
                            if (filename) {
                                saveAs(response.body, filename.toLowerCase());
                            }

                            const successMessage: string = response.headers.get("success-message");
                            if (successMessage) {
                                this.sharedAlertService.showToast(successMessage);
                            }
                        },
                    );
            });
    }

    private sendPortalPageTriggerRequest(
        portalPageTrigger: PortalPageTriggerResourceModel,
        entityType: string,
        pageType: PageType,
        tab: string = null,
        entityId: string = null,
        filters: Map<string, string | Array<string>>,
    ): Observable<any> {
        return this.portalExtensionApiService
            .executePortalPageTrigger(portalPageTrigger, entityType, pageType, tab, entityId, filters)
            .pipe(map((res: any) => res));
    }

    private isPortalPageTriggerEnabled(
        trigger: PortalPageTriggerResourceModel,
        entityType: string,
        pageType: PageType,
        segment: string,
    ): boolean {
        let isEnabled: boolean = false;
        const page: PageResourceModel = trigger.pages
            .find((p: PageResourceModel) =>
                this.stringHelper.equalsIgnoreCase(p.entityType, entityType) &&
                this.stringHelper.equalsIgnoreCase(p.pageType, pageType));
        isEnabled =
            page && (!page.tabs || page.tabs.length == 0 || !segment || page.tabs.includes(segment.toLowerCase()));
        return isEnabled;
    }

    public clearPortalFeatureSettings(): void {
        this.portalExtensions = null;
    }
}
