import {
    Injectable, ViewContainerRef, ComponentFactoryResolver,
    ComponentRef, ComponentFactory, Injector,
} from '@angular/core';
import { IonRouterOutlet } from '@ionic/angular';
import { Router, ActivationStart, ActivatedRouteSnapshot } from '@angular/router';
import { filter, map } from 'rxjs/operators';
import { AuthenticationService } from './authentication.service';
import { Subject, SubscriptionLike } from 'rxjs';
import { SplitLayoutManager } from '../models/split-layout-manager';
import { ComponentLifecycleHelper } from '@app/helpers/component-lifecycle-helper';
import { PageWithMaster } from '@app/pages/master-detail/page-with-master';
import { Errors } from '@app/models/errors';
import { BlankPage } from '@app/pages/blank/blank.page';
import { EventService } from './event.service';
import { WindowSize } from '@app/models/window-size.enum';
import { MenuState } from '@app/models/menu-state.enum';
import { TypedRouteData } from '@app/routing/typed-route-data';

/**
 * The layout options
 */
export enum ScreenLayoutSetting {
    /**
     * Split screen layout divides the main view area into two parts to allow master and detail views
     */
    Split,

    /**
     * Single means the main view area only has the one component within.
     */
    Single,
}

export enum MenuLayoutSetting {
    Collapsed,
    Expanded,
}

export enum SplitState {
    Active = 'active',
    Inactive = 'inactive',
}

/**
 * Manages the layout by responding to navigation events and determining which component is shown where, 
 * including in master detail split screen situations.
 */
@Injectable({ providedIn: 'root' })
export class LayoutManagerService implements SplitLayoutManager {

    private masterView: ViewContainerRef;
    private detailNav: IonRouterOutlet;
    private _currentMasterComponent: any;
    public windowSize: WindowSize;
    private screenLayoutSetting: ScreenLayoutSetting;
    private menuLayoutSetting: MenuLayoutSetting = MenuLayoutSetting.Expanded;
    private _menuDisabled: boolean = true;
    public splitPaneEnabledSubject: Subject<boolean> = new Subject<boolean>();

    /**
     * If the split pane is to be shown, it's considered enabled. However it may not yet be shown due to
     * a running animation.
     */
    public splitPaneEnabled: boolean;

    /**
     * If the split pane is visible (meaning animations have finished) and it's enabled
     */
    public splitPaneVisible: boolean;

    public previousSplitPaneEnabled: boolean;
    public lastRouteSnapshot: ActivatedRouteSnapshot;
    private masterComponentRef: ComponentRef<any>;
    private subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public expectingtoLoadMasterComponent: boolean = false;
    public splitState: string;
    public splitPaneAnimatingOut: boolean = false;
    public masterContainerClass: string;

    public constructor(
        private router: Router,
        private authenticationService: AuthenticationService,
        private resolver: ComponentFactoryResolver,
        private eventService: EventService,
    ) {
        this.onNavigationAssignMasterComponent();
        this.onDetailCreatedLoadMasterComponent();
        this.onUserLogoutDestroyMasterComponent();
        this.onAuthenticationStateChangedUpdateMenuState();
    }

    /**
     * Initialises this class with the required references to manage layout
     */
    public init(masterView: ViewContainerRef, detailNav: IonRouterOutlet): void {
        this.masterView = masterView;
        this.detailNav = detailNav;
        this.loadBlankMasterComponent();
        this.updateWindowSize();
    }

    public get WindowSize(): WindowSize {
        return this.windowSize;
    }

    public get Layout(): ScreenLayoutSetting {
        return this.screenLayoutSetting;
    }

    private get menuDisabled(): boolean {
        return this._menuDisabled;
    }

    private set menuDisabled(value: boolean) {
        const oldValue: boolean = this._menuDisabled;
        this._menuDisabled = value;
        if (oldValue != value) {
            this.eventService.menuStateChanged(this.getMenuState());
        }
    }

