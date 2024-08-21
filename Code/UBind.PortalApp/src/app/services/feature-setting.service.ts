import { Injectable } from '@angular/core';
import { MenuItem } from '../models/menu-item';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';
import { FeatureSetting } from '@app/models/feature-setting.enum';
import { FeatureSettingResourceModel } from '@app/resource-models/feature-setting.resource-model';
import { FeatureSettingApiService } from './api/feature-setting-api.service';
import { EventService } from './event.service';
import { interval } from 'rxjs';
import { AuthenticationService } from './authentication.service';
import * as _ from 'lodash';

/**
 * The feature settings service holds feature settings.
 */
@Injectable({ providedIn: 'root' })
export class FeatureSettingService {

    private features: Array<FeatureSetting> = new Array<FeatureSetting>();
    private tenantAlias: string;
    private tenantId: string;
    private portalId: string;
    private isMasterPortal: boolean;

    public constructor(
        private appConfigService: AppConfigService,
        private featureSettingApiService: FeatureSettingApiService,
        private eventService: EventService,
        private authenticationService: AuthenticationService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
            this.portalId = appConfig.portal.portalId;
            this.tenantAlias = appConfig.portal.tenantAlias;
            this.isMasterPortal = this.appConfigService.isMasterPortal();
        });

        this.processFeatures(this.appConfigService.getFeatureSettings());

        // set the interval into 15minutes to reload the feature settings.
        interval(900000).subscribe(async () => {
            if (this.authenticationService.isAuthenticated()) {
                await this.loadFeatureSettings();
            }
        });
    }

    private processFeatures(featureSettings: Array<FeatureSettingResourceModel>): void {
        this.features = new Array<FeatureSetting>();
        featureSettings.forEach((pfs: FeatureSettingResourceModel) => {
            if (!pfs.disabled) {
                this.features.push(<FeatureSetting>pfs.name);
            }
        });
    }

    public getFeatures(): Array<FeatureSetting> {
        return this.features;
    }

    public hasFeature(feature: FeatureSetting): boolean {
        if (this.appConfigService.isMasterPortal()) {
            return true;
        }

        return this.features.includes(feature);
    }

    public hasCustomerFeature(): boolean {
        return this.hasFeature(FeatureSetting.CustomerManagement);
    }

    public hasPolicyFeature(): boolean {
        return this.hasFeature(FeatureSetting.PolicyManagement);
    }

    public hasQuoteFeature(): boolean {
        return this.hasFeature(FeatureSetting.QuoteManagement);
    }

    public hasClaimFeature(): boolean {
        return this.hasFeature(FeatureSetting.ClaimsManagement);
    }

    public hasUserManagementFeature(): boolean {
        return this.hasFeature(FeatureSetting.UserManagement);
    }

    public hasEmailFeature(): boolean {
        return this.hasFeature(FeatureSetting.MessageManagement);
    }

    public hasProductFeature(): boolean {
        return this.hasFeature(FeatureSetting.PolicyManagement);
    }

    public hasOrganisationFeature(): boolean {
        return this.hasFeature(FeatureSetting.OrganisationManagement);
    }

    public hasPortalManagementFeature(): boolean {
        return this.hasFeature(FeatureSetting.PortalManagement);
    }

    public removeMenuItemsForDisabledFeatures(menuItems: Array<MenuItem>): Array<MenuItem> {
        if (!this.features) {
        // they've not loaded yet
            return new Array<MenuItem>();
        }
        let filteredMenuItems: Array<MenuItem> = menuItems.filter((menuItem: MenuItem) => {
            if (!menuItem.requiresFeature) {
                // this menu item doesn't require an enabled feature
                return true;
            }

            return this.features.includes(menuItem.requiresFeature);
        });
        return filteredMenuItems;
    }

    private async loadFeatureSettings(): Promise<void> {
        if (this.isMasterPortal) {
            // master portal doesn't have feature settings
            return;
        }
        let newFeatureSettings: Array<FeatureSettingResourceModel> = this.portalId != null
            ? await this.featureSettingApiService.getPortalSettings(this.tenantId, this.portalId).toPromise()
            : await this.featureSettingApiService.getTenantSettings(this.tenantId).toPromise();
        let oldFeatureSettings: Array<FeatureSettingResourceModel> = this.appConfigService.getFeatureSettings();
        const hasFeatureSettingChanged: boolean = _.isEqual(oldFeatureSettings, newFeatureSettings);
        if (!hasFeatureSettingChanged) {
            this.appConfigService.setFeatureSettings(newFeatureSettings);
            this.processFeatures(newFeatureSettings);
            this.eventService.featureSettingChanged();
        }
    }
}
