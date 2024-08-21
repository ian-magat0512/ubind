import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';
import { Observable } from 'rxjs';

/**
 * Export file attachment API service class.
 * TODO: Write a better class header: file attachment API functions.
 */
@Injectable({ providedIn: 'root' })
export class FileAttachmentApiService {
    private baseUrl: string = null;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((ac: AppConfig) => {
            this.baseUrl = ac.portal.api.baseUrl;
        });
    }

    public downloadFileAttachment(entityType: string, entityId: string, attachmentId: string): Observable<any> {
        const url: string = `${this.baseUrl}${entityType}/${entityId}/attachment/${attachmentId}/content`;
        return this.httpClient.get(url, { responseType: 'blob' });
    }
}
