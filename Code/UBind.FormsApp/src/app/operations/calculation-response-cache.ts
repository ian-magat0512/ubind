import { Injectable } from "@angular/core";
import { WorkingConfiguration } from "@app/models/configuration/working-configuration";
import { ApplicationService } from "@app/services/application.service";
import { EventService } from "@app/services/event.service";
import { md5 } from 'hash-wasm';

export interface CalculationCacheEntry {
    requestPayload: any;
    responsePayload: any;
    timestamp: number;
}

/**
 * Stores the last N calculation requests in a cache so we can provide instant
 * responses for previous requests.
 */
@Injectable({
    providedIn: 'root',
})
export class CalculationResponseCache {

    private static maxAgeMinutes: number = 10;
    private static clearFrequencySeconds: number = 60;
    public static maxEntries: number = 20;
    private entries: Map<string, CalculationCacheEntry> = new Map<string, CalculationCacheEntry>();

    public constructor(
        private applicationService: ApplicationService,
        eventService: EventService,
    ) {
        setInterval(() => {
            this.clearOldCacheEntries();
        },
        CalculationResponseCache.clearFrequencySeconds * 1000);

        // clear the cache when the configuration changes
        eventService.updatedConfiguration.subscribe((config: WorkingConfiguration) => this.entries.clear());
    }

    public async tryGetCachedResponse(requestPayload: any): Promise<any> {
        const hash: string = await this.hash(requestPayload);
        const entry: CalculationCacheEntry = this.entries.get(hash);
        if (this.applicationService.debug && entry) {
            console.log(`Found cached response with ${hash} `
                + `created ${this.getSecondsAgo(entry.timestamp)} seconds ago`);
        }
        return entry ? entry.responsePayload : null;
    }

    public async addSuccessfulResponse(
        requestPayload: any,
        responsePayload: any,
        timestamp: number = Date.now(),
    ): Promise<void> {
        const hash: string = await this.hash(requestPayload);
        if (this.entries.get(hash)) {
            if (this.applicationService.debug) {
                console.log(`removing and re-adding ${hash} in the calculation response cache, because it already `
                    + `exists`);
            }
            // remove and re-add so it's added to the end
            this.entries.delete(hash);
        }
        this.entries.set(hash, {
            requestPayload: requestPayload,
            responsePayload: responsePayload,
            timestamp: timestamp,
        });
        if (this.applicationService.debug) {
            console.log(`added entry ${hash} to calculation response cache, which now has size ${this.entries.size}`);
        }
        this.trimToMaxSize();
    }

    private async hash(requestPayload: any): Promise<string> {
        const stringified: string = JSON.stringify(requestPayload);
        try {
            return await md5(stringified);
        } catch (err: any) {
            console.error(err);
            console.log('unable to use hash-wasm library - are you using a really old browser?');
        }
        return stringified;
    }

    private trimToMaxSize() {
        if (this.entries.size <= CalculationResponseCache.maxEntries) {
            return;
        }
        const keysToDelete: Array<string> = new Array<string>();
        for (const key of this.entries.keys()) {
            keysToDelete.push(key);
            if (keysToDelete.length + CalculationResponseCache.maxEntries >= this.entries.size) {
                break;
            }
        }
        if (this.applicationService.debug) {
            console.log(`Deleting ${keysToDelete.length} key(s) from the calculation response cache `
                + `because it has ${this.entries.size} entries.`);
        }
        for (const key of keysToDelete) {
            this.entries.delete(key);
        }
    }

    private clearOldCacheEntries(): void {
        const keysToDelete: Array<string> = new Array<string>();
        const now: number = Date.now();
        for (const key of this.entries.keys()) {
            const entry: CalculationCacheEntry = this.entries.get(key);
            const expiryTimestamp: number = entry.timestamp + (CalculationResponseCache.maxAgeMinutes * 60 * 1000);
            if (now > expiryTimestamp) {
                keysToDelete.push(key);
            }
        }
        if (this.applicationService.debug) {
            console.log(`Deleting ${keysToDelete.length} key(s) from the calculation response cache `
                + `because they have expired.`);
        }
        for (const key of keysToDelete) {
            this.entries.delete(key);
        }
    }

    private getSecondsAgo(timestamp: number): number {
        const now: number = Date.now();
        const millisAgo: number = now - timestamp;
        return millisAgo / 1000;
    }
}
