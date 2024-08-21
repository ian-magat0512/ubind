import {
    Component, ViewChild, ViewContainerRef, ComponentRef,
    ComponentFactoryResolver, OnInit, OnDestroy,
    ViewEncapsulation, Injector, ElementRef, ViewRef,
} from "@angular/core";
import { ListNoSelectionPage } from "@app/pages/list-no-selection/list-no-selection.page";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { ActivatedRoute, ActivatedRouteSnapshot } from "@angular/router";
import { Subject } from "rxjs";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { EventService } from "@app/services/event.service";
import { RouteHelper } from "@app/helpers/route.helper";
import { filter, takeUntil } from "rxjs/operators";

/**
 * This component essentially wraps a child component and displays it, 
 * however it selects the child component to display depending upon the current layout.
 * If we are in "split screen" mode, where there is a master and a detail component, 
 * then it will show the detail component as per normal. 
 * However if we are not showing the split screen (e.g. because we have small screen, like on a mobile phone), 
 * then instead of showing the detail component, 
 * we'll actually show the master component instead. This component therefore maintains references 
 * to both the detail and the master component, 
 * and switches which one to show, based upon the layout.
 * 
 * This is useful in the following situations:
 * 1. A list view where there is no item selected, so the detail (right pane) 
 * would be blank when there is a split screen, 
 * but when it's not a slit screen you don't want to show a blank screen, so you show the master 
 * component (which is the list) instead.
 * 2. A screen which doesn't have a master (e.g. manage my account). In this case you want to show 
 * the manage my account screen in the left pane 
 * (as the master) and a blank screen in the right pane (detail), and when the screen resizes to 
 * mobile size, you want to switch away from showing a 
 * blank screen to showing the manage my account screen (the master) in the detail view.
 * 
 * How to use:
 * The components to show and how they are laid out must be configured in the routes.
 * 
 * Example route for a list view:
 * {
 *     path: 'list',
 *     component: ShowMasterComponentWhenNotSplit,
 *     data: {
 *         masterComponent: ListQuotePage,
 *         detailComponent: ListNoSelectionPage,
 *         noSelectionMessage: 'Select a quote to view details'
 *     }
 * }
 *
 * Example route for a screen which doesn't have a master:
 * {
 *     path: "",
 *     component: ShowMasterComponentWhenNotSplit,
 *     data: {
 *         masterComponent: DetailAccountPage,
 *         detailComponent: BlankPage
 *     }
 * },
 *
 * Note: These examples do not include things like route guards and permissions. Please add them as appropriate.
 */
@Component({
    selector: 'show-master-when-not-split',
    template: `<ng-container #target></ng-container>`,
    styles: [`
        show-master-when-not-split > * {
            height: 100%;
        }
    `],
    encapsulation: ViewEncapsulation.None,
})
export class ShowMasterComponentWhenNotSplit extends DetailPage implements OnInit, OnDestroy {
    public splitActive: boolean = false;
    private activeComponent: any;
    private createdViews: Map<any, ViewRef> = new Map<any, ViewRef>();
    private createdComponents: Map<any, ComponentRef<any>> = new Map<any, ComponentRef<any>>();

    @ViewChild('target', { read: ViewContainerRef, static: true }) public target: ViewContainerRef;
    private componentRef: ComponentRef<any>;

    public constructor(
        private resolver: ComponentFactoryResolver,
        private layoutManager: LayoutManagerService,
        private route: ActivatedRoute,
        eventService: EventService,
        injector: Injector,
        elementRef: ElementRef,
        private routeHelper: RouteHelper,
    ) {
        super(eventService, elementRef, injector);
    }

    private onSplitActiveChangeSwitchComponent(): void {
        this.layoutManager.splitPaneEnabledSubject.pipe(
            takeUntil(this.destroyed),
            filter((splitActive: boolean) => splitActive != this.splitActive),
        ).subscribe((splitActive: boolean) => {
            this.splitActive = splitActive;
            setTimeout(() => {
                this.resolveComponent();
            }, 0);
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.splitActive = this.layoutManager.splitPaneEnabled;
        this.onSplitActiveChangeSwitchComponent();
        setTimeout(() => {
            this.resolveComponent();
        }, 0);
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
        this.target.clear();
        if (this.componentRef) {
            this.componentRef.destroy();
        }
        this.createdViews.forEach((viewRef: ViewRef) => viewRef.destroy());
        this.createdComponents.clear();
    }

    private resolveComponent(): void {
        if (!this.routeHelper.navigationInProgress && this.layoutManager.currentMasterComponent) {
            let componentToActivate: any = this.splitActive
                ? this.getDynamicDetailComponent()
                : this.route.snapshot.data.masterComponent;
            if (componentToActivate != null && componentToActivate != this.activeComponent) {
                const existingViewRef: ViewRef = this.createdViews.get(componentToActivate);
                if (existingViewRef) {
                    this.target.insert(existingViewRef, 0);
                    this.target.detach(1);
                } else {
                    let componentFactory: any = this.resolver.resolveComponentFactory(componentToActivate);
                    let viewRef: ViewRef = this.target.get(0);
                    if (viewRef) {
                        this.target.detach(0);
                    }
                    this.componentRef = this.target.createComponent(componentFactory);
                    this.detectWhenNgOnInitCalled(this.componentRef);
                    viewRef = this.target.get(0);
                    this.createdViews.set(componentToActivate, viewRef);
                    this.createdComponents.set(componentToActivate, this.componentRef);

                    // manually trigger the ionic lifecycle hooks since they won't be triggered by ionic
                    // do this in a separate thread to ensure ngOnInit is called first
                    setTimeout(async () => {
                        // sometimes ngOnInit is not called (e.g. during resizing something goes wrong)
                        // this ensures it's called before we call the ionic functions.
                        if (this.componentRef['ngOnInitCalled'] === false) {
                            this.componentRef.instance.ngOnInit();
                        }
                    }, 0);
                }
                this.activeComponent = componentToActivate;
            }
        }
    }

    private getDynamicDetailComponent(): any {
        let snapshot: ActivatedRouteSnapshot = this.route.snapshot;
        return snapshot.data.detailComponent
            ? snapshot.data.detailComponent
            : ListNoSelectionPage;
    }

    private detectWhenNgOnInitCalled(componentRef: ComponentRef<any>): void {
        componentRef['ngOnInitCalled'] = false;
        let originalNgOnInit: any = componentRef.instance.ngOnInit;
        componentRef.instance.ngOnInit = ((): any => {
            if (originalNgOnInit) {
                componentRef['ngOnInitCalled'] = true;
                return originalNgOnInit.bind(componentRef.instance)();
            }
        });
    }
}
