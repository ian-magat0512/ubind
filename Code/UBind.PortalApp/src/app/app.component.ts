import {
    Component, OnInit, ViewChild, HostListener,
    ElementRef, ChangeDetectorRef, AfterContentInit,
    OnDestroy, ViewContainerRef,
} from '@angular/core';
import { trigger, state, style, transition, animate, query } from '@angular/animations';
import { PopoverController, ActionSheetController, MenuController } from '@ionic/angular';
import { Platform, IonRouterOutlet } from '@ionic/angular';
import { SplashScreen } from '@ionic-native/splash-screen/ngx';
import { StatusBar } from '@ionic-native/status-bar/ngx';
import { FeatureSettingService } from './services/feature-setting.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { Subject, Subscription } from 'rxjs';
import { BroadcastService } from './services/broadcast.service';
import { PopoverMyAccountComponent } from './components/popover-my-account/popover-my-account.component';
import { contentAnimation } from '../assets/animations';
import { Permission } from "./helpers/permissions.helper";
import { AuthenticationService } from './services/authentication.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { StringHelper } from '@app/helpers';
import { DomSanitizer, Meta, SafeUrl } from '@angular/platform-browser';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { ListNoSelectionPage } from '@app/pages/list-no-selection/list-no-selection.page';
import { EventService, UserId } from '@app/services/event.service';
import { AppConfig } from '@app/models/app-config';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EnvironmentChangeRedirectService } from '@app/services/environment-change-redirect.service';
import { UserService } from '@app/services/user.service';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { AccountApiService } from './services/api/account-api.service';
import { takeUntil } from 'rxjs/operators';
import { PermissionService } from './services/permission.service';
import { SubscriptionLike } from 'rxjs/internal/types';
import { IconLibrary } from './models/icon-library.enum';

declare let window: any;

/**
 * The main component for the app which is loaded for every url, with child components loaded within it.
 */
