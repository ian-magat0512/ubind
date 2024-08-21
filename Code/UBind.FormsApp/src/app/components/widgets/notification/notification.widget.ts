import { takeUntil } from 'rxjs/operators';
import { animate, style, transition, trigger } from '@angular/animations';
import { Component, ElementRef, HostListener, OnInit } from '@angular/core';
import { Widget } from '../widget';
import { ToastLikeNotification } from '@app/models/toast-like-notification';
import { NotificationService } from '@app/services/notification.service';
import { AnimationEvent } from '@angular/animations';
import { SidebarOffsetService } from '@app/services/sidebar-offset.service';

/**
 * Displays notifications at the top of the screen
 */
@Component({
    selector: 'notification-widget',
    templateUrl: './notification.widget.html',
    animations: [
        trigger('notificationAnimation', [
            transition('void => *', [
                style({ transform: 'translateY(-100%)' }),
                animate('200ms ease-in', style({ transform: 'translateY(0%)' }))            ]),
            transition('* => void', [
                animate('200ms ease-in', style({ transform: 'translateY(-100%)' })),
            ]),
        ]),
    ],
    styleUrls: [
        './notification.widget.scss',
    ],
})
export class NotificationWidget extends Widget implements OnInit {

    public notifications: Array<ToastLikeNotification> = new Array<ToastLikeNotification>();
    private notificationWidgetElement: HTMLElement;
    private fadeDurationMillis: number = 100;
    private minimumDisplayTimeMillis: number = 500;

    public constructor(
        private elementRef: ElementRef,
        private notificationService: NotificationService,
        private sidebarOffsetService: SidebarOffsetService,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.notificationWidgetElement = this.elementRef.nativeElement;
        this.notificationService.notificationSubject
            .pipe(takeUntil(this.destroyed))
            .subscribe((notification: ToastLikeNotification) => {
                this.notify(notification);
            });

        /* For testing >>>
        let count: number = 0;
        setInterval(() => {
            count++;
            let notification: ToastLikeNotification = {
                message: `This is notification ${count}`
            };
            this.notify(notification);
        }, 2000);
        */
    }

    @HostListener("window:message", ['$event'])
    public onMessage(event: any): void {
        if (event.data.messageType == 'scroll' || event.data.messageType == 'resize') {
            let offsetToTop: number = this.sidebarOffsetService.getOffsetTop();
            this.notificationWidgetElement.style.top =
                event.data.payload.verticalScrollAmountPixels + offsetToTop + 'px';
        }
    }

    public async notify(notification: ToastLikeNotification): Promise<void> {
        notification.expireAfterMillis = notification.expireAfterMillis || 3000;
        this.notifications.push(notification);
        if (this.notifications.length > 1) {
            let previousNotification: ToastLikeNotification = this.notifications[this.notifications.length - 2];
            if (previousNotification && previousNotification.expireUponNextNotification) {
                if (previousNotification.active) {
                    await this.expireNotification(previousNotification);
                } else {
                    previousNotification.expireAfterMillis = this.minimumDisplayTimeMillis;
                }
            }
        } else {
            this.showNextNotification();
        }
    }

    private showNextNotification(): void {
        let notification: ToastLikeNotification
            = this.notifications.filter((n: ToastLikeNotification) => !n.expired)[0];
        notification.active = true;
        notification.startTimeMillis = performance.now();
        setTimeout(() => this.expireNotification(notification), notification.expireAfterMillis);
    }

    public onCompletedTransition($event: AnimationEvent): void {
        if ($event.toState == 'void') {
            if (this.notifications.filter((n: ToastLikeNotification) => !n.expired).length > 0) {
                this.showNextNotification();
            }
        }
    }

    private async expireNotification(notification: ToastLikeNotification): Promise<void> {
        const now: number = performance.now();
        const displayTimeMillis: number = now - notification.startTimeMillis;
        const additionalTimeMillis: number = Math.max(0, this.minimumDisplayTimeMillis - displayTimeMillis);
        if (additionalTimeMillis) {
            setTimeout(async () => {
                await this.expireNotificationNow(notification);
            }, additionalTimeMillis);
        } else {
            await this.expireNotificationNow(notification);
        }
    }

    private async expireNotificationNow(notification: ToastLikeNotification): Promise<void> {
        return new Promise((resolve: any, reject: any): void => {
            notification.animatingOut = true;
            notification.active = false;
            notification.expired = true;
            setTimeout(() => {
                notification.animatingOut = false;
                this.removeExpiredNotifications();
                resolve();
            }, this.fadeDurationMillis + 100);
        });
    }

    private removeExpiredNotifications(): void {
        for (let index: number = 0; index < this.notifications.length; index++) {
            let notification: ToastLikeNotification = this.notifications[index];
            if (notification.expired && !notification.animatingOut) {
                this.notifications.splice(index, 1);
                index--;
            }
        }
    }
}
