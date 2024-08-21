import {
    ChangeDetectorRef, Component, ElementRef,
    Injector, OnInit, AfterViewChecked, OnDestroy,
} from '@angular/core';
import { FormBuilder, FormControl, FormArray, FormGroup, AbstractControl } from '@angular/forms';
import { scrollbarStyle } from '@assets/scrollbar';
import { LocalDateHelper } from '@app/helpers';
import { FilterSelection } from '@app/viewmodels/filter-selection.viewmodel';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Router } from '@angular/router';
import { RouteHelper } from '@app/helpers/route.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { EventService } from "@app/services/event.service";
import { PageWithMaster } from "@pages/master-detail/page-with-master";
import { IonicLifecycleEventReplayBus } from "@app/services/ionic-lifecycle-event-replay-bus";
import { EntityEditFieldOption, FieldOption } from '@app/models/entity-edit-field-option';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { UserType } from '@app/models/user-type.enum';
import { AuthenticationService } from '@app/services/authentication.service';
import { FilterHelper } from '@app/helpers/filter.helper';
import { SortOption } from '@app/components/filter/sort-option';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { EnvironmentChange } from '@app/models/environment-change';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { DetailsListFormDateItem } from '@app/models/details-list/details-list-form-date-item';
import { DetailsListFormSelectItem } from '@app/models/details-list/details-list-form-select-item';
import { DetailsListFormCheckboxGroupItem } from '@app/models/details-list/details-list-form-checkbox-group-item';

/**
 * Export filter sort page component class
 * This class component manage the filtering and sorting
 * of data in the list.
 */
