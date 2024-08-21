import { ComponentFixture, TestBed } from "@angular/core/testing";
import { ReactiveFormsModule } from "@angular/forms";
import { CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { EntityDetailsListComponent } from "./entity-details-list.component";
import { UserViewModel } from "@app/viewmodels/user.viewmodel";
import { CustomerDetailViewModel } from "@app/viewmodels";
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedPopoverService } from "@app/services/shared-popover.service";
import { PermissionService } from "@app/services/permission.service";
import { AllowAccessDirective } from "@app/directives/allow-access.directive";
import { UserService } from "@app/services/user.service";
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';

describe("EntityDetailsListComponent", () => {
    let mockPermissionService: any;
    let mockUserService: any;

    beforeEach(() => {
        mockPermissionService = {
            hasElevatedPermissionsOfTheRelatedEntity: (): boolean => {
                return true;
            },
            hasElevatedPermissionsViaModel: (): boolean => {
                return true;
            },
            hasPermissions: (): boolean => {
                return true;
            },
            hasOneOfPermissions: (): boolean => {
                return true;
            },
            hasPermission: (): boolean => {
                return true;
            },
        };

        mockUserService = {
            unlinkIdentity: (): Promise<UserResourceModel> => {
                return Promise.resolve({} as UserResourceModel);
            },
        };

        TestBed.configureTestingModule({
            declarations: [
                EntityDetailsListComponent,
                AllowAccessDirective,
            ],
            providers: [
                { provide: NavProxyService, useValue: {} },
                { provide: SharedPopoverService, useValue: {} },
                { provide: PermissionService, useValue: mockPermissionService },
                { provide: UserService , useValue: mockUserService },
            ],
            schemas: [
                CUSTOM_ELEMENTS_SCHEMA,
            ],
            imports: [
                ReactiveFormsModule,
                RouterTestingModule,
                HttpClientModule,
            ],
        }).compileComponents();
    });

    it("should create the component", async () => {
        const fixture: ComponentFixture<EntityDetailsListComponent> =
            TestBed.createComponent(EntityDetailsListComponent);
        const app: any = fixture.debugElement.componentInstance;
        expect(app).toBeTruthy();
    });

    it("customer details = should filter groups and render/format items", () => {
        const fixture: ComponentFixture<EntityDetailsListComponent> =
            TestBed.createComponent(EntityDetailsListComponent);
        let component: EntityDetailsListComponent = fixture.componentInstance;
        let customer: any = {
            fullName: "",
            firstName: "customer",
            lastName: "fullName",
            middleNames: "Middle",
            namePrefix: "Dr",
            nameSuffix: "Jr",
            email: "CUSTOMER@test.com",
            preferredName: "prefer",
            userStatus: "active",
            ownerId: "",
            ownerFullName: "",
            createdDateTime: Date.now.toString(),
            id: "",
            tenantId: "test",
            quotes: [],
            policies: [],
            claims: [],
            transactions: [],
        };
        let customerModel: CustomerDetailViewModel = new CustomerDetailViewModel(customer);
        let navProxyService: NavProxyService = TestBed.inject(NavProxyService);
        let popoverService: SharedPopoverService = TestBed.inject(SharedPopoverService);
        let permissionService: PermissionService = TestBed.inject(PermissionService);
        let detailsListItems: Array<DetailsListItem>
            = customerModel.createCustomerDetailsList(navProxyService, popoverService, false, false, permissionService);

        // Sut
        component.detailsListItems = detailsListItems;
        component.ngOnInit();

        // assert
        expect(component.itemGroups.findIndex((g: any) => g.name == "person") > -1).toBe(true);
        expect(component.itemGroups.findIndex((g: any) => g.name == "others") > -1).toBe(true);
        expect(detailsListItems.findIndex((f: DetailsListItem) =>
            f.DisplayValue == "Dr customer (prefer) Middle fullName Jr" && f.GroupName == "person") > -1).toBe(true);
        expect(detailsListItems.findIndex((f: DetailsListItem) =>
            f.DisplayValue == "active" && f.GroupName == "others") > -1).toBe(false);
    });

    it("user details = should filter groups and render/format items", () => {
        const fixture: ComponentFixture<EntityDetailsListComponent> =
            TestBed.createComponent(EntityDetailsListComponent);
        let component: EntityDetailsListComponent = fixture.componentInstance;
        let user: any = {
            fullName: "",
            firstName: "user",
            lastName: "fullName",
            middleNames: "Middle",
            namePrefix: "Dr",
            nameSuffix: "Jr",
            email: "USER@test.com",
            preferredName: "prefer",
            userStatus: "active",
            blocked: false,
            createdDateTime: Date.now.toString(),
            lastModifiedDateTime: Date.now.toString(),
            id: "",
            profilePictureId: "",
            userType: "",
        };

        let userModel: UserViewModel = new UserViewModel(user);
        let navProxyService: NavProxyService = TestBed.inject(NavProxyService);
        let userService: UserService = TestBed.inject(UserService);
        let sharedPopperService: SharedPopoverService = TestBed.inject(SharedPopoverService);
        let permissionService: PermissionService = TestBed.inject(PermissionService);


        let detailsListItems: Array<DetailsListItem> = userModel.createDetailsList(
            userService,
            navProxyService,
            sharedPopperService,
            permissionService,
            false,
            false,
        );
        // Sut
        component.detailsListItems = detailsListItems;
        component.ngOnInit();

        // assert
        expect(component.itemGroups.findIndex((g: any) => g.name == "person") > -1).toBe(true);
        expect(component.itemGroups.findIndex((g: any) => g.name == "others") > -1).toBe(false);
        expect(detailsListItems.findIndex((f: DetailsListItem) =>
            f.DisplayValue == "Dr user (prefer) Middle fullName Jr" &&
            f.GroupName == "person") > -1).toBe(true);
        expect(detailsListItems.findIndex((f: DetailsListItem) =>
            f.DisplayValue == "active" && f.GroupName == "others") > -1).toBe(false);
    });

    it("account details = should filter groups and render/format items", () => {
        const fixture: ComponentFixture<EntityDetailsListComponent> =
            TestBed.createComponent(EntityDetailsListComponent);
        let component: EntityDetailsListComponent = fixture.componentInstance;
        let user: any = {
            fullName: "",
            firstName: "account",
            lastName: "fullName",
            middleNames: "Middle",
            namePrefix: "Dr",
            nameSuffix: "Jr",
            preferredName: "prefer",
            blocked: false,
            createdDateTime: "",
            lastModifiedDateTime: "",
            id: "",
            profilePictureId: "",
            status: "",
            userType: "",
        };

        let userModel: UserViewModel = new UserViewModel(user);
        let navProxyService: NavProxyService = TestBed.inject(NavProxyService);
        let userService: UserService = TestBed.inject(UserService);
        let sharedPopperService: SharedPopoverService = TestBed.inject(SharedPopoverService);
        let permissionService: PermissionService = TestBed.inject(PermissionService);

        let detailsListItems: Array<DetailsListItem> = userModel.createDetailsList(
            userService,
            navProxyService,
            sharedPopperService,
            permissionService,
            false,
            false,
            true);

        // Su
        component.detailsListItems = detailsListItems;
        component.ngOnInit();

        // assert
        expect(component.itemGroups.findIndex((g: any) => g.name == "person") > -1).toBe(true);
        expect(detailsListItems.findIndex((f: DetailsListItem) =>
            f.DisplayValue == "Dr account (prefer) Middle fullName Jr" &&
            f.GroupName == "person") > -1).toBe(true);
    });

    it("custom details = should filter groups, move icon, and render items", () => {
        const fixture: ComponentFixture<EntityDetailsListComponent> =
            TestBed.createComponent(EntityDetailsListComponent);
        let component: EntityDetailsListComponent = fixture.componentInstance;

        let detailsListItems: Array<DetailsListItem> = [] as Array<DetailsListItem>;

        detailsListItems.push(DetailsListItem.createItem(
            null,
            "group1",
            "test_title1",
            "test_desc1",
            "icon1",
        ));
        detailsListItems.push(DetailsListItem.createItem(
            null,
            "group1",
            "test_title2",
            "test_desc2",
            ""));

        detailsListItems.push(DetailsListItem.createItem(
            null,
            "group2",
            "",
            "test_desc1",
            "icon1"));
        detailsListItems.push(DetailsListItem.createItem(
            null,
            "group2",
            "test_title2",
            "test_desc2",
            "icon1"));
        detailsListItems.push(DetailsListItem.createItem(
            null,
            "group2",
            "test_title3",
            "test_desc2",
            "icon1"));

        // Sut
        component.detailsListItems = detailsListItems;
        component.ngOnInit();

        // Assert

        expect(component.itemGroups.findIndex((g: any) => g.name == "group1") > -1).toBe(true);
        expect(component.itemGroups.findIndex((g: any) => g.name == "group2") > -1).toBe(true);

        // Group 1
        expect(component.detailsListItems.findIndex((f: DetailsListItem) =>
            f.DisplayValue == "test_title1" && f.GroupName == "group1") > -1).toBe(true);
        expect(component.detailsListItems.findIndex((f: DetailsListItem) =>
            f.DisplayValue == "test_title2" && f.GroupName == "group1") > -1).toBe(true);

        // Group 2
        expect(component.detailsListItems.filter((f: DetailsListItem) =>
            f.GroupName == "group2").length == 2).toBe(true);
        expect(component.detailsListItems.filter((f: DetailsListItem) => f.GroupName == "group2" &&
            f.DisplayValue == "test_title3" && f.Icon).length == 1).toBe(true);
        expect(component.detailsListItems.filter((f: DetailsListItem) => f.GroupName == "group2" &&
            f.DisplayValue == "test_title2" && f.Icon).length == 1).toBe(true);
        expect(component.detailsListItems.findIndex((f: DetailsListItem) => f.DisplayValue == "test_title2" &&
            f.GroupName == "group2") > -1).toBe(true);
        expect(component.detailsListItems.findIndex((f: DetailsListItem) => f.DisplayValue == "test_title3" &&
            f.GroupName == "group2") > -1).toBe(true);
    });
});
