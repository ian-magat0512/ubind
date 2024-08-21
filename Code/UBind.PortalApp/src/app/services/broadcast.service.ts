import { Observable, ReplaySubject, Subject } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { Injectable } from '@angular/core';

/**
 * Data to be broadcast
 */
interface BroadcastEvent {
    key: any;
    data?: any;
}

/**
 * An event
 */
interface Event {
    key: string;
    value: any;
}

/**
 * Export broadcast service class.
 * TODO: Write a better class header: broadcast events services.
 */
@Injectable({ providedIn: 'root' })
export class BroadcastService {

    private eventBus: ReplaySubject<BroadcastEvent> = new ReplaySubject<BroadcastEvent>();
    protected _eventsSubject: Subject<Event> = new Subject<Event>();

    public constructor() {

    }

    public broadcast(key: any, data?: any): void {

        this.eventBus.next({ key, data });
    }

    public on<T>(key: any): Observable<T> {

        return this.eventBus.asObservable()
            .pipe(
                filter((event: BroadcastEvent) => event.key == key),
                map((event: BroadcastEvent) => <T>event.data),
            );
    }

    /**
     * Newly implemented broadcasting methods, won't delete 
     * the first implementation as it is used by other components at the moment.
     * Will have to recheck every components first that uses it.
     */

    public dispatchEvent(key: string, value: any): void {
        this._eventsSubject.next({ key, value });
    }

    public getEvent<T>(key: string): Observable<T> {
        return this._eventsSubject.asObservable().pipe(
            filter((e: Event) => e.key === key),
            map((e: Event) => e.value),
        );
    }
}
