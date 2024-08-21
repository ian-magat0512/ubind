import { ApplicationService } from "@app/services/application.service";
import { EventService } from "@app/services/event.service";
import { CalculationCacheEntry, CalculationResponseCache } from "./calculation-response-cache";

describe('CalculationResponseCache', () => {
    let applicationService: ApplicationService;
    let eventService: EventService;
    let cache: CalculationResponseCache;


    beforeEach(() => {
        applicationService = new ApplicationService();
        eventService = new EventService();
        cache = new CalculationResponseCache(applicationService, eventService);
    });

    it('should expire cache entries that are old', async () => {
        // Arrange
        const entries: Map<string, CalculationCacheEntry> = (cache as any).entries;
        const elevenMinutes: number = 11 * 60 * 1000;
        await cache.addSuccessfulResponse({ asdf: 'qwer' }, { zxcv: 'poiu' }, Date.now() - elevenMinutes);
        expect(entries.size).toBe(1);

        // Act
        (cache as any).clearOldCacheEntries();

        // Assert
        expect(entries.size).toBe(0);
    });

    it('should not store more than N entries', async () => {
        // Arrange
        CalculationResponseCache.maxEntries = 10;
        const entries: Map<string, CalculationCacheEntry> = (cache as any).entries;
        await cache.addSuccessfulResponse({ asdf: '0' }, { zxcv: '0' });
        await cache.addSuccessfulResponse({ asdf: '1' }, { zxcv: '1' });
        await cache.addSuccessfulResponse({ asdf: '2' }, { zxcv: '2' });
        await cache.addSuccessfulResponse({ asdf: '3' }, { zxcv: '3' });
        await cache.addSuccessfulResponse({ asdf: '4' }, { zxcv: '4' });
        await cache.addSuccessfulResponse({ asdf: '5' }, { zxcv: '5' });
        await cache.addSuccessfulResponse({ asdf: '6' }, { zxcv: '6' });
        await cache.addSuccessfulResponse({ asdf: '7' }, { zxcv: '7' });
        await cache.addSuccessfulResponse({ asdf: '8' }, { zxcv: '8' });
        await cache.addSuccessfulResponse({ asdf: '9' }, { zxcv: '9' });
        expect(entries.size).toBe(10);

        // Act
        await cache.addSuccessfulResponse({ asdf: 'a' }, { zxcv: 'a' });

        // Assert
        expect(entries.size).toBe(10);
        const firstEntryIterator: IteratorResult<[string, CalculationCacheEntry]> = entries.entries().next();
        const entry: CalculationCacheEntry = firstEntryIterator.value[1];
        expect(entry.responsePayload).toEqual({ zxcv: '1' });
    });

    it('should store the latest calculation result last even when it already exists', async () => {
        // Arrange
        CalculationResponseCache.maxEntries = 10;
        const entries: Map<string, CalculationCacheEntry> = (cache as any).entries;
        await cache.addSuccessfulResponse({ asdf: '0' }, { zxcv: '0' });
        await cache.addSuccessfulResponse({ asdf: '1' }, { zxcv: '1' });
        await cache.addSuccessfulResponse({ asdf: '2' }, { zxcv: '2' });
        expect(entries.size).toBe(3);

        // Act
        await cache.addSuccessfulResponse({ asdf: '0' }, { zxcv: '0' });

        // Assert
        expect(entries.size).toBe(3);
        let index: number = 0;
        for (const [, value] of entries) {
            switch (index) {
                case 0:
                    expect(value.responsePayload).toEqual({ zxcv: '1' });
                    break;
                case 1:
                    expect(value.responsePayload).toEqual({ zxcv: '2' });
                    break;
                case 2:
                    expect(value.responsePayload).toEqual({ zxcv: '0' });
                    break;
            }
            index++;
        }
    });
});