@Component({
    selector: 'app-filter-sort',
    templateUrl: './filter-sort-page.html',
    styleUrls: [
        '../../../assets/css/scrollbar-form.css',
        '../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class FilterSortPage implements PageWithMaster, OnInit, AfterViewChecked, OnDestroy {
    private params: any;
    private sortOptions: SortOption;
    private defaultFilterBy: string = "None";
    private filterByDates: Array<string> = [this.defaultFilterBy];
    protected greaterThan: string = '> ';
    protected lessThan: string = '< ';
    protected filterByDateOptions: Array<FieldOption> = [];
    protected sortByOptions: Array<FieldOption> = [];
    protected sortOrderOptions: Array<FieldOption> = [];
    protected sortBy: string;
    protected sortOrder: string;
    protected dateFilteringPropertyName: string;
    protected dateIsAfter: string;
    protected dateIsBefore: string;
    protected statusControls: Array<FormControl>;
    protected dataOptionControls: Array<FormControl> = new Array<FormControl>();
    protected userRole: string = null;
    protected selectedId: string = null;
    protected statusList: Array<any> = [];
    protected isTestDataIncluded: boolean;
    protected isTestDataVisible: boolean;
    protected statusTitle: string = '';
    public saveTitle: string = "Apply";
    public ionicLifecycleEventReplayBus: IonicLifecycleEventReplayBus;
    public statusTypes: Array<FieldOption> = [];
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public filterSortDetails: Array<DetailsListFormItem> = [];
    public entityTypeName: string = null;
    public filterSortForm: FormGroup;
    public defaultSortBy: string;
    public defaultSortOrder: string;
    public filterSortTitle: string = null;
    protected destroyed: Subject<void>;
    protected testDataFormItem: DetailsListFormCheckboxGroupItem;

    public constructor(
        protected cdRef: ChangeDetectorRef,
        protected navProxy: NavProxyService,
        protected formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        public router: Router,
        public routeHelper: RouteHelper,
        protected eventService: EventService,
        protected elementRef: ElementRef,
        public injector: Injector,
        private authService: AuthenticationService,
        protected appConfigService: AppConfigService,
    ) {
    }

    public ngAfterViewChecked(): void {
        this.cdRef.detectChanges();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.detailComponentCreated(this);
        this.subscribeToEnvironmentChange();

        // Initialize Form Data and Components
        this.params = this.routeHelper.navigationExtras.state;
        if (this.params == null) {
            this.returnToPrevious();
            return;
        }

        this.initializeFormComponents(this.params);

        // Filter and Sort Page setting values    
        this.selectedId = this.params.selectedId;
        this.entityTypeName = this.params.entityTypeName;

        // Page Titles
        this.filterSortTitle = this.params.filterTitle;
        this.statusTitle = this.params.statusTitle;

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

        // Status Filter
        if (this.statusControls && this.statusControls.length > 0) {
            this.filterSortDetails.push(DetailsListFormCheckboxGroupItem.create(
                filterOptionsCard,
                'statusList',
                'Status'));
        }

        // Test Data Filter
        if (this.userRole === UserType.Client && this.dataOptionControls && this.dataOptionControls.length > 0) {
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

    protected subscribeToEnvironmentChange(): void {
        this.eventService.environmentChangedSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((ec: EnvironmentChange) => {
                this.setTestDataVisibility(this.testDataFormItem, ec.newEnvironment);
            });
    }

    protected setTestDataVisibility(
        testDataDetail: DetailsListFormItem,
        environment: DeploymentEnvironment): void {
        if (testDataDetail) {
            this.isTestDataVisible = environment === DeploymentEnvironment.Production;
            testDataDetail.Visible = this.isTestDataVisible;
        }
    }

    protected initializeFormComponents(params: any): void {
        if (params == null) {
            this.returnToPrevious();
            return;
        }

        // Client Admin's Option
        this.userRole = this.authService.userType;

        // Sort Controls Value
        this.filterByDates = params.filterByDates
            ? this.filterByDates.concat(params.filterByDates)
            : this.filterByDates;
        this.sortOptions = params.sortOptions;
        let selectedOptions: any = params.selectedSortOption;
        this.defaultSortBy = this.sortOptions.sortBy[0];
        this.defaultSortOrder = this.sortOptions.sortOrder[0];
        this.sortBy = selectedOptions.sortBy.length > 0 ? selectedOptions.sortBy[0] : this.defaultSortBy;
        this.dateFilteringPropertyName = params.selectedDateFilteringPropertyName
            ? params.selectedDateFilteringPropertyName : this.filterByDates[0];
        this.sortOrder = selectedOptions.sortOrder.length > 0
            ? selectedOptions.sortOrder[0] : this.defaultSortOrder;

        if (this.sortOptions.sortBy.length > 0) {
            this.sortOptions.sortBy.forEach((item: any) => {
                this.sortByOptions.push({ label: item, value: item });
            });
            this.fieldOptions.push({ name: SortFilterHelper.sortBy, type: "select", options: this.sortByOptions });
        }

        if (this.sortOptions.sortOrder.length > 0) {
            this.sortOptions.sortOrder.forEach((item: any) => {
                this.sortOrderOptions.push({ label: item, value: item });
            });
            this.fieldOptions.push({ name: "sortOrder", type: "select", options: this.sortOrderOptions });
        }

        if (this.filterByDates.length > 0) {
            this.filterByDates.forEach((item: any) => {
                this.filterByDateOptions.push({ label: item, value: item });
            });
            this.fieldOptions.push({
                name: SortFilterHelper.dateFilteringPropertyName,
                type: "select",
                options: this.filterByDateOptions,
            });
        }

        // Status Controls Value
        this.statusList = params.statusList;
        this.statusList.forEach((item: any) => {
            this.statusTypes.push({ label: item.status, value: item.value });
        });
        this.statusControls = this.statusTypes.map((c: FieldOption) => new FormControl(c.value));

        if (this.statusList?.length > 0) {
            this.fieldOptions.push({ name: "statusList", type: "checkbox", options: this.statusTypes });
        }

        // Test Data Control
        this.isTestDataIncluded = params.testData;
        if (params.testData !== undefined) {
            if (this.userRole === UserType.Client) {
                this.dataOptionControls = [new FormControl(this.isTestDataIncluded.toString())];
                this.fieldOptions.push({
                    name: "dataOptions",
                    type: "checkbox",
                    options: [
                        {
                            label: "Include Test Data",
                            value: this.isTestDataIncluded.toString(),
                        },
                    ],
                });
            }
        }

        // Date Controls Value
        this.dateIsAfter = params.dateIsAfter ?
            new Date(params.dateIsAfter.replace(this.greaterThan, '') + 'UTC').toISOString() : '';
        this.dateIsBefore = params.dateIsBefore ?
            new Date(params.dateIsBefore.replace(this.lessThan, '') + 'UTC').toISOString() : '';

        // Form Body
        this.filterSortForm = this.createForm();
    }

    public createForm(): FormGroup {
        return this.formBuilder.group({
            sortBy: new FormControl(this.sortBy),
            sortOrder: new FormControl(this.sortOrder),
            dateFilteringPropertyName: new FormControl(this.dateFilteringPropertyName),
            dateIsAfter: [this.dateIsAfter ? this.dateIsAfter.substr(0, this.dateIsAfter.indexOf('T')) : ''],
            dateIsBefore: [this.dateIsBefore ? this.dateIsBefore.substr(0, this.dateIsBefore.indexOf('T')) : ''],
            statusList: new FormArray(this.statusControls),
            dataOptions: new FormArray(this.dataOptionControls),
        });
    }

    public applyChanges(formValues: any): void {
        let isValid: boolean = this.validateFilterByDate(formValues, this.filterSortForm, this.filterSortDetails);
        if (isValid) {
            return;
        }

        let filters: Array<FilterSelection> = this.gatherSelections(formValues);
        this.broadcastFilterSelection(filters);
        this.navigateBack();
    }

    public returnToPrevious(): void {
        this.navigateBack();
    }

    protected navigateBack(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push("list");
        this.navProxy.navigateBack(pathSegments);
    }

    protected broadcastFilterSelection(filters: Array<FilterSelection>): void {
        this.eventService.getEntityFilterChangedSubject(this.entityTypeName).next(filters);
    }

    protected gatherSelections(formValues: any): Array<FilterSelection> {

        const filterSelections: Array<FilterSelection> = [];
        let propertyIsDate: boolean =
            SortFilterHelper.determineSortAndFilterPropertyIsDate(this.defaultSortBy);

        if (formValues.sortBy.length > 0 && formValues.sortBy != this.defaultSortBy) {
            filterSelections.push(FilterHelper.createFilterSelection(
                SortFilterHelper.sortBy,
                formValues.sortBy,
                formValues.sortBy,
                IconLibrary.AngularMaterial,
            ));

            propertyIsDate =
                SortFilterHelper.determineSortAndFilterPropertyIsDate(formValues.sortBy);
        }

        if (formValues.sortOrder.length > 0
            && (formValues.sortOrder != this.defaultSortOrder
                || formValues.sortBy != this.defaultSortBy)) {
            filterSelections.push(FilterHelper.createFilterSelection(
                'sortOrder',
                formValues.sortOrder,
                formValues.sortOrder,
                IconLibrary.AngularMaterial,
                propertyIsDate,
            ));
        }

        formValues.statusList.map((x: any, index: any) => {
            this.statusList[index].value = x;
        });

        if (formValues.statusList.some((x: boolean) => x === true)) {
            for (let i: number = 0; i < formValues.statusList.length; i++) {
                if (formValues.statusList[i] === true) {
                    filterSelections.push(FilterHelper.createFilterSelection('status', this.statusList[i].status));
                }
            }
        }

        if (formValues.dateFilteringPropertyName.length > 0
            && formValues.dateFilteringPropertyName != this.defaultFilterBy
            && (formValues.dateIsAfter.length > 0
                || formValues.dateIsBefore.length > 0)) {
            filterSelections.push(FilterHelper.createFilterSelection(
                SortFilterHelper.dateFilteringPropertyName,
                formValues.dateFilteringPropertyName,
                formValues.dateFilteringPropertyName,
                IconLibrary.AngularMaterial,
            ));
        }

        if (formValues.dateIsAfter.length > 0) {
            const dateIsAfterValue: string = this.greaterThan + LocalDateHelper.toLocalDate(formValues.dateIsAfter);
            filterSelections.push(FilterHelper.createFilterSelection(
                'afterDateTime',
                dateIsAfterValue,
                dateIsAfterValue,
                IconLibrary.AngularMaterial,
            ));
        }

        if (formValues.dateIsBefore.length > 0) {
            const dateIsBeforeValue: string = this.lessThan + LocalDateHelper.toLocalDate(formValues.dateIsBefore);
            filterSelections.push(FilterHelper.createFilterSelection(
                'beforeDateTime',
                dateIsBeforeValue,
                dateIsBeforeValue,
                IconLibrary.AngularMaterial,
            ));
        }

        if (this.isTestDataIncluded !== undefined
            && this.isTestDataVisible
            && formValues.dataOptions.length > 0 && formValues.dataOptions[0]
        ) {
            filterSelections.push(FilterHelper.createFilterSelection(
                'includeTestData',
                formValues.dataOptions[0],
                "Test Data",
                IconLibrary.AngularMaterial,
            ));
        }

        return filterSelections;
    }

    protected validateFilterByDate(
        formValues: any,
        filterSortForm: FormGroup,
        detailListItem: Array<DetailsListFormItem>,
    ): boolean {
        const filterByDateControl: AbstractControl = filterSortForm.get(SortFilterHelper.dateFilteringPropertyName);

        if ((formValues.dateFilteringPropertyName.length > 0
            && formValues.dateFilteringPropertyName == this.defaultFilterBy)
            && (formValues.dateIsAfter.length > 0
                || formValues.dateIsBefore.length > 0)) {
            let dateFilterItem: DetailsListFormItem =
                detailListItem.find((item: DetailsListFormItem) =>
                    item.Alias == SortFilterHelper.dateFilteringPropertyName);
            filterByDateControl.markAsTouched({ onlySelf: true });
            filterByDateControl.setErrors({ invalidSelect: true });
            dateFilterItem?.FormControl.markAsTouched({ onlySelf: true });
            dateFilterItem?.FormControl.setErrors({ invalidSelect: true });
            return true;
        }

        return false;
    }
}
