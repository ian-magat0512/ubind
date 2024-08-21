import { ChangeDetectorRef, Component, ElementRef, Injector, OnInit, AfterViewChecked, OnDestroy } from "@angular/core";
import { FormArray, FormBuilder, FormControl, FormGroup } from "@angular/forms";
import { QuoteTypeFilter } from "@app/models/quote-type-filter.enum";
import { QuoteTypeNames } from "@app/models/quote-type.enum";
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
 * Export quote filter component class
 * To filter the data in the quote list.
 */
@Component({
    selector: 'app-quote-filter-sort',
    templateUrl: './quote-filter.component.html',
    styleUrls: [
        '../../../assets/css/scrollbar-form.css',
        '../../../assets/css/form-toolbar.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class QuoteFilterComponent extends FilterSortPage implements OnInit, AfterViewChecked, OnDestroy {

    private quoteTypes: Array<Checkbox> = [];
    private quoteTypeControls: Array<FormControl>;
    private quoteTypesList: Array<any> = [];
    private quoteTypesOptions: Array<any> = [];
    private products: Array<Checkbox> = [];
    private productControls: Array<FormControl>;
    private productList: Array<any> = [];
    private productOptions: Array<any> = [];
    private quoteParams: any;
    public filterQuoteTitle: string;
    public quoteFilterSortForm: FormGroup;
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
        if (this.quoteParams == null) {
            this.navigateBack();
            return;
        }

        this.isLoading = this.quoteParams.isProductLoading;
        this.quoteTypesList = this.quoteParams.quoteTypesList;
        this.productList = this.quoteParams.productList;
        this.statusTitle = this.quoteParams.statusTitle;

        super.initializeFormComponents(this.quoteParams);

        // Product Control Values
        this.setProductFieldOption();

        // Quote Types Control Values
        this.quoteTypesList.forEach((item: any) => {
            this.quoteTypesOptions.push({
                label: item.status === QuoteTypeFilter.NewBusiness ?
                    QuoteTypeNames.NewBusiness : item.status,
                value: item.value,
            });
        });
        this.fieldOptions.push({ name: 'quoteTypeList', type: 'checkbox', options: this.quoteTypesOptions });

        // Test Data
        this.isTestDataIncluded = this.quoteParams.testData;

        // Form Body
        this.quoteFilterSortForm = this.createForm();
    }

    private setProductFieldOption(): void {
        if (this.productList) {
            this.productOptions = [];
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
                    this.quoteFilterSortForm = this.createForm();
                    this.isLoading = false;
                }
            });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.subscribeToEnvironmentChange();

        // Intialize Form Data and Components
        this.quoteParams = this.routeHelper.navigationExtras.state;
        if (!this.quoteParams) {
            this.navigateBack();
            return;
        }

        // Page setting values
        this.entityTypeName = this.quoteParams.entityTypeName;
        this.selectedId = this.quoteParams.selectedId;

        this.listenProductFilterUpdate();
        this.buildForm();

        // Page Title
        this.filterQuoteTitle = this.quoteParams.filterTitle;

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

        // Quote Type Filter
        this.filterSortDetails.push(DetailsListFormCheckboxGroupItem.create(
            filterOptionsCard,
            'quoteTypeList',
            'Quote Type'));

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

    protected setQuoteTypeControl(): void {
        this.quoteTypes = [
            new Checkbox(
                QuoteTypeFilter.NewBusiness,
                QuoteTypeNames.NewBusiness,
                this.getQuoteTypeCheckBoxValue(QuoteTypeFilter.NewBusiness),
            ),
            new Checkbox(
                QuoteTypeFilter.Adjustment,
                QuoteTypeNames.Adjustment,
                this.getQuoteTypeCheckBoxValue(QuoteTypeFilter.Adjustment),
            ),
            new Checkbox(
                QuoteTypeFilter.Renewal,
                QuoteTypeNames.Renewal,
                this.getQuoteTypeCheckBoxValue(QuoteTypeFilter.Renewal),
            ),
            new Checkbox(
                QuoteTypeFilter.Cancellation,
                QuoteTypeNames.Cancellation,
                this.getQuoteTypeCheckBoxValue(QuoteTypeFilter.Cancellation),
            ),
        ];
        this.quoteTypeControls = this.quoteTypes.map((c: Checkbox) => new FormControl(c.value));
    }

    protected getQuoteTypeCheckBoxValue(quoteTypeName: string): boolean {
        if (this.quoteTypesList) {
            return this.quoteTypesList.some((quoteType: any) =>
                quoteType.status === quoteTypeName && quoteType.value === true);
        }
        return false;
    }

    public createForm(): FormGroup {

        this.setProductControl();
        this.setQuoteTypeControl();

        return this.formBuilder.group({
            sortBy: new FormControl(this.sortBy),
            sortOrder: new FormControl(this.sortOrder),
            dateFilteringPropertyName: new FormControl(this.dateFilteringPropertyName),
            dateIsAfter: [this.dateIsAfter ? this.dateIsAfter.substr(0, this.dateIsAfter.indexOf('T')) : ''],
            dateIsBefore: [this.dateIsBefore ? this.dateIsBefore.substr(0, this.dateIsBefore.indexOf('T')) : ''],
            productList: this.productControls ? new FormArray(this.productControls) : [],
            quoteTypeList: new FormArray(this.quoteTypeControls),
            statusList: new FormArray(this.statusControls),
            dataOptions: new FormArray(this.dataOptionControls),
        });
    }

    public returnToPrevious(): void {
        this.navigateBack();
    }

    public applyChanges(formValues: any): void {
        let isValid: boolean = super.validateFilterByDate(formValues, this.quoteFilterSortForm, this.filterSortDetails);
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

        if (formValues.quoteTypeList.some((x: boolean) => x)) {
            for (const index in formValues.quoteTypeList) {
                if (formValues.quoteTypeList[index]) {
                    filterSelections.push(FilterHelper.createFilterSelection(
                        'quoteTypes',
                        this.quoteTypes[index].id,
                        this.quoteTypes[index].name,
                        IconLibrary.AngularMaterial,
                    ));
                }
            }
        }

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
