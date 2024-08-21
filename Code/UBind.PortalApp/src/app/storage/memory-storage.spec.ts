import { MemoryStorage } from './memory-storage';

describe('MemoryStorage', () => {
    beforeEach(() => {
        const memoryStorage: MemoryStorage = new MemoryStorage();
        memoryStorage.clear();
    });

    it('should have a length of 1 when a value is added', () => {
        const key: string = 'id';
        const value: string = '2020';

        const memoryStorage: MemoryStorage = new MemoryStorage();
        memoryStorage.setItem(key, value);

        expect(memoryStorage.length).toBe(1);
    });

    it('should have a length of 1 when a value is added', () => {
        const key: string = 'id';
        const value: string = '2020';

        const memoryStorage: MemoryStorage = new MemoryStorage();
        memoryStorage.setItem(key, value);

        expect(memoryStorage.length).toBe(1);
    });

    it('should return a value from memoryStorage if supported.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const memoryStorage: MemoryStorage = new MemoryStorage();
        memoryStorage.setItem(key, value);

        expect(memoryStorage.getItem(key)).toBe(value);
    });

    it('should return undefined when the memoryStorage item is removed.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const memoryStorage: MemoryStorage = new MemoryStorage();
        memoryStorage.setItem(key, value);
        memoryStorage.removeItem(key);

        expect(memoryStorage.getItem(key)).toBe(undefined);
    });

    it('should return the key for the selected memoryStorage index.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const memoryStorage: MemoryStorage = new MemoryStorage();
        memoryStorage.setItem(key, value);

        expect(memoryStorage.key(0)).toBe(key);
    });

    it('should return 0 length if the storage is cleared.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const memoryStorage: MemoryStorage = new MemoryStorage();
        memoryStorage.setItem(key, value);
        memoryStorage.clear();

        expect(memoryStorage.length).toBe(0);
    });
});
