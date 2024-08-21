import { Component, ElementRef, Injector } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';

/**
 * Export blank page component class
 * TODO: Write a better class header: blank page.
 */
@Component({
    selector: 'app-blank',
    templateUrl: './blank.page.html',
})
export class BlankPage extends DetailPage {
    public message: string;

    public constructor(
        public route: ActivatedRoute,
        protected router: Router,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(eventService, elementRef, injector);
    }
}
