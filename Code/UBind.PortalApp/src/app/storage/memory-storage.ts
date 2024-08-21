/**
 * An in memory map which implements the Storage interface used by browsers for browser storage.
 */
export class MemoryStorage implements Storage {
    private static storageMap: any = new Map<string, any>();
    public length: number = 0;

    public clear(): void {
        MemoryStorage.storageMap.clear();
        this.length = 0;
    }

    public getItem(key: string): string {
        let value: string = MemoryStorage.storageMap.get(key);
        return value;
    }

    public key(index: number): string {
        let keys: Array<string> = Array.from(MemoryStorage.storageMap.keys());
        return keys[index];
    }

    public removeItem(key: string): void {
        if (MemoryStorage.storageMap.has(key)) {
            MemoryStorage.storageMap.delete(key);
            this.length--;
        }
    }

    public setItem(key: string, value: string): void {
        if (typeof value === 'undefined') {
            this.removeItem(key);
        } else {
            if (!(Object.prototype.hasOwnProperty.call(MemoryStorage.storageMap, key))) {
                this.length++;
            }
            MemoryStorage.storageMap.set(key, '' + value);
        }
    }
}
