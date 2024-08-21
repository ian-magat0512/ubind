import {
    ChangeDetectorRef, Component, CUSTOM_ELEMENTS_SCHEMA, ViewChild,
} from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NoopAnimationsModule } from "@angular/platform-browser/animations";
import { Router } from "@angular/router";
import { LocalDateHelper } from "@app/helpers";
import { RouteHelper } from "@app/helpers/route.helper";
import { ListModule } from "@app/list.module";
import { Entity } from "@app/models/entity";
import { RepositoryRegistry } from "@app/repositories/repository-registry";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { LoadDataService } from "@app/services/load-data.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedModule } from "@app/shared.module";
import { GroupedEntityViewModel } from "@app/viewmodels/grouped-entity.viewmodel";
import { SortDirection, SortedEntityViewModel } from "@app/viewmodels/sorted-entity.viewmodel";
import { SegmentableEntityViewModel } from "@app/viewmodels/segmentable-entity.viewmodel";
import { AlertController, ModalController, NavController } from "@ionic/angular";
import { BehaviorSubject, Observable, Subject } from "rxjs";
import { SharedComponentsModule } from "../shared-components.module";
import { EntityListComponent } from "./entity-list.component";
import { AuthenticationService } from "@app/services/authentication.service";
import { PortalExtensionsService } from "@app/services/portal-extensions.service";
import { AppConfigService } from "@app/services/app-config.service";
import { HttpClientModule } from "@angular/common/http";

/**
 * A fake entity for testing the entity list component.
 */
interface TestEntityResourceModel extends Entity {
    id: string;
    fullName: string;
    isTestData: boolean;
    status: string;
    createdDateTime: string;
    profilePictureId: string;
    deleted: boolean;
}

/**
 * A fake entity view model for use in testing the EntityListComponent
 */
class TestEntityViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(customer: TestEntityResourceModel) {
        this.id = customer.id;
        this.name = customer.fullName;
        this.isTestData = customer.isTestData;
        this.segment = customer.status.toLowerCase();
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(customer.createdDateTime);
        this.groupByValue = this.createdDate = LocalDateHelper.toLocalDate(customer.createdDateTime);
        this.sortByValue = customer.fullName;
        this.sortDirection = SortDirection.Ascending;
        this.deleteFromList = customer.deleted;

        this.profilePictureId = customer.profilePictureId;
    }

    public id: string;
    public name: string;
    public segment: string;
    public isTestData: boolean;
    public createdDate: string;
    public createdTime: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public profilePictureId: string;
    public deleteFromList: boolean = false;

    public setGroupByValue(
        testEntityList: Array<TestEntityViewModel>,
        groupBy: string,
    ): Array<TestEntityViewModel> {
        if (groupBy === "Created Date") {
            testEntityList.forEach((item: any) => {
                item.groupByValue = item.createdDate;
            });
        }
        return testEntityList;
    }

    public setSortOptions(
        testEntityList: Array<TestEntityViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<TestEntityViewModel> {
        if (sortBy === "Created Date") {
            testEntityList.forEach((item: any) => {
                item.sortByValue = item.createdDate;
                item.sortDirection = sortDirection;
            });
        }
        return testEntityList;
    }
}

