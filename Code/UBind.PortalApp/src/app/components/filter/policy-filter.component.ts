import {
    ChangeDetectorRef, Component, ElementRef,
    Injector, OnInit, AfterViewChecked, OnDestroy,
} from "@angular/core";
import { FormArray, FormBuilder, FormControl, FormGroup } from "@angular/forms";
import { Checkbox } from "@app/viewmodels/checkbox.viewmodel";
import { FilterSelection } from "@app/viewmodels/filter-selection.viewmodel";
import { scrollbarStyle } from "@assets/scrollbar";
import { FilterSortPage } from "@pages/filter-sort/filter-sort-page";
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Router } from '@angular/router';
import { RouteHelper } from '@app/helpers/route.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { EventService } from "@app/services/event.service";
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { UserType } from "@app/models/user-type.enum";
import { AuthenticationService } from '@app/services/authentication.service';
import { FilterHelper } from "@app/helpers/filter.helper";
import { ProductFilter } from "@app/models/product-filter";
import { AppConfigService } from "@app/services/app-config.service";
import { DeploymentEnvironment } from "@app/models/deployment-environment.enum";
import { Subject } from "rxjs";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailsListFormSelectItem } from "@app/models/details-list/details-list-form-select-item";
import { DetailsListFormDateItem } from "@app/models/details-list/details-list-form-date-item";
import { DetailsListFormCheckboxGroupItem } from "@app/models/details-list/details-list-form-checkbox-group-item";
import { SortFilterHelper } from "@app/helpers/sort-filter.helper";

/**
 * Export policy filter component class
 * To filter the data in the policy list.
 */