    /**
     * Listens to navigation events and assigns the master component to 
     * the masterNav when it's specified for that route.
     */
    private onNavigationAssignMasterComponent(): void {
        this.subscriptions.push(this.router.events.pipe(
            filter((event: any) => event instanceof ActivationStart),
            map((event: any) => (<ActivationStart>event).snapshot),
        ).subscribe((snapshot: ActivatedRouteSnapshot) => {
            if (!snapshot.routeConfig.redirectTo && !snapshot.routeConfig.loadChildren) {
                let currentRouteIdentifier: any = this.lastRouteSnapshot
                        ? this.lastRouteSnapshot.data.routeIdentifier : null;
                let data: TypedRouteData = snapshot.data;
                this.screenLayoutSetting = data.layout ? data.layout : data.masterComponent ?
                        ScreenLayoutSetting.Split : ScreenLayoutSetting.Single;
                this.menuLayoutSetting = data.initialMenuState ? data.initialMenuState : this.menuLayoutSetting;
                this.menuDisabled = data.menuDisabled ? data.menuDisabled : false;
                this.masterContainerClass = data.masterContainerClass;
                if (data.masterComponent) {
                    if (this._currentMasterComponent != data.masterComponent
                        || currentRouteIdentifier != data.routeIdentifier
                    ) {
                        this.expectingtoLoadMasterComponent = true;
                    }
                }
                this._currentMasterComponent = data.masterComponent;
                this.lastRouteSnapshot = snapshot;
            }
        }));
    }

    private onUserLogoutDestroyMasterComponent(): void {
        this.subscriptions.push(this.eventService.userLogoutSubject$.subscribe(() => {
            if (this.masterView.length) {
                this.masterView.clear();
            }
        }));
    }

    private onDetailCreatedLoadMasterComponent(): void {
        this.eventService.detailComponentCreatedSubject$
            .pipe(filter((page: PageWithMaster) => this.expectingtoLoadMasterComponent))
            .subscribe((page: PageWithMaster) => {
                this.expectingtoLoadMasterComponent = false;
                this.loadMasterComponentForDetailComponent(page);
            });
    }

    public onRouteActivate($event: any): void {
        let component: any = <any>$event;
        if (this.expectingtoLoadMasterComponent) {
            this.expectingtoLoadMasterComponent = false;
            if (this.instanceOfComponentWithMaster(component)) {
                this.loadMasterComponentForDetailComponent(component);
            } else {
                throw Errors.General.Unexpected(
                    "This route is configured to have a master detail split screen setup. "
                    + "In order for this to work the component \"" + component.constructor.name + "\" needs "
                    + "to implement the interface \"ComponentWithMaster\", however it doesn't. "
                    + "Please edit the class \"" + component.constructor.name + "\" to make "
                    + "it extend \"DetailComponent\", or implement \"ComponentWithMaster\" directly.",
                );
            }
        } else {
            this.checkForSplitActiveChange();
        }
    }

    private instanceOfComponentWithMaster(object: any): object is PageWithMaster {
        return 'injector' in object
            && 'ionicLifecycleEventReplayBus' in object;
    }

    /**
     * Loads the master component when the current layout calls for a master/detail view.
     * The route parameters define whether the current layout should havea master/detail view.
     * @param injector an instance of the Injector for the current module.
     * @param detailComponent the detail component which is initiating the load
     */
    private loadMasterComponentForDetailComponent(detailComponent: PageWithMaster): void {
        let component: any = this._currentMasterComponent;
        if (component) {
            const componentFactoryResolver: any = detailComponent.injector.get(ComponentFactoryResolver);
            const componentFactory: any = componentFactoryResolver.resolveComponentFactory(component);
            const reflectiveInjector: Injector = Injector.create({
                providers: [{ provide: component, useValue: component }],
                parent: detailComponent.injector,
            });
            if (this.masterView.length > 1) {
                this.masterView.remove(1);
            }
            this.masterComponentRef = this.masterView.createComponent(componentFactory, 0, reflectiveInjector);
            ComponentLifecycleHelper.waitForNgOnInit(this.masterComponentRef.instance).then(() => {
                this.checkForSplitActiveChange();
            });
        }
    }

    private loadBlankMasterComponent(): void {
        let componentFactory: ComponentFactory<BlankPage> = this.resolver.resolveComponentFactory(BlankPage);
        this.masterView.createComponent(componentFactory);
    }

