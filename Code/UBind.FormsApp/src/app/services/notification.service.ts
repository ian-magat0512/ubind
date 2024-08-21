import { Injectable } from '@angular/core';
import { ToastLikeNotification } from '@app/models/toast-like-notification';
import { Subject } from 'rxjs';

/**
 * Sends toast like notifications to be displayed for a given time period
 */
@Injectable()
export class NotificationService {
    public notificationSubject: Subject<ToastLikeNotification> = new Subject<ToastLikeNotification>();

    public constructor(
    ) {
    }

    public notify(notification: ToastLikeNotification): void {
        this.notificationSubject.next(notification);
    }
}
