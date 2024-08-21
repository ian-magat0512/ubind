import { Injectable } from '@angular/core';
import { Subscription, BehaviorSubject } from 'rxjs';
import { ToastController } from '@ionic/angular';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { EventService } from '@app/services/event.service';
import { AppConfig } from '@app/models/app-config';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { StringHelper, UrlHelper } from '@app/helpers';
import { Location } from '@angular/common';
import { FeatureSettingResourceModel } from '@app/models';

/**
 * Export App config service class.
 * TODO: Write a better class header: loading of App config.
 */
@Injectable({ providedIn: 'root' })
export class AppConfigService {
    public currentConfig: AppConfig;
    public environmentOnChange$: Subscription;
    public appConfigSubject: BehaviorSubject<AppConfig>;
    public initialisationErrorMessage: string = null;
    private featureSettings: Array<FeatureSettingResourceModel> = new Array<FeatureSettingResourceModel>();

    public constructor(
        protected toastCtrl: ToastController,
        private eventService: EventService,
        private sharedAlertService: SharedAlertService,
        private location: Location,
    ) {
    }

    public getEnvironment(): DeploymentEnvironment {
        if (this.currentConfig.portal.environment) {
            return DeploymentEnvironment[this.currentConfig.portal.environment];
        }
        let environment: string = StringHelper.toPascalCase(
            UrlHelper.getQueryStringParameter("environment", this.location.path()));
        return DeploymentEnvironment[environment] || DeploymentEnvironment.Production;
    }

    public changeEnvironment(newEnvironment: DeploymentEnvironment, isRedirect: boolean = false): void {

        let oldEnvironment: DeploymentEnvironment = this.getEnvironment();
        if (oldEnvironment === newEnvironment) {
            return;
        }

        this.setEnvironment(newEnvironment);

        if (!isRedirect) {
            this.sharedAlertService.showToast(`Switched to ${this.getEnvironment().toLowerCase()} environment`);
        }

        this.eventService.environmentChanged({ oldEnvironment, newEnvironment });
    }

    public setEnvironment(environment: string): void {
        this.currentConfig.portal.environment = environment;
        this.currentConfig.portal.api.accountUrl =
            `${this.currentConfig.portal.api.baseUrl}${this.currentConfig.portal.tenantAlias}/${environment}`;
        this.appConfigSubject.next(this.currentConfig);
    }

    public updatePortalOrganistionDuringLogin(organisationId: string, organisationAlias: string): void {
        this.currentConfig.portal.organisationId = organisationId;
        this.currentConfig.portal.organisationAlias = organisationAlias;
        this.appConfigSubject.next(this.currentConfig);
    }

    public get organisationName(): string {
        return this.currentConfig.portal.organisationName;
    }

    public get organisationAlias(): string {
        return this.currentConfig.portal.organisationAlias;
    }

    public get tenantAlias(): string {
        return this.currentConfig.portal.tenantAlias;
    }

    public get isDefaultOrganisation(): boolean {
        return this.currentConfig.portal.isDefaultOrganisation;
    }

    public getCurrentPortalAlias(): string {
        return this.currentConfig.portal.alias || null;
    }

    public setFeatureSettings(featureSettings: Array<FeatureSettingResourceModel>): void {
        this.featureSettings = featureSettings;
    }

    public getFeatureSettings(): Array<FeatureSettingResourceModel> {
        return this.featureSettings;
    }

    public isMasterPortal(): boolean {
        return this.currentConfig.portal.tenantAlias == 'ubind'
            || this.currentConfig.portal.tenantAlias == 'master';
    }
}
