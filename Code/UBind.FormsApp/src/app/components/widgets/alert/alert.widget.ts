import {
    Component, OnInit, Input, Output,
    EventEmitter, HostListener, AfterViewInit, ViewChild, ElementRef, OnDestroy, DoCheck,
} from '@angular/core';
import { trigger, style, animate, transition } from '@angular/animations';
import { Alert } from '@app/models/alert';
import { AlertService } from '@app/services/alert.service';
import { fromEvent, Subject } from 'rxjs';
import { debounceTime, filter, takeUntil } from 'rxjs/operators';
import { EventService } from '@app/services/event.service';

/**
 * The alert widget displays "popup" style alert messages, typically when an error occurs.
 */
@Component({
    selector: 'alert-widget',
    templateUrl: './alert.widget.html',
    animations: [
        trigger('dialog', [
            transition('void => *', [
                style({ transform: 'scale3d(.3, .3, .3)' }),
                animate(100),
            ]),
            transition('* => void', [
                animate(100, style({ transform: 'scale3d(.0, .0, .0)' })),
            ]),
        ]),
    ],
    styleUrls: [
        './alert.widget.scss',
    ],
})
export class AlertWidget implements OnInit, OnDestroy, AfterViewInit, DoCheck {
    @Input() public closable: boolean = true;
    @Input() public visible: boolean = false;
    @Output() public visibleChange: EventEmitter<boolean> = new EventEmitter<boolean>();

    @ViewChild('dialog') public dialog: ElementRef;

    public alert: Alert;

    private destroyed: Subject<void> = new Subject<void>();

    /**
     * This is used to ensure no console errors are output during certain unit tests, which causes them to be
     * picked up as a failure by our tester running in bamboo.
     */
    public static suppressErrors: boolean = false;

    public constructor(
        private alertService: AlertService,
        private eventService: EventService,
    ) {
    }

    public ngOnInit(): void {
        this.alertService.updateSubject.subscribe(this.update.bind(this));
        this.alertService.visibleSubject.subscribe(this.setVisible.bind(this));
    }

    public ngOnDestroy(): void {
        this.destroyed?.next();
        this.destroyed?.complete();
    }

    public ngAfterViewInit(): void {
        fromEvent(window, 'message')
            .pipe(
                filter((event: any) => event.data.messageType == 'scroll'),
                debounceTime(200),
                takeUntil(this.destroyed),
            )
            .subscribe((event: any) => {
                this.dialog.nativeElement.style.top =
                    (Math.max(event.data.payload.verticalScrollAmountPixels, 0) + 50)
                    + 'px';
            });
    }

    public ngDoCheck(): void {
        if (this.visible) {
            this.publishAlertBottom();
        }
    }

    public publishAlertBottom(): void {
        const bounds: DOMRect = this.dialog.nativeElement.getBoundingClientRect() as DOMRect;
        this.eventService.alertBottomSubject.next(bounds.bottom);
    }

    public update(alert: Alert): void {
        if (alert.isValid()) {
            this.alert = alert;
            this.visible = true;
            this.visibleChange.emit(this.visible);
        } else if (!AlertWidget.suppressErrors) {
            console.error("We were trying to display an alert, but it didn't have the required properties.");
            console.error(alert);
        }
    }

    @HostListener('document:keydown.esc', ['$event'])
    public onEscapeKeydownHandler(): void {
        this.close();
    }

    public setVisible(visible: boolean): void {
        this.visible = visible;
    }

    public close(): void {
        this.visible = false;
        this.visibleChange.emit(this.visible);
        this.eventService.alertBottomSubject.next(0);
    }
}
