import { IonicLifecycleEventReplayBus } from "@app/services/ionic-lifecycle-event-replay-bus";
import { Injector } from "@angular/core";

/**
 * Represents a page which has a master page, ie there is a route specifying that
 * a different page should be shown in the left hand pane when this page is shown in the
 * right hand pane.
 */
export interface PageWithMaster {
    injector: Injector;
    ionicLifecycleEventReplayBus: IonicLifecycleEventReplayBus;
}