    public onDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
        if (this.masterView) {
            this.masterView.clear();
        }
        if (this.masterComponentRef) {
            this.masterComponentRef.destroy();
        }
    }

    public get currentMasterComponent(): any {
        return this._currentMasterComponent;
    }

    /**
     * Returns true when the split pane is to be shown, or false when there is to be no split
     */
    public shouldShowSplit(): boolean {
        if (this.detailNav.isActivated && this.detailNav.component
            && this.isSplitLayoutManager(this.detailNav.component)
        ) {
            return (<SplitLayoutManager> this.detailNav.component).shouldShowSplit();
        }
        return this.windowSize != WindowSize.Small && this.screenLayoutSetting == ScreenLayoutSetting.Split;
    }

    private isSplitLayoutManager(object: any): object is SplitLayoutManager {
        return 'shouldShowSplit' in object;
    }

    public isMasterVisible(): boolean {
        return this._currentMasterComponent != null && this.shouldShowSplit();
    }

    public isMenuDisabled(): boolean {
        return this.menuDisabled || !this.authenticationService.isAuthenticated();
    }

    public getMenuState(): MenuState {
        if (!this.authenticationService.isAuthenticated()) {
            return MenuState.Zero;
        }

        return this.canShowFixedMenu()
            ? (this.isMenuExpanded() ? MenuState.Expanded : MenuState.Collapsed)
            : MenuState.Zero;
    }

    /**
     * Returns true when the menu is to be shown (non floating), or 
     * false when it should not be shown (and can only be brought up in a floating manner)
     */
    public canShowFixedMenu(): boolean {
        return (this.windowSize == WindowSize.MediumLarge || this.windowSize == WindowSize.Large) && !this.menuDisabled;
    }

    public canShowFloatingMenu(): boolean {
        return (this.windowSize == WindowSize.Small ||
            this.windowSize == WindowSize.Medium ||
            this.windowSize == WindowSize.MediumLarge) &&
            !this.menuDisabled;
    }

    public isMenuCollapsed(): boolean {
        return (this.windowSize == WindowSize.MediumLarge ||
            (this.windowSize == WindowSize.Large &&
                this.menuLayoutSetting == MenuLayoutSetting.Collapsed)) &&
            !this.menuDisabled;
    }

    public isMenuExpanded(): boolean {
        return this.windowSize == WindowSize.Large &&
            this.menuLayoutSetting == MenuLayoutSetting.Expanded &&
            !this.menuDisabled;
    }

    public isMobile(): boolean {
        return this.windowSize == WindowSize.Small;
    }

    public getMenuWidth(): number {
        return document.getElementsByClassName('desktop-side-menu')[0].clientWidth;
    }

    public getMasterViewComponentWidth(): number {
        return document.getElementById('masterViewContainer').clientWidth;
    }

    public setMenuCollapsed(isCollapsed: boolean = true): void {
        this.menuLayoutSetting = isCollapsed ? MenuLayoutSetting.Collapsed : MenuLayoutSetting.Expanded;
    }

    public toggleMenuCollapsed(): void {
        this.menuLayoutSetting = this.menuLayoutSetting == MenuLayoutSetting.Expanded
            ? MenuLayoutSetting.Collapsed
            : MenuLayoutSetting.Expanded;
        this.eventService.menuStateChanged(this.getMenuState());
    }

    /**
     * Gets the width of the content pane, which is everything except the menu
     */
    public getContentWidth(): number {
        return window.innerWidth - this.getMenuWidth();
    }

    /**
     * Determine whether profile picture can be shown in the top right. 
     * Only shown at desktop size and when the user is logged in etc
     */
    public canShowProfilePictureIcon(): boolean {
        return (this.windowSize == WindowSize.MediumLarge ||
            this.windowSize == WindowSize.Large) &&
            this.authenticationService.isAuthenticated();
    }

    public updateWindowSize(): void {
        this.windowSize =
            window.innerWidth < 768 ? WindowSize.Small :
                window.innerWidth < 992 ? WindowSize.Medium :
                    window.innerWidth < 1200 ? WindowSize.MediumLarge :
                        WindowSize.Large;
        this.eventService.windowSizeChanged(this.windowSize);
        this.eventService.menuStateChanged(this.getMenuState());
        this.checkForSplitActiveChange(true);
    }

    private checkForSplitActiveChange(resizing: boolean = false): void {
        this.splitPaneEnabled = this.shouldShowSplit();
        if (this.splitPaneEnabled != this.previousSplitPaneEnabled) {
            this.splitState = this.splitPaneEnabled ? SplitState.Active : SplitState.Inactive;
            if (!resizing && !this.splitPaneEnabled && this.previousSplitPaneEnabled) {
                this.splitPaneAnimatingOut = true;
            } else {
                this.splitPaneVisible = this.splitPaneEnabled;
            }
            this.previousSplitPaneEnabled = this.splitPaneEnabled;
            this.splitPaneEnabledSubject.next(this.splitPaneEnabled);
        }
    }

    public removeSplitPaneAnimationDone(): void {
        if (this.splitPaneAnimatingOut) {
            if (this.masterView.length > 1) {
                this.masterView.remove(1);
            }
            this.splitPaneAnimatingOut = false;
            this.splitPaneVisible = false;
        }
    }

    private onAuthenticationStateChangedUpdateMenuState(): void {
        this.eventService.userAuthenticatedSubject$.subscribe(() => {
            this.eventService.menuStateChanged(this.getMenuState());
        });
    }
}
