import { OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { AppConfig } from '../models/app-config';
import { AppConfigService } from '../services/app-config.service';

/**
 * Export Profile pic url pipe class
 * This class is for transforming the profile picture urls.
 */
@Pipe({ name: 'profilePicUrl' })
export class ProfilePicUrlPipe implements PipeTransform, OnDestroy {
    public imageBaseUrl: string;
    public appConfigSub: Subscription;
    public tenantId: string;

    public constructor(private sanitizer: DomSanitizer, private appConfigService: AppConfigService) {
        this.appConfigSub = this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.imageBaseUrl = appConfig.portal.api.baseUrl + 'picture/';
            this.tenantId = appConfig.portal.tenantId;
        });
    }
    public ngOnDestroy(): void {
        if (this.appConfigSub) {
            this.appConfigSub.unsubscribe();
        }
    }

    public transform(value: string, fallback: string = null): SafeUrl {
        let url: string = fallback;

        if (value) {
            url = this.imageBaseUrl + value + `?tenant=${this.tenantId}`;
        }

        return this.sanitizer.bypassSecurityTrustUrl(url);
    }

}
