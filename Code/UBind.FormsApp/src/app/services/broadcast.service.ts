import { Observable, ReplaySubject } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";

/**
 * An event that can be broadcast and listened to.
 */
interface BroadcastEvent {
    key: any;
    data?: any;
}

/**
 * Export broadcast service class.
 * TODO: Write a better class header: broadcast service functions.
 */
@Injectable()
export class BroadcastService {

    public eventBus: ReplaySubject<BroadcastEvent> = new ReplaySubject<BroadcastEvent>();

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
}
