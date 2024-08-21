import { Component } from '@angular/core';
import { ModalController, NavParams } from '@ionic/angular';
import { Checkbox } from "@app/viewmodels/checkbox.viewmodel";
import { FormBuilder, FormControl, FormArray, FormGroup, AbstractControl } from '@angular/forms';
import { scrollbarStyle } from '@assets/scrollbar';
import { LocalDateHelper } from '@app/helpers';
import { FilterSelection } from '@app/viewmodels/filter-selection.viewmodel';
import { FilterHelper } from '@app/helpers/filter.helper';

/**
 * Export filter component class
 * To filter the data in the list.
 */
@Component({
    selector: 'filter',
    templateUrl: './filter.component.html',
    styleUrls: [
        './filter.component.scss',
        '../../../assets/css/scrollbar-div.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class FilterComponent {
    public filterTitle: string = '';
    public statusTitle: string = '';
    public nullValues: boolean = true;
    public filterForm: FormGroup;
    public statuses: Array<Checkbox> = [];
    public userType: any = '';
    public userRole: any = '';
    private greaterThan: string = '> ';
    private lessThan: string = '< ';
    protected model: any;
    protected statusControls: Array<FormControl>;
    protected afterDate: string;
    protected beforeDate: string;

    public constructor(
        protected navParams: NavParams,
        protected formBuilder: FormBuilder,
        protected modalCtrl: ModalController,
    ) {
        this.filterTitle = navParams.get('filterTitle');
        this.statusTitle = navParams.get('statusTitle');
        this.model = navParams.get('dateData');
        const statusList: Array<any> = navParams.get('statusList');
        if (statusList) {
            statusList.forEach((element: any) => {
                this.statuses.push(new Checkbox(element.status, element.status, element.value));
            });
        }

        this.statusControls = this.statuses.map((c: Checkbox) => new FormControl(c.value));
        this.afterDate = this.model.after ?
            new Date(this.model.after.replace(this.greaterThan, '') + 'UTC').toISOString() : '';
        this.beforeDate = this.model.before ?
            new Date(this.model.before.replace(this.lessThan, '') + 'UTC').toISOString() : '';
        this.createForm();
    }

    protected createForm(): void {
        this.filterForm = this.formBuilder.group({
            createdAfter: [this.afterDate ? this.afterDate.substr(0, this.afterDate.indexOf('T')) : ''],
            createdBefore: [this.beforeDate ? this.beforeDate.substr(0, this.beforeDate.indexOf('T')) : ''],
            statusList: new FormArray(this.statusControls),
            includeTestData: new FormControl(false),
        });
    }

    public getControls(field: string): Array<AbstractControl> {
        return (this.filterForm.get(field) as FormArray).controls;
    }

    public cancel(): void {
        this.modalCtrl.dismiss();
    }

    public applyChanges(): void {
        const filterSelections: Array<FilterSelection> = [];
        this.gatherSelections(filterSelections);
        this.modalCtrl.dismiss(filterSelections);
    }

    protected gatherSelections(filterSelections: Array<FilterSelection>): void {
        const formValues: any = this.filterForm.value;
        if (formValues.createdAfter.length > 0) {
            filterSelections.push(FilterHelper.createFilterSelection(
                'createdAfterDateTime',
                this.greaterThan + LocalDateHelper.toLocalDate(formValues.createdAfter),
            ));
        }
        if (formValues.createdBefore.length > 0) {
            filterSelections.push(FilterHelper.createFilterSelection(
                'createdBeforeDateTime',
                this.lessThan + LocalDateHelper.toLocalDate(formValues.createdBefore),
            ));
        }
        if (formValues.statusList.some((x: boolean) => x === true)) {
            for (let i: number = 0; i < formValues.statusList.length; i++) {
                if (formValues.statusList[i] === true) {
                    filterSelections.push(FilterHelper.createFilterSelection(
                        'status',
                        this.statuses[i].id,
                    ));
                }
            }
        }
        if (formValues.includeTestData) {
            filterSelections.push(FilterHelper.createFilterSelection('includeTestData', formValues.includeTestData));
        }
    }

    public determineWhetherApplyButtonShouldBeEnabled(): void {
        this.nullValues = !this.filterForm.dirty;
    }
}
