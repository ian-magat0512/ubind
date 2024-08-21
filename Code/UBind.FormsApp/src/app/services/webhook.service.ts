import { Injectable, EventEmitter, Output, Directive } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { ApiCacheService } from './api-cache.service';

/**
 * Export webhook service class.
 * TODO: Write a better class header: webhook service functions.
 */
@Directive()
@Injectable()
export class WebhookService {

    public inProgressSubject: BehaviorSubject<boolean> = new BehaviorSubject(false);
    public webhookFieldInProgressSubject: BehaviorSubject<boolean> = new BehaviorSubject(false);
    @Output() public activeWebhookCount: EventEmitter<any> = new EventEmitter<any>();

    private _activeWebhookCount: number = 0;
    private activeWebhookFields: Array<string> = [];

    public constructor(
        public apiCache: ApiCacheService,
    ) {
    }

    public addActiveWebhookField(webhookFieldPath: string): void {
        this.activeWebhookFields.push(webhookFieldPath);
        this.webhookFieldInProgressSubject.next(true);
        this.increaseActiveWebhookCount();
    }

    public removeActiveWebhookField(webhookFieldPath: string): void {
        let index: number = this.activeWebhookFields.findIndex((value: string) => value == webhookFieldPath);
        this.activeWebhookFields.splice(index, 1);
        this.webhookFieldInProgressSubject.next(false);
        this.reduceActiveWebhookCount();
    }

    private increaseActiveWebhookCount(): void {
        this._activeWebhookCount++;
        this.inProgressSubject.next(true);
        this.activeWebhookCount.emit(this._activeWebhookCount);
    }

    private reduceActiveWebhookCount(): void {
        this._activeWebhookCount--;
        if (this._activeWebhookCount < 1) {
            this.inProgressSubject.next(false);
        }
        this.activeWebhookCount.emit(this._activeWebhookCount);
    }

    public getActiveWebhookCount(): number {
        return this._activeWebhookCount;
    }

    public getWebhookFieldIsActive(webhookFieldPath: string): boolean {
        return this.activeWebhookFields.includes(webhookFieldPath);
    }

    public sendRequest(
        webhookFieldPath: string,
        httpVerb: string,
        url: string,
        body: any,
        cacheMaxAgeSeconds: number = null,
    ): Observable<any> {
        this.addActiveWebhookField(webhookFieldPath);
        return this.apiCache.processRequest(
            httpVerb,
            url,
            body,
            cacheMaxAgeSeconds)
            .pipe(
                finalize(() => {
                    this.removeActiveWebhookField(webhookFieldPath);
                }),
            );
    }
}
