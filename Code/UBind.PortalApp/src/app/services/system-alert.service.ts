import { Injectable } from "@angular/core";
import { SystemAlertResourceModel } from "@app/models";
import { Observable } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { SystemAlertApiService } from "./api/system-alert-api.service";
import { SharedAlertService } from "./shared-alert.service";

/**
 * Export system alert service class.
 * TODO: Write a better class header: system alert service functions.
 */
@Injectable({ providedIn: 'root' })
export class SystemAlertService {

    public constructor(
        private systemAlertApiService: SystemAlertApiService,
        private sharedAlertService: SharedAlertService,
    ) {

    }

    public disableSystemAlert(
        tenant: string,
        systemAlert: SystemAlertResourceModel,
        cancelled: Observable<void>,
    ): void {
        this.systemAlertApiService.disable(tenant, systemAlert.id)
            .pipe(takeUntil(cancelled))
            .subscribe(
                (dt: SystemAlertResourceModel) => {
                    systemAlert.alertMessage = dt.alertMessage;
                    this.sharedAlertService.showToast(dt.systemAlertType.name + ' system alert has been disabled.');
                },
            );
    }

    public enableSystemAlert(tenant: string, systemAlert: SystemAlertResourceModel, cancelled: Observable<void>): void {
        this.systemAlertApiService.enable(tenant, systemAlert.id)
            .pipe(takeUntil(cancelled))
            .subscribe(
                (dt: SystemAlertResourceModel) => {
                    systemAlert.alertMessage = dt.alertMessage;
                    this.sharedAlertService.showToast(dt.systemAlertType.name + ' system alert has been enabled.');
                },
            );
    }
}