@Component({
    selector: 'app-policy-filter-sort',
    templateUrl: './policy-filter.component.html',
    styleUrls: [
        '../../../assets/css/scrollbar-form.css',
        '../../../assets/css/form-toolbar.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class PolicyFilterComponent extends FilterSortPage implements OnInit, AfterViewChecked, OnDestroy {

    private products: Array<Checkbox> = [];
    private productControls: Array<FormControl>;
    private productList: Array<any> = [];
    private productOptions: Array<any> = [];
    private policyParams: any;
    public filterPolicyTitle: string;
    public policyFilterSortForm: FormGroup;
    public isLoading: boolean;
    public constructor(
        cdRef: ChangeDetectorRef,
        navProxy: NavProxyService,
        formBuilder: FormBuilder,
        layoutManager: LayoutManagerService,
        router: Router,
        routeHelper: RouteHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        authService: AuthenticationService,
        appConfigService: AppConfigService,
    ) {
        super(
            cdRef,
            navProxy,
            formBuilder,
            layoutManager,
            router,
            routeHelper,
            eventService,
            elementRef,
            injector,
            authService,
            appConfigService,
        );
    }

    public ngAfterViewChecked(): void {
        this.cdRef.detectChanges();
    }

    private buildForm(): void {
        if (this.policyParams == null) {
            this.navigateBack();
            return;
        }

        this.isLoading = this.policyParams.isProductLoading;
        this.productList = this.policyParams.productList;
        this.statusTitle = this.policyParams.statusTitle;

        super.initializeFormComponents(this.policyParams);

        // Product Control Values
        this.setProductFieldOption();

        // Test Data
        this.isTestDataIncluded = this.policyParams.testData;

        // Form Body
        this.policyFilterSortForm = this.createForm();
    }

    private setProductFieldOption(): void {
        if (this.productList) {
            this.productList.forEach((item: any) => {
                this.productOptions.push({
                    label: item.name,
                    value: item.value,
                });
            });
            this.fieldOptions.push({ name: 'productList', type: 'checkbox', options: this.productOptions });
        }
    }

    private listenProductFilterUpdate(): void {
        this.eventService.productFilterUpdateSubject$
            .subscribe((productFilterList: Array<ProductFilter>) => {
                if (!this.productList.length && this.isLoading) {
                    this.productList = productFilterList;
                    this.setProductFieldOption();
                    this.policyFilterSortForm = this.createForm();
                    this.isLoading = false;
                }
            });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.subscribeToEnvironmentChange();

        // Intialize Form Data and Components
        this.policyParams = this.routeHelper.navigationExtras.state;
        if (!this.policyParams) {
            this.navigateBack();
            return;
        }

        // Page setting values
        this.entityTypeName = this.policyParams.entityTypeName;
        this.selectedId = this.policyParams.selectedId;

        this.listenProductFilterUpdate();
        this.buildForm();

        // Page Title
        this.filterPolicyTitle = this.policyParams.filterTitle;

        // Control Groups
        const sortOptionsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Sort,
            'Sort Options');
        const filterOptionsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Filters,
            'Filter Options');

        // Sort Options
        this.filterSortDetails.push(DetailsListFormSelectItem.create(
            sortOptionsCard,
            'sortBy',
            'Sort by')
            .withIcon('sort', IconLibrary.AngularMaterial)
            .withHeader('Sort Options'));

        this.filterSortDetails.push(DetailsListFormSelectItem.create(
            sortOptionsCard,
            'sortOrder',
            'Sort order'));

        // Filter Options - Dates
        this.filterSortDetails.push(DetailsListFormSelectItem.create(
            filterOptionsCard,
            SortFilterHelper.dateFilteringPropertyName,
            'Filter by date')
            .withIcon('filter', IconLibrary.AngularMaterial)
            .withHeader('Filter Options'));
        this.filterSortDetails.push(DetailsListFormDateItem.create(
            filterOptionsCard,
            'dateIsAfter',
            'Date is after'));
        this.filterSortDetails.push(DetailsListFormDateItem.create(
            filterOptionsCard,
            'dateIsBefore',
            'Date is before'));

        // Product Filter
        this.filterSortDetails.push(DetailsListFormCheckboxGroupItem.create(
            filterOptionsCard,
            'productList',
            'Product'));

        // Status Filter
        this.filterSortDetails.push(DetailsListFormCheckboxGroupItem.create(
            filterOptionsCard,
            'statusList',
            'Status'));

        // Test Data Filter
        if (this.userRole === UserType.Client) {
            let currentEnvironment: DeploymentEnvironment = this.appConfigService.getEnvironment();
            this.testDataFormItem = DetailsListFormCheckboxGroupItem.create(
                filterOptionsCard,
                'dataOptions',
                'Data Options');
            this.filterSortDetails.push(this.testDataFormItem);
            this.setTestDataVisibility(this.testDataFormItem, currentEnvironment);
        }
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    protected setProductControl(): void {
        this.products = [];
        if (this.productList) {
            this.productList.forEach((item: any) => {
                this.products.push(new Checkbox(item.id, item.name, item.value));
            });
            this.productControls = this.products.map((c: Checkbox) => new FormControl(c.value));
        }
    }

    public createForm(): FormGroup {

        this.setProductControl();

        return this.formBuilder.group({
            sortBy: new FormControl(this.sortBy),
            sortOrder: new FormControl(this.sortOrder),
            dateFilteringPropertyName: new FormControl(this.dateFilteringPropertyName),
            dateIsAfter: [this.dateIsAfter ? this.dateIsAfter.substr(0, this.dateIsAfter.indexOf('T')) : ''],
            dateIsBefore: [this.dateIsBefore ? this.dateIsBefore.substr(0, this.dateIsBefore.indexOf('T')) : ''],
            productList: new FormArray(this.productControls),
            statusList: new FormArray(this.statusControls),
            dataOptions: new FormArray(this.dataOptionControls),
        });
    }

    public returnToPrevious(): void {
        this.navigateBack();
    }

    public applyChanges(formValues: any): void {
        let isValid: boolean = super.validateFilterByDate(
            formValues,
            this.policyFilterSortForm,
            this.filterSortDetails);
        if (isValid) {
            return;
        }

        let filters: Array<FilterSelection> = this.gatherSelections(formValues);
        this.broadcastFilterSelection(filters);
        this.navigateBack();
    }

    protected broadcastFilterSelection(filters: Array<FilterSelection>): void {
        this.eventService.getEntityFilterChangedSubject(this.entityTypeName).next(filters);
    }

    protected navigateBack(): void {
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        if (this.selectedId) {
            this.navProxy.navigateBack([pathSegments[pathSegments.length - 1], this.selectedId]);
        } else {
            this.navProxy.navigateBack([pathSegments[pathSegments.length - 1], 'list']);
        }
    }

    protected gatherSelections(formValues: any): Array<FilterSelection> {
        let filterSelections: Array<FilterSelection> = [];
        filterSelections = super.gatherSelections(formValues);

        if (formValues.productList.some((x: boolean) => x)) {
            for (const index in formValues.productList) {
                if (formValues.productList[index]) {
                    filterSelections.push(FilterHelper.createFilterSelection(
                        'productIds',
                        this.products[index].id,
                        this.products[index].name,
                    ));
                }
            }
        }
        return filterSelections;
    }
}
