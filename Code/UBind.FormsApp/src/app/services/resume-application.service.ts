import { Injectable } from "@angular/core";
import { ResilientStorage } from '@app/storage/resilient-storage';
import { Subject } from 'rxjs';
import { ApplicationService } from './application.service';
import { SessionDataManager } from '../storage/session-data-manager';

/**
 * Export resume application service class.
 * TODO: Write a better class header: resume applications functions.
 */
@Injectable()
export class ResumeApplicationService {

    public readonly QuoteIdStorageKey: string = "quoteId";
    public readonly ClaimIdStorageKey: string = "claimId";
    public readonly ExpiresStorageKey: string = "expires";

    public savedQuoteIdChangeSubject: Subject<string> = new Subject<string>();
    public savedClaimIdChangeSubject: Subject<string> = new Subject<string>();

    public constructor(
        protected storage: ResilientStorage,
        protected sessionDataManager: SessionDataManager,
        protected applicationService: ApplicationService,
    ) { }

    public saveQuoteIdForLater(value: string, dayOffset: number, now: Date = new Date()): void {
        this.savePropertyForLater(this.QuoteIdStorageKey, value, dayOffset, now);
        this.savedQuoteIdChangeSubject.next(value);
    }

    public saveClaimIdForLater(value: string, dayOffset: number, now: Date = new Date()): void {
        this.savePropertyForLater(this.ClaimIdStorageKey, value, dayOffset, now);
        this.savedClaimIdChangeSubject.next(value);
    }

    public savePropertyForLater(
        propertyName: string,
        value: string,
        dayOffset: number,
        now: Date = new Date()): void {
        let expiry: string = this.getExpiry(dayOffset, now).toUTCString();
        let key: string = this.getKey();
        let quoteSession: any = {
            [propertyName]: value,
            [this.ExpiresStorageKey]: expiry,
        };

        this.storage.setItem(key, JSON.stringify(quoteSession));
    }

    private getKey(): string {
        return this.applicationService.tenantAlias + '~~~'
            + this.applicationService.organisationAlias + '~~~'
            + this.applicationService.productAlias + '~~~'
            + this.applicationService.environment;
    }

    private getExpiry(dayOffset: number, now: Date = new Date()): Date {
        let d: Date = now;
        d.setTime(d.getTime() + (dayOffset * 24 * 60 * 60 * 1000));
        return d;
    }

    public loadExistingQuoteId(now: Date = new Date()): string {
        return this.loadExistingProperty(this.QuoteIdStorageKey, now);
    }

    public loadExistingClaimId(now: Date = new Date()): string {
        return this.loadExistingProperty(this.ClaimIdStorageKey, now);
    }

    public loadExistingProperty(propertyName: string, now: Date = new Date()): string {
        let key: string = this.getKey();
        let value: string = this.sessionDataManager.getQuoteSessionValue(propertyName, key);
        if (!value) {
            return null;
        }
        let expiry: string = this.sessionDataManager.getQuoteSessionValue(this.ExpiresStorageKey, key);
        let nowTimestamp: number = now.getTime();
        let expiryTimestamp: number = Date.parse(expiry);
        if (nowTimestamp > expiryTimestamp) {
            this.storage.removeItem(key);
            return null;
        }
        return value;
    }

    public deleteQuoteId(): void {
        if (this.deleteExistingProperty(this.QuoteIdStorageKey)) {
            this.savedQuoteIdChangeSubject.next();
        }
    }

    public deleteClaimId(): void {
        if (this.deleteExistingProperty(this.ClaimIdStorageKey)) {
            this.savedClaimIdChangeSubject.next();
        }
    }

    public deleteExistingProperty(propertyName: string): boolean {
        let key: string = this.getKey();
        if (this.sessionDataManager.getQuoteSessionValue(propertyName, key)) {
            this.sessionDataManager.removeQuoteSessionValue(propertyName, key);
            return true;
        }
        return false;
    }
}
