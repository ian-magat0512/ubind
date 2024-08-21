import { Injectable } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Alert } from '../models/alert';
import { EventService } from './event.service';
import { MessageService } from './message.service';

/**
 * Send alert or error messages to be displayed until dismissed.
 */
@Injectable()
export class AlertService {
    public updateSubject: Subject<Alert> = new Subject<Alert>();
    public visibleSubject: Subject<boolean> = new Subject<boolean>();

    public constructor(
        private messageService: MessageService,
        private eventService: EventService,
    ) {
    }

    public alert(alert: Alert): void {
        this.updateSubject.next(alert);
        this.sendMessageToLoader(alert);
    }

    public hide(): void {
        this.visibleSubject.next(false);
    }

    public show(): void {
        this.visibleSubject.next(true);
    }

    /**
     * If the app has not fully loaded then we need to direct the notification to the loader which
     * can display the message, otherwise it won't be seen.
     */
    private sendMessageToLoader(alert: Alert): void {
        let message: string = alert.message;
        if (alert.additionalDetails && alert.additionalDetails.length) {
            message += `. ${alert.additionalDetails.join('. ')}`;
        }

        let subscription: Subscription = this.eventService.webFormLoadedSubject
            .pipe(finalize(() => subscription.unsubscribe()))
            .subscribe((loaded: boolean) => {
                if (!loaded) {
                    let payload: any = {
                        'message': message,
                        'severity': 3,
                    };
                    this.messageService.sendMessage('displayMessage', payload);
                }
            });
    }

}
