import { MemoryStorage } from './memory-storage';
import { Injectable } from '@angular/core';

/**
 * This class utilises 3 storage mechanisms
 * 1. Memory - stores data in a map. Works fine for Single Page Applications...until you reload
 * 2. SessionStorage - Keeps the data until you close the browser window.
 *    Session storage can be shared between iframes on the same domain.
 * 3. LocalStorage - Keeps the data even after you close the browser window which is great for long running sessions.
 *    Local storage can't be shared between iframes thought.
 *
 * If we can, we are going to write to all 3 so that our data can persist in the most challenging conditions...
 */
@Injectable({ providedIn: 'root' })
export class ResilientStorage implements Storage {

    private storages: Array<Storage> = new Array<Storage>();

    public constructor() {
        this.storages.push(new MemoryStorage());
        try {
            if (this.isSupported(window.localStorage)) {
                this.storages.push(window.localStorage);
            }
        } catch {
            console.log('window.localStorage is not supported. Trying window.sessionStorage.');
        }
        try {
            if (this.isSupported(window.sessionStorage)) {
                this.storages.push(window.sessionStorage);
            }
        } catch {
            console.log('window.sessionStorage is not supported. Falling back to memory storage.');
        }
    }

    public length: number = 0;

    public clear(): void {
        this.length = 0;
        this.storages.forEach((s: Storage) => s.clear());
    }

    public getItem(key: string): string {
        for (let s of this.storages) {
            let value: string = s.length > 0 ? s.getItem(key) : null;
            if (value) {
                return value;
            }
        }
        return null;
    }

    public key(index: number): string {
        for (let s of this.storages) {
            let key: string = s.key(index);
            if (key) {
                return key;
            }
        }
        return null;
    }

    public removeItem(key: string): void {
        let found: boolean = false;
        this.storages.forEach((s: Storage) => {
            let value: string = s.getItem(key);
            if (value) {
                found = true;
                s.removeItem(key);
            }
        });
        if (found) {
            this.length--;
        }
    }

    public setItem(key: string, value: string): void {
        let added: boolean = false;
        if (value == undefined) {
            value = null;
        }
        this.storages.forEach((s: Storage) => {
            if (typeof value === 'undefined') {
                s.removeItem(key);
            } else {
                s.setItem(key, value);
                added = true;
            }
        });
        if (added) {
            this.length++;
        }
    }

    private isSupported(storage: Storage): boolean {
        try {
            return storage != null && storage != undefined;
        } catch {
            return false;
        }
    }
}
