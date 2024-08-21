import { ResilientStorage } from './resilient-storage';

describe('ResilientStorage', () => {

    beforeEach(() => {
        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.clear();
    });

    it('should have a length of 1 when a value is added', () => {
        const key: string = 'id';
        const value: string = '2020';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);

        expect(resilientStorage.length).toBe(1);
    });

    it('should have a length of 1 when a value is added', () => {
        const key: string = 'id';
        const value: string = '2020';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);

        expect(resilientStorage.length).toBe(1);
    });

    it('should return a value from resilientStorage if supported.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);

        expect(resilientStorage.getItem(key)).toBe(value);
    });

    it('should return null when the resilientStorage item is removed.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);
        resilientStorage.removeItem(key);

        expect(resilientStorage.getItem(key)).toBe(null);
    });

    it('should return the key for the selected resilientStorage index.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);

        expect(resilientStorage.key(0)).toBe(key);
    });

    it('should return 0 length if the storage is cleared.', () => {
        const key: string = 'id';
        const value: string = '2020';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);
        resilientStorage.clear();

        expect(resilientStorage.length).toBe(0);
    });

    it('should write a value to localStorage.', () => {
        const key: string = 'id';
        const value: string = '2021';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);

        expect(window.localStorage.getItem(key)).toBe(value);
    });

    it('should write a value to sessionStorage.', () => {
        const key: string = 'id';
        const value: string = '2022';

        const resilientStorage: ResilientStorage = new ResilientStorage();
        resilientStorage.setItem(key, value);

        expect(window.sessionStorage.getItem(key)).toBe(value);
    });
});