@Component({
    selector: 'app-root',
    templateUrl: 'app.component.html',
    styleUrls: [
        'app.component.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
    animations: [
        trigger('menuAnimation', [
            state('collapsed', style({
                width: '57px',
            })),
            state('expanded', style({
                width: '201px',
            })),
            state("zero", style({
                width: "0",
            })),
            transition("collapsed <=> expanded", animate("250ms ease-in-out")),
            transition("zero <=> collapsed", animate("250ms ease-in-out")),
        ]),
        trigger('paneAnimation', [
            state('collapsed', style({
                marginLeft: '57px',
            })),
            state('expanded', style({
                marginLeft: '201px',
            })),
            state("zero", style({
                marginLeft: "0",
            })),
            transition('collapsed <=> expanded', animate('250ms ease-in-out')),
            transition('zero <=> collapsed', animate('250ms ease-in-out')),
        ]),
        trigger('removeSplitPaneAnimation', [
            transition('active => inactive', [
                query('ion-content', animate('200ms ease-out', style({ opacity: 0 }))),
            ]),
        ]),
        contentAnimation,
    ],
})
export class AppComponent implements OnInit, OnDestroy, AfterContentInit {

    @ViewChild('masterView', { read: ViewContainerRef, static: true })
    public masterView: ViewContainerRef;
    @ViewChild('detailNav', { read: IonRouterOutlet, static: true })
    public detailNav: IonRouterOutlet;
    @ViewChild('toggleDesktopSideMenuButton', { read: ElementRef, static: true })
    public toggleDesktopSideMenuButton: ElementRef;
    @ViewChild('desktopSideMenu', { read: ElementRef, static: true })
    public desktopSideMenu: ElementRef;
    @ViewChild('desktopSplitPane', { read: ElementRef, static: true })
    public desktopSplitPane: ElementRef;
    @ViewChild('desktopProfileIcon', { read: ElementRef, static: true })
    public desktopProfileIcon: ElementRef;
    public tenantName: string;
    public organisationName: string;
    public portalTitle: string;
    public hasPortalTitle: boolean = false;
    private tenantId: string;
    public profilePictureBaseUrl: string;
    public profilePictureUrl: SafeUrl;
    public rootPage: any = ListNoSelectionPage;
    public isMenuCollapsed: boolean;
    public userDidTapMyAccount$: Subscription;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public permission: typeof Permission = Permission;
    public environment: any;
    public defaultImgPath: string = 'assets/imgs/profile-placeholder.svg';
    public onPreviewParam: string;
    public selectedPage: any;
    protected destroyed: Subject<void>;
    public isIE11: boolean = !(window.ActiveXObject) && "ActiveXObject" in window;
    public isEdge: boolean = window.navigator.userAgent.indexOf("Edge") > -1;
    public userFullName: string;
    public userEmail: string;
    public profilePictureId: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        // DO NOT REMOVE environmentChangeRedirectService!!!
        private environmentChangeRedirectService: EnvironmentChangeRedirectService,
        public actionSheetController: ActionSheetController,
        platform: Platform,
        splashScreen: SplashScreen,
        statusBar: StatusBar,
        public authenticationService: AuthenticationService,
        public navProxy: NavProxyService,
        public appConfigService: AppConfigService,
        private broadcastService: BroadcastService,
        private changeDetector: ChangeDetectorRef,
        private featureSettingService: FeatureSettingService,
        private productFeatureService: ProductFeatureSettingService,
        public popoverController: PopoverController,
        private menuCtrl: MenuController,
        public userService: UserService,
        private meta: Meta,
        public layoutManager: LayoutManagerService,
        public eventService: EventService,
        private userPath: UserTypePathHelper,
        private sharedPopoverService: SharedPopoverService,
        private sanitizer: DomSanitizer,
        private accountApiService: AccountApiService,
        private permissionService: PermissionService,
    ) {
        platform.ready().then(() => {
            statusBar.styleDefault();
            splashScreen.hide();
        });

        this.setPortalTitle();
    }

    private setPortalTitle(): void {
        this.subscriptions.push(this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (appConfig.portal.tenantName) {
                this.tenantName = appConfig.portal.tenantName;
                this.organisationName = appConfig.portal.organisationName;
                this.portalTitle = appConfig.portal.title;
                this.hasPortalTitle = !StringHelper.isNullOrWhitespace(this.portalTitle);
            }
        }));
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.layoutManager.init(this.masterView, this.detailNav);
        this.listenForChangesToLoggedInUser();
    }

    private listenForChangesToLoggedInUser(): void {
        this.userFullName = this.authenticationService.userFullName;
        this.userEmail = this.authenticationService.userEmail;
        this.profilePictureId = this.authenticationService.profilePictureId;
        this.subscriptions.push(this.eventService.userLoginSubject$.subscribe((userId: UserId) => {
            this.userFullName = this.authenticationService.userFullName;
            this.userEmail = this.authenticationService.userEmail;
            this.profilePictureId = this.authenticationService.profilePictureId;
        }));
        this.subscriptions.push(
            this.eventService.getEntityUpdatedSubject('User').subscribe((user: UserResourceModel) => {
                this.updateUserDetails(user);
            }),
        );
        this.eventService.customerUpdatedSubject$.subscribe(() => {
            if (!this.permissionService.hasPermission(Permission.ViewMyAccount)) {
                return;
            }
            this.accountApiService.get()
                .pipe(takeUntil(this.destroyed))
                .subscribe(
                    (user: UserResourceModel) => {
                        this.updateUserDetails(user);
                    },
                );
        });
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe);
        this.layoutManager.onDestroy();
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public ngAfterContentInit(): void {
        if (this.userDidTapMyAccount$) {
            this.userDidTapMyAccount$.unsubscribe();
        }
        this.userDidTapMyAccount$ = this.broadcastService.on("userDidTapMyAccountEvent").subscribe(() => {
            this.userDidTapMyAccount();
            this.popoverController.dismiss();
        });

        this.onResize();
        this.setMetaTags();

        if (this.isIE11 || this.isEdge) {
            document.addEventListener('click', (e: any) => {
                if (this.hasClass(e.target, 'menu-backdrop')) {
                    this.menuCtrl.close();
                }
            });
        }
    }

    public async userDidTapMyAccount(): Promise<void> {
        this.navProxy.navigate([this.userPath.account]);
    }

    private hasClass(elem: any, className: string): any {
        return elem.classList.contains(className);
    }

    private updateUserDetails(user: UserResourceModel): void {
        if (this.authenticationService.userId == user.id) {
            this.userFullName = user.fullName;
            this.userEmail = user.email;
            this.profilePictureId = user.profilePictureId;
        }
    }

    @HostListener('window:resize', ['$event'])
    public async onResize(): Promise<void> {
        this.layoutManager.updateWindowSize();
        const popoverTop: HTMLIonPopoverElement = await this.popoverController.getTop();
        if (popoverTop) {
            popoverTop.dismiss();
        }

        if (this.layoutManager.canShowFixedMenu() && this.menuCtrl.isOpen()) {
            this.menuCtrl.close();
        }
    }

    @HostListener('window:blur', ['$event'])
    public async onBlur(): Promise<void> {
        const focusedElements: Array<Element> = Array.from(document.getElementsByClassName('ion-focused'));
        for (let element of focusedElements) {
            element.classList.remove('ion-focused');
        }
    }

    public async presentMyAccountPopover(ev: any): Promise<void> {
        await this.sharedPopoverService.show({
            component: PopoverMyAccountComponent,
            event: ev,
            showBackdrop: false,
        }, 'My account menu popover');
    }

    public toggleDesktopSideMenu(env: any): void {
        this.layoutManager.toggleMenuCollapsed();
    }

    private setMetaTags(): void {
        this.meta.addTag({
            name: 'format-detection',
            content: 'telephone=no',
        });
        this.meta.addTag({
            name: 'msapplication-tap-highlight',
            content: 'no',
        });
        this.meta.addTag({
            name: 'apple-mobile-web-app-capable',
            content: 'yes',
        });
        this.meta.addTag({
            name: 'apple-mobile-web-app-status-bar-style',
            content: 'black',
        });
        this.meta.addTag({
            name: 'X-UA-Compatible',
            content: 'IE=edge',
        });
    }

    public menuAnimationDone($event: any): void {
        this.changeDetector.markForCheck();
    }

    public onRouteActivate($event: any): void {
        this.layoutManager.onRouteActivate($event);
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultImgPath, null);
    }
}
