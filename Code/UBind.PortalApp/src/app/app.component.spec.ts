import { AppComponent } from "./app.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { CUSTOM_ELEMENTS_SCHEMA, ChangeDetectorRef, Renderer2, ErrorHandler } from "@angular/core";
import { AllowAccessDirective } from "./directives/allow-access.directive";
import {
    ActionSheetController, Platform,
    PopoverController, MenuController, AlertController,
    IonRouterOutlet, NavController,
} from "@ionic/angular";
import { SplashScreen } from "@ionic-native/splash-screen/ngx";
import { StatusBar } from "@ionic-native/status-bar/ngx";
import { LocationStrategy } from "@angular/common";
import { Router, ChildrenOutletContexts, ActivatedRoute } from "@angular/router";
import { BroadcastService } from "./services/broadcast.service";
import { Observable, Subject } from "rxjs";
import { Permission } from "./helpers/permissions.helper";
import { NoopAnimationsModule } from "@angular/platform-browser/animations";
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AuthenticationService } from "./services/authentication.service";
import { ParentFrameMessageService } from "./services/parent-frame-message.service";
import { PortalApiService } from "./services/api/portal-api.service";
import { FeatureSettingService } from "./services/feature-setting.service";
import { FocusOnShowDirective } from "./directives/focus-element.directive";
import { SharedLoaderService } from "./services/shared-loader.service";
import { MenuItem } from "./models/menu-item";
import { ErrorHandlerService } from "./services/error-handler.service";
import { GlobalErrorHandler } from "./providers/global-error-handler";
import { UserService } from "./services/user.service";
import { ProfilePicUrlPipe } from "./pipes/profile-pic-url.pipe";
import { ProductFeatureSettingService } from "./services/product-feature-service";
import { HttpClientModule, HttpErrorResponse } from "@angular/common/http";
import { AccountApiService } from "./services/api/account-api.service";
import { RouterTestingModule } from "@angular/router/testing";

