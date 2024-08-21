import { Injectable } from '@angular/core';

/**
 * The cache entry to be cached.
 */
interface CacheEntry {
    data: any;
    createdTimeStamp: number;
}

/**
 * This is a Generic Cache Service that is used to cache data and expires cache with in a given max age.
 */
@Injectable({
    providedIn: 'root',
})
export class GenericCache  {
    private cacheEntries: Map<string, CacheEntry> = new Map<string, CacheEntry>();

    public constructor() {
    }

    public getCachedDataOrNull(cacheKey: string, maxAgeSeconds: number): any | null {
        let cachedData: CacheEntry = this.cacheEntries.get(cacheKey);

        if (cachedData == null) {
            return null;
        }

        const cacheExpiry: number = cachedData.createdTimeStamp + maxAgeSeconds * 1000;
        const currentTime: number = Date.now();
        const isCacheExpired: boolean = currentTime > cacheExpiry;

        if (!isCacheExpired) {
            return cachedData.data;
        }
        return null;
    }

    public setCacheEntry(cacheKey: string, data: any): void {
        this.removeCacheEntry(cacheKey);
        const createdTimeStamp: number = Date.now();
        const cachedData: CacheEntry = {
            data: data,
            createdTimeStamp: createdTimeStamp,
        };
        this.cacheEntries.set(cacheKey, cachedData);
    }

    public removeCacheEntry(cacheKey: string): void {
        if (this.cacheEntries.get(cacheKey)) {
            this.cacheEntries.delete(cacheKey);
        }
    }
}
