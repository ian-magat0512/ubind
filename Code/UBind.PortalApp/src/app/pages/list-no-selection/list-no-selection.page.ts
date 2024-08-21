import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import {
    ActivatedRoute, Router,
    ActivationEnd, ActivatedRouteSnapshot,
} from '@angular/router';
import { filter, takeUntil, map } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Export list no selection page component class
 * TODO: Write a better class header: no selection list page.
 */
@Component({
    selector: 'app-list-no-selection',
    templateUrl: './list-no-selection.page.html',
})
export class ListNoSelectionPage extends DetailPage implements OnInit, OnDestroy {
    public message: string;

    public constructor(
        public route: ActivatedRoute,
        protected router: Router,
        protected authService: AuthenticationService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.onNavigationUpdateItemName();
        this.updateItemName(this.route.snapshot);
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private onNavigationUpdateItemName(): void {
        this.router.events.pipe(
            filter((event: any) => event instanceof ActivationEnd),
            map((event: any) => (<ActivationEnd>event).snapshot),
            takeUntil(this.destroyed),
        )
            .subscribe((snapshot: ActivatedRouteSnapshot) => {
                this.updateItemName(snapshot);
            });
    }

    private updateItemName(snapshot: ActivatedRouteSnapshot): void {
        if (snapshot.data.noSelectionMessage) {
            this.message = this.authService.isMutualTenant() ?
                snapshot.data.noSelectionMessage.replace('policy', 'protection') :
                snapshot.data.noSelectionMessage;
        }
    }
}