// disabled due to intermittent failures. Will be fixed in UB-10955
xdescribe("AppComponent with filtered permission", () => {

    let platformSpy: any;
    let splashScreenSpy: any;
    let statusBarSpy: any;
    let locationStrategySpy: any;
    let authenticationServiceStub: any;
    let routerStub: any;
    let appConfigServiceStub: any;
    let featureSettingServiceStub: any;
    let userServiceStub: any;
    let broadcastServiceStub: any;
    let messageServiceStub: any;
    let portalApiServiceStub: any;
    let sharedLoaderServiceStub: any;
    let navProxyStub: any;
    let errorHandlerServiceStub: any;
    let productFeatureServiceStub: any;
    let accountApiServiceSpy: any;

    beforeEach(() => {
        platformSpy = jasmine.createSpyObj("Platform", { ready: Promise.resolve() });
        splashScreenSpy = jasmine.createSpyObj("SplashScreen", ["hide"]);
        statusBarSpy = jasmine.createSpyObj("StatusBar", ["styleDefault"]);
        locationStrategySpy = jasmine.createSpyObj("LocationStrategy", ["onPopState"]);
        accountApiServiceSpy = jasmine.createSpyObj("AccountApiService", ["get"]);

        authenticationServiceStub = {
            isAuthenticated: (): boolean => true,
            userRole: "UBindAdmin",
            tenantId: "ubind",
            isAdmin: (): boolean => true,
            isCustomer: (): boolean => false,
            isMasterUser: (): boolean => false,
            logout: (): void => { },
        };

        routerStub = {
            events: new Observable((observer: any): void => {
                observer.next({});
                observer.complete();
            }),
        };

        appConfigServiceStub = {
            get: (): void => { },
            getEnvironment: (): string => "Development",
            appConfigSubject: new Subject(),
        };

        featureSettingServiceStub = {
            getPortalFeatures: (): any => new Observable((observer: any): void => {
                observer.next({});
                observer.complete();
            }),
            getFeatureMenu: (): Array<MenuItem> => new Array<MenuItem>(),
        };

        userServiceStub = {
            userHasPermission: (permission: Permission): boolean => {
                if (permission === Permission.ViewTenants) {
                    return false; // No view tenant permisssion
                }
                return true;
            },
        };

        broadcastServiceStub = {
            on: (key: any): any => new Observable((observer: any): void => {
                observer.next({});
                observer.complete();
            }),
        };

        messageServiceStub = {
            findGetParameter: (parameter: string): any => undefined,
            sendMessage: (messageType: string, payload: any): void => { },
        };

        portalApiServiceStub = {
            getPortalById: (): any => new Observable((observer: any): void => {
                observer.next({});
                observer.complete();
            }),
            getByAlias: (): any => new Observable((observer: any): void => {
                observer.next({});
                observer.complete();
            }),
        };

        sharedLoaderServiceStub = {
            presentWait: (): Promise<void> => Promise.resolve(),
            present: (): Promise<void> => Promise.resolve(),
            dismiss: (): void => { },
        };

        navProxyStub = {
            navigate: (): void => { },
        };

        errorHandlerServiceStub = {
            handleError: (err: HttpErrorResponse): void => {
                console.error(err);
            },
        };

        TestBed.configureTestingModule({
            declarations: [
                AppComponent,
                AllowAccessDirective,
                FocusOnShowDirective,
                IonRouterOutlet,
                ProfilePicUrlPipe,
            ],
            schemas: [CUSTOM_ELEMENTS_SCHEMA],
            providers: [
                { provide: ActionSheetController, useValue: {} },
                { provide: Platform, useValue: platformSpy },
                { provide: SplashScreen, useValue: splashScreenSpy },
                { provide: StatusBar, useValue: statusBarSpy },
                { provide: LocationStrategy, useValue: locationStrategySpy },
                { provide: Router, useValue: routerStub },
                { provide: AuthenticationService, useValue: authenticationServiceStub },
                { provide: NavProxyService, useValue: navProxyStub },
                { provide: AppConfigService, useValue: appConfigServiceStub },
                { provide: SharedLoaderService, useValue: sharedLoaderServiceStub },
                { provide: ParentFrameMessageService, useValue: messageServiceStub },
                { provide: BroadcastService, useValue: broadcastServiceStub },
                { provide: ChangeDetectorRef, useValue: {} },
                { provide: PortalApiService, useValue: portalApiServiceStub },
                { provide: FeatureSettingService, useValue: featureSettingServiceStub },
                { provide: ProductFeatureSettingService, useValue: productFeatureServiceStub },
                { provide: PopoverController, useValue: {} },
                { provide: Renderer2, useValue: {} },
                { provide: UserService, useValue: userServiceStub },
                { provide: MenuController, useValue: {} },
                { provide: AlertController, useValue: {} },
                { provide: ChildrenOutletContexts, useValue: new ChildrenOutletContexts() },
                { provide: NavController, useValue: {} },
                { provide: ErrorHandler, useClass: GlobalErrorHandler },
                { provide: ErrorHandlerService, useValue: errorHandlerServiceStub },
                { provide: ActivatedRoute, useValue: {} },
                { provide: AccountApiService, useValue: accountApiServiceSpy },

            ],
            imports: [
                NoopAnimationsModule,
                HttpClientModule,
                RouterTestingModule.withRoutes(
                    [
                        { path: 'add', component: AppComponent, pathMatch: 'full' },
                    ],
                ),
            ],
        }).compileComponents();
    });

    it("should create the component", async () => {
        const fixture: ComponentFixture<AppComponent> = TestBed.createComponent(AppComponent);
        const app: any = fixture.debugElement.componentInstance;
        expect(app).toBeTruthy();
    });

    // The following test needs more work
    /*
    it("appPages should not contain filtered page", () => {
        const fixture = TestBed.createComponent(AppComponent);
        const component = fixture.componentInstance;
        component.ngOnInit();
        expect(component.appPages.filter(p => p.identifier === "Tenants")).toEqual([]);
    });
    */
});
