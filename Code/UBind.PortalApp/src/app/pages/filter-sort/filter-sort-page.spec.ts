import { TestBed, ComponentFixture } from "@angular/core/testing";
import { ModalController } from "@ionic/angular";
import { ReactiveFormsModule } from "@angular/forms";
import { CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { QueryRequestHelper } from "@app/helpers";
import { RouterModule, NavigationExtras } from "@angular/router";
import { RouteHelper } from "@app/helpers/route.helper";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { SharedModule } from '@app/shared.module';
import { RouterTestingModule } from '@angular/router/testing';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { APP_BASE_HREF } from '@angular/common';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserType } from "@app/models/user-type.enum";
import { FieldOption } from "@app/models/entity-edit-field-option";

describe("FilterSortPage", () => {
    let statusList: Array<string> = [
        "Incomplete",
        "Review",
        "Endorsement",
        "Approved",
        "Declined",
        "Complete",
    ];
    let mockLayoutManager: any;
    let mockRouteHelper: any;
    let navProxyStub: any;
    let authServiceStub: any;
    let component: FilterSortPage;
    let fixture: ComponentFixture<FilterSortPage>;
    let sortOptions: SortOption = {
        sortBy: [
            'Created Date',
            'Customer Name',
        ],
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };
    let selectedSortOption: SortOption = { sortBy: ['Created Date'], sortOrder: [SortDirection.Ascending] };
    let query: Array<any> = QueryRequestHelper.constructStringFilters(statusList, []);
    let navigationExtras: NavigationExtras = {
        state: {
            statusList: query,
            filterByDates: ['Created Date'],
            dateIsBefore: '< 18 Feb 2021 ',
            dateIsAfter: '> 03 Feb 2021 ',
            filterTitle: 'Filter & Sort Policies',
            statusTile: 'Status',
            entityTypeName: 'Policy',
            selectedId: '',
            sortOptions: sortOptions,
            selectedSortOption: selectedSortOption,
            testData: true,
        },
    };

    mockLayoutManager = {
        canShowFixedMenu: (): boolean => true,
        isMenuExpanded: (): boolean => true,
        splitPaneVisible: true,
    };

    mockRouteHelper = {
        getParam: (): any => null,
        navigationExtras: navigationExtras,
    };

    navProxyStub = {
        navigate: (): void => { },
        navigateBack: (): boolean => true,
    };

    authServiceStub = {
        userType: (): UserType => UserType.Client,
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            declarations: [
                FilterSortPage,
            ],
            providers: [
                { provide: ModalController, useValue: {} },
                { provide: RouteHelper, useValue: mockRouteHelper },
                { provide: LayoutManagerService, useValue: mockLayoutManager },
                { provide: EventService, useClass: EventService },
                { provide: NavProxyService, useValue: navProxyStub },
                { provide: APP_BASE_HREF, useValue: '/' },
                { provide: AuthenticationService, useValue: authServiceStub },
            ],
            schemas: [
                CUSTOM_ELEMENTS_SCHEMA,
            ],
            imports: [
                ReactiveFormsModule,
                SharedModule,
                RouterModule.forRoot([]),
                RouterTestingModule,
            ],
        }).compileComponents();
    });

    beforeEach(async () => {
        fixture = TestBed.createComponent(FilterSortPage);
        component = fixture.debugElement.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it("should create the component", async () => {
        fixture.detectChanges();
        const app: any = fixture.debugElement.componentInstance;
        expect(app).toBeTruthy();
    });

    it("Router param should not be empty", async () => {
        expect(component.routeHelper.navigationExtras.state).toBeTruthy();
    });

    it("status should contain the statusList", () => {
        expect(component.statusTypes.map((s: FieldOption) => s.label)).toEqual(jasmine.arrayContaining(statusList));
    });

    it("Filter title should be in Plural form", () => {
        expect(component.filterSortTitle).not.toContain(component.entityTypeName);
        expect(component.filterSortTitle).toContain('Policies');
    });

    it("should have default sort by value", () => {
        expect(component.defaultSortBy).toBe('Created Date');
    });

    it("should have default sort order value", () => {
        expect(component.defaultSortOrder).toBe(SortDirection[SortDirection.Descending]);
    });

});