describe('EntityListComponent', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let mockTestEntityApiServiceStub: any;
    let mockLayoutManager: any;
    let mockRouteHelper: any;
    let mockAuthService: any;
    let testData: Array<TestEntityResourceModel>;
    let mockPortalExtensionsService: any;
    let appConfigServiceStub: any;

    /**
     * Test Host component class
     */
    @Component({
        selector: `app-host-component`,
        template: `
            <app-entity-list #list
                [title]="title"
                [listItemNamePlural]="'customers'"
                [entityLoaderService]="entityLoaderService"
                [viewModelConstructor]="viewModelConstructor"
                [entityTypeName]="'TestEntity'"
                [entityRouteParamName]="'customerId'"
                [itemTemplate]="customerListItemTemplate"
            >
            </app-entity-list>
            <ng-template #customerListItemTemplate let-item>
                <ion-label>
                    <span>
                        {{ item.name }}
                        <ion-icon class="testdata-list-icon" 
                        *ngIf="item.isTestData" name="logo-game-controller-a"></ion-icon>
                    </span>
                </ion-label>
            </ng-template>
            `,
    })
    class TestHostComponent {
        public title: string = 'My initial title';
        public listItemNamePlural: string;
        public entityLoaderService: any = mockTestEntityApiServiceStub;
        public viewModelConstructor: typeof TestEntityViewModel = TestEntityViewModel;

        @ViewChild(EntityListComponent, { static: true })
        public listComponent: EntityListComponent<TestEntityViewModel, TestEntityResourceModel>;
    }

    beforeEach(async () => {
        testData = [
            {
                id: '1',
                fullName: 'First Person',
                isTestData: false,
                status: 'New',
                createdDateTime: '2020-09-21T04:35:23.2904651Z',
                profilePictureId: '1',
                deleted: false,
            },
            {
                id: '2',
                fullName: 'Third Person',
                isTestData: false,
                status: 'New',
                createdDateTime: '2020-09-23T04:35:23.2904651Z',
                profilePictureId: '2',
                deleted: false,
            },
            {
                id: '3',
                fullName: 'Second Person',
                isTestData: false,
                status: 'New',
                createdDateTime: '2020-09-21T05:35:23.2904651Z',
                profilePictureId: '3',
                deleted: false,
            },
        ];

        mockTestEntityApiServiceStub = {
            getList: (params?: Map<string, string | Array<string>>): Observable<Array<TestEntityResourceModel>> => {
                return new BehaviorSubject<Array<TestEntityResourceModel>>(testData);
            },

            getById: (
                id: string,
                params?: Map<string,
                    string | Array<string>>,
            ): Observable<TestEntityResourceModel> => {
                return new BehaviorSubject<TestEntityResourceModel>(testData[id]);
            },
        };

        mockLayoutManager = {
            canShowFixedMenu: (): boolean => true,
            isMenuExpanded: (): boolean => true,
            getMasterViewComponentWidth: (): number => 600,
            splitPaneVisible: true,
            splitPaneEnabledSubject: new BehaviorSubject<boolean>(true),
        };

        mockRouteHelper = {
            getParam: (): any => null,
        };

        appConfigServiceStub = {
            get: (): void => { },
            getEnvironment: (): string => "Development",
            appConfigSubject: new Subject(),
        };

        mockAuthService = {
            isAgent: (): boolean => true,
        };

        mockPortalExtensionsService = {
            getEnabledPortalPageTriggers: (): any => [],
            getActionButtonPopovers: (): any => [],
        };

        await TestBed.configureTestingModule({
            declarations: [
                TestHostComponent,
            ],
            providers: [
                { provide: ChangeDetectorRef, useValue: ChangeDetectorRef },
                { provide: NavProxyService, useValue: {} },
                { provide: ModalController, useValue: {} },
                { provide: AlertController, useValue: {} },
                { provide: LoadDataService, useClass: LoadDataService },
                { provide: RouteHelper, useValue: mockRouteHelper },
                { provide: LayoutManagerService, useValue: mockLayoutManager },
                { provide: Router, useValue: {} },
                { provide: EventService, useClass: EventService },
                { provide: RepositoryRegistry, useClass: RepositoryRegistry },
                { provide: NavController, useValue: {} },
                { provide: AuthenticationService, useValue: mockAuthService },
                { provide: PortalExtensionsService, useValue: mockPortalExtensionsService },
                { provide: AppConfigService, useValue: appConfigServiceStub },
            ],
            schemas: [
                CUSTOM_ELEMENTS_SCHEMA,
            ],
            imports: [
                HttpClientModule,
                SharedModule,
                SharedComponentsModule,
                ListModule,
                NoopAnimationsModule,
            ],
        }).compileComponents().then(() => {
            fixture = TestBed.createComponent(TestHostComponent);
            sut = fixture.componentInstance;
        });
    });

    it('should render the title of the list page', () => {
        // Arrange

        // Act
        fixture.detectChanges();

        // Assert
        let titleElement: any = fixture.nativeElement.querySelector('ion-title');
        expect(titleElement.innerText).toBe('My initial title');
    });

    it('should set the title of the list page', () => {
        // Arrange
        sut.title = 'My new title';

        // Act
        fixture.detectChanges();

        // Assert
        let titleElement: any = fixture.nativeElement.querySelector('ion-title');
        expect(titleElement.innerText).toBe('My new title');
    });

    it('should load all the items into its repository', async () => {
        // Arrange

        // Act
        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');
        await sut.listComponent.load();
        fixture.detectChanges();

        // Assert
        expect(sut.listComponent.repository.pager).toBeTruthy('repository pager was null');
        expect(sut.listComponent.repository.boundList.length).toBe(3);
    });

    it('should create headers for each unique groupByValue when loading items into list', async () => {
        // Arrange

        // Act
        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');
        await sut.listComponent.load();
        fixture.detectChanges();

        // Assert
        expect(sut.listComponent.repository.pager).toBeTruthy('repository pager was null');
        expect(sut.listComponent.repository.boundList.length).toBe(3);
        expect(sut.listComponent.headers.length).toBe(2);
    });

    it('should sort entity list', async () => {
        // Arrange

        // Act
        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');
        await sut.listComponent.load();
        fixture.detectChanges();

        // Assert
        expect(sut.listComponent.repository.pager).toBeTruthy('repository pager was null');
        expect(sut.listComponent.repository.boundList.length).toBe(3);
        expect(sut.listComponent.repository.boundList[0].name).toBe('First Person');
        expect(sut.listComponent.repository.boundList[1].name).toBe('Second Person');
        expect(sut.listComponent.repository.boundList[2].name).toBe('Third Person');
    });

    it('should add a new header when a new item is added that needs a new header', async () => {
        // Arrange
        let newModel: TestEntityResourceModel = {
            id: '1',
            fullName: 'Fourth Person',
            isTestData: false,
            status: 'New',
            createdDateTime: '2020-09-24T04:35:23.2904651Z',
            profilePictureId: '1',
            deleted: false,
        };
        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');
        await sut.listComponent.load();
        fixture.detectChanges();
        expect(sut.listComponent.headers.length).toBe(2);

        // Act
        let eventService: EventService = TestBed.inject(EventService);
        eventService.getEntityCreatedSubject('TestEntity').next(newModel);

        // Assert        
        expect(sut.listComponent.repository.boundList.length).toBe(3);
        expect(sut.listComponent.headers.length).toBe(3);
    });

    it(
        'should add a new header when an existing item is updated and it\'s group by value changes to a unique value',
        async () => {
        // Arrange
            let updatedModel: TestEntityResourceModel = {
                id: '3',
                fullName: 'Second Person',
                isTestData: false,
                status: 'New',
                createdDateTime: '2020-09-24T05:35:23.2904651Z',
                profilePictureId: '3',
                deleted: false,
            };
            fixture.detectChanges();
            expect(sut.listComponent.repository).toBeTruthy('repository was null');
            await sut.listComponent.load();
            fixture.detectChanges();
            expect(sut.listComponent.headers.length).toBe(2);

            // Act
            let eventService: EventService = TestBed.inject(EventService);
            eventService.getEntityUpdatedSubject('TestEntity').next(updatedModel);

            // Assert        
            expect(sut.listComponent.repository.boundList.length).toBe(3);
            expect(sut.listComponent.headers.length).toBe(3);
        },
    );

    it('should delete an item from the list', async () => {
        // Arrange
        let deletedModel: TestEntityResourceModel = {
            id: '3',
            fullName: 'Second Person',
            isTestData: false,
            status: 'New',
            createdDateTime: '2020-09-24T05:35:23.2904651Z',
            profilePictureId: '3',
            deleted: true,
        };
        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');
        await sut.listComponent.load();
        fixture.detectChanges();
        expect(sut.listComponent.repository.boundList.length).toBe(3);

        // Act
        let eventService: EventService = TestBed.inject(EventService);
        eventService.getEntityUpdatedSubject('TestEntity').next(deletedModel);

        // Assert        
        expect(sut.listComponent.repository.boundList.length).toBe(2);
    });

    it('should delete empty headers after removing an item from the list', async () => {
        // Arrange
        let deletedModel: TestEntityResourceModel = {
            id: '2',
            fullName: 'Third Person',
            isTestData: false,
            status: 'New',
            createdDateTime: '2020-09-23T04:35:23.2904651Z',
            profilePictureId: '2',
            deleted: true,
        };

        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');
        await sut.listComponent.load();
        fixture.detectChanges();
        expect(sut.listComponent.headers.length).toBe(2);

        // Act
        let eventService: EventService = TestBed.inject(EventService);
        eventService.getEntityUpdatedSubject('TestEntity').next(deletedModel);

        // Assert        
        expect(sut.listComponent.headers.length).toBe(1);
    });

    it('should delete empty headers after updating an item from the list', async () => {
        // Arrange
        let deletedModel: TestEntityResourceModel = {
            id: '2',
            fullName: 'Third Person',
            isTestData: false,
            status: 'New',
            createdDateTime: '2020-09-21T04:35:23.2904651Z',
            profilePictureId: '2',
            deleted: false,
        };

        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');
        await sut.listComponent.load();
        fixture.detectChanges();
        expect(sut.listComponent.headers.length).toBe(2);

        // Act
        let eventService: EventService = TestBed.inject(EventService);
        eventService.getEntityUpdatedSubject('TestEntity').next(deletedModel);

        // Assert        
        expect(sut.listComponent.headers.length).toBe(1);
    });

    it('should load the data once in the list then expect has headers when loading', async () => {
        // Arrange
        fixture.detectChanges();
        expect(sut.listComponent.repository).toBeTruthy('repository was null');

        // Act
        if (!sut.listComponent.repository.isDataLoading) {
            await sut.listComponent.load();
        }
        fixture.detectChanges();

        // Assert      
        expect(sut.listComponent.repository.boundList.length).toBe(3);
        expect(sut.listComponent.repository.isDataLoading).toBeTruthy();
        expect(sut.listComponent.headers.length).toBeGreaterThanOrEqual(1);
    });
});
