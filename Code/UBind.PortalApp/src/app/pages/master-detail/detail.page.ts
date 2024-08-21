import { AfterViewInit, Directive, ElementRef, Injector } from "@angular/core";
import { IonicLifecycleEventReplayBus } from "@app/services/ionic-lifecycle-event-replay-bus";
import { PageWithMaster } from "./page-with-master";
import { EventService } from "@app/services/event.service";
import { Subject } from "rxjs";
import { IonicHelper } from "@app/helpers/ionic.helper";
import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Represents a detail component, or more accurately, a component which may have 
 * an associated master component when in split screen mode
 */
@Directive({ selector: '[appDetail]' })
export abstract class DetailPage implements AfterViewInit, PageWithMaster {
    public isLoading: boolean = true;
    public errorMessage: string;
    protected destroyed: Subject<void>;
    public ionicLifecycleEventReplayBus: IonicLifecycleEventReplayBus;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
    ) {
        this.ionicLifecycleEventReplayBus = new IonicLifecycleEventReplayBus(elementRef);
        eventService.detailComponentCreated(this);
    }

    public ngAfterViewInit(): void {
        IonicHelper.initIonSegmentButtons();
    }
}
