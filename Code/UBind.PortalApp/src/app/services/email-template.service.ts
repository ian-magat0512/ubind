import { Injectable } from "@angular/core";
import { EmailTemplateSetting } from "@app/models/email-template";
import { Observable } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { EmailTemplateApiService } from "./api/email-template-api.service";
import { SharedAlertService } from "./shared-alert.service";

/**
 * Export email template service class.
 * This class manage email template functions service.
 */
@Injectable({ providedIn: 'root' })
export class EmailTemplateService {

    public constructor(
        private emailTemplateApiService: EmailTemplateApiService,
        private sharedAlertService: SharedAlertService,
    ) {

    }

    public enableEmailTemplate(tenant: string, emailTemplateId: string, cancelled: Observable<void>): void {
        this.emailTemplateApiService.enable(emailTemplateId, tenant)
            .pipe(takeUntil(cancelled))
            .subscribe((res: EmailTemplateSetting) => {
                this.sharedAlertService.showToast(res.name + ' email template has been enabled');
            });
    }

    public disableEmailTemplate(tenant: string, emailTemplateId: string, cancelled: Observable<void>): void {
        this.emailTemplateApiService.disable(emailTemplateId, tenant)
            .pipe(takeUntil(cancelled))
            .subscribe((res: EmailTemplateSetting) => {
                this.sharedAlertService.showToast(res.name + ' email template has been disabled');
            });
    }

}
