import { Component, OnDestroy, OnInit } from "@angular/core";
import { DeploymentEnvironment } from "@app/models/deployment-environment.enum";
import { EnvironmentChange } from "@app/models/environment-change";
import { AppConfigService } from "@app/services/app-config.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { EventService } from "@app/services/event.service";
import { SharedPopoverService } from "@app/services/shared-popover.service";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { PopoverEnvironmentSelectionComponent,
} from "../popover-environment-selection/popover-environment-selection.component";
import { contentAnimation } from '../../../assets/animations';
import { AppConfig } from "@app/models/app-config";
import { PermissionService } from "@app/services/permission.service";

/**
 * A control which sits at the bottom of the page which allows the user to see and change the data environment
 * (e.g. development, staging or production)
 */
@Component({
    selector: 'app-environment-control',
    templateUrl: './environment-control.component.html',
    styleUrls: [ './environment-control.component.scss' ],
    animations: [ contentAnimation ],
})
export class EnvironmentControlComponent implements OnInit, OnDestroy {

    public canChangeEnvironment: boolean = false;
    public isDevelopmentOrStaging: boolean = false;
    public environment: DeploymentEnvironment;
    public isAgentUser: boolean = false;

    protected destroyed: Subject<void>;

    public constructor(
        public authenticationService: AuthenticationService,
        private eventService: EventService,
        private appConfigService: AppConfigService,
        private sharedPopoverService: SharedPopoverService,
        private permissionService: PermissionService,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.canChangeEnvironment = this.permissionService.canAccessOtherEnvironments();
        this.eventService.environmentChangedSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((ec: EnvironmentChange) => {
                this.environment = ec.newEnvironment;
                this.updateState();
            });
        this.appConfigService.appConfigSubject.pipe(takeUntil(this.destroyed))
            .subscribe((appConfig: AppConfig) => {
                this.environment = <any>appConfig.portal.environment;
                this.updateState();
            });
        this.eventService.userAuthenticatedSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((authenticated: boolean) => {
                this.isAgentUser = authenticated && this.authenticationService.isAgent();
                this.updateState();
            });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async clickEnvironmentSelection(event: any, type: any): Promise<void> {
        if (this.canChangeEnvironment) {
            const popoverDismissAction = (data: any): void => {
                if (data?.data) {
                    this.appConfigService.changeEnvironment(data.data.environment);
                }
            };

            await this.sharedPopoverService.show(
                {
                    component: PopoverEnvironmentSelectionComponent,
                    showBackdrop: false,
                    cssClass: 'custom-popover bottom-popover-positioning',
                    mode: 'md',
                    event: event,
                },
                'My account environment option popover',
                popoverDismissAction,
            );
        }
    }

    private updateState(): void {
        this.canChangeEnvironment = this.permissionService.canAccessOtherEnvironments() && this.isAgentUser;
        this.isDevelopmentOrStaging = this.environment == DeploymentEnvironment.Staging ||
            this.environment == DeploymentEnvironment.Development;
    }
}
