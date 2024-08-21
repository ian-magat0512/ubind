import { ComponentFixture, TestBed } from "@angular/core/testing";
import { CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { UpdatePolicyPage } from './update-policy.page';
import { ReactiveFormsModule } from "@angular/forms";
import { RouterTestingModule } from '@angular/router/testing';
import { PermissionService } from '@app/services/permission.service';
import { Subject } from 'rxjs';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { AllowAccessDirective } from "@app/directives/allow-access.directive";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { QuoteApiService } from "@app/services/api/quote-api.service";
import { AppConfigService } from "@app/services/app-config.service";
import { HttpClientModule, HttpErrorResponse } from "@angular/common/http";
import { ErrorHandlerService } from "@app/services/error-handler.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { FeatureSettingResourceModel } from "@app/models";

describe('UpdatePolicyPage', () => {
    let navProxyStub: any;
    let quoteApiSpy: any;
    let appConfigServiceStub: any;
    let errorHandlerServiceStub: any;
    let authenticationServiceStub: any;
    let mockPermissionService: any;

    beforeEach(() => {
        quoteApiSpy = jasmine.createSpyObj("QuoteApiService", ["getById"]);
        mockPermissionService = jasmine.createSpyObj('PermissionService', ['hasElevatedPermissionsViaModel']);
        jasmine.DEFAULT_TIMEOUT_INTERVAL = 10000;

        navProxyStub = {
            navigate: (): void => { },
            destroyed: new Subject<void>(),
        };

        appConfigServiceStub = {
            get: (): void => { },
            getEnvironment: (): string => "Development",
            getFeatureSettings: (): Array<FeatureSettingResourceModel> => [],
            appConfigSubject: new Subject(),
        };

        errorHandlerServiceStub = {
            handleError: (err: HttpErrorResponse): void => {
                console.error(err);
            },
        };

        authenticationServiceStub = {
            isAuthenticated: (): boolean => true,
            userRole: "UBindAdmin",
            tenantId: "ubind",
            isAdmin: (): boolean => true,
            isCustomer: (): boolean => false,
            isMasterAdmin: (): boolean => false,
            logout: (): void => { },
        };

        TestBed.configureTestingModule({
            declarations: [
                UpdatePolicyPage,
                AllowAccessDirective,
            ],
            providers: [
                { provide: NavProxyService, useValue: navProxyStub },
                { provide: PermissionService, useValue: mockPermissionService },
                { provide: QuoteApiService, useValue: quoteApiSpy },
                { provide: AppConfigService, useValue: appConfigServiceStub },
                { provide: ErrorHandlerService, useValue: errorHandlerServiceStub },
                { provide: AuthenticationService, useValue: authenticationServiceStub },
            ],
            schemas: [
                CUSTOM_ELEMENTS_SCHEMA,
            ],
            imports: [
                ReactiveFormsModule,
                RouterTestingModule,
                HttpClientModule,
            ],
        });
    });

    describe('ngAfterViewInit', () => {
        it('should call the checkPermissions method when customerDetails is null', () => {
            mockPermissionService.hasElevatedPermissionsViaModel.and.returnValue(true);
            const fixture: ComponentFixture<UpdatePolicyPage> = TestBed.createComponent(UpdatePolicyPage);
            const component: UpdatePolicyPage = fixture.componentInstance;
            (component as any).destroyed = new Subject<void>();
            const quote: QuoteResourceModel = {
                quoteNumber: 'TEST-123',
                quoteTitle: 'some title',
                productId: 'someProductId',
                productAlias: 'dev',
                productName: 'Dev',
                deploymentEnvironment: 'Development',
                customerDetails: null,
                totalAmount: '100',
                lastModifiedDateTime: '2020-01-01',
                status: 'active',
                createdDateTime: '2020-01-01',
                expiryDateTime: '2020-01-01',
                isTestData: false,
                quoteType: 1,
                organisationId: 'someOrgId',
                organisationAlias: 'someOrgAlias',
                ownerUserId: 'someUserId',
                policyId: '',
                id: '',
            };
            spyOn(component as any, 'checkPermissions').and.callThrough();
            (<any>component).checkPermissions(quote);
            expect((<any>component).checkPermissions).toHaveBeenCalled();
        });

        it('should call the checkPermissions method when customerDetails is not null', () => {
            mockPermissionService.hasElevatedPermissionsViaModel.and.returnValue(true);
            const fixture: ComponentFixture<UpdatePolicyPage> = TestBed.createComponent(UpdatePolicyPage);
            const component: UpdatePolicyPage = fixture.componentInstance;
            (component as any).destroyed = new Subject<void>();
            const quote: QuoteResourceModel = {
                quoteNumber: 'TEST-123',
                quoteTitle: 'some title',
                productId: 'someProductId',
                productAlias: 'dev',
                productName: 'Dev',
                deploymentEnvironment: 'Development',
                customerDetails: {
                    id: '345366',
                    fullName: 'Urie Brendon',
                    displayName: 'Urie Brendon',
                    isTestData: false,
                    status: 'active',
                    createdDateTime: '2020-01-01',
                    lastModifiedDateTime: '2020-01-01',
                    profilePictureId: '',
                    ownerUserId: '',
                    userId: '',
                    primaryPersonId: '',
                    deleteFromList: false,
                },
                totalAmount: '100',
                lastModifiedDateTime: '2020-01-01',
                status: 'active',
                createdDateTime: '2020-01-01',
                expiryDateTime: '2020-01-01',
                isTestData: false,
                quoteType: 1,
                organisationId: 'someOrgId',
                organisationAlias: 'someOrgAlias',
                ownerUserId: 'someUserId',
                policyId: '',
                id: '',
            };
            spyOn(component as any, 'checkPermissions').and.callThrough();
            (<any>component).checkPermissions(quote);
            expect((<any>component).checkPermissions).toHaveBeenCalled();
        });
    });
});
