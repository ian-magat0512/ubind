import { FormGroup, FormControl, ValidatorFn, AbstractControl } from '@angular/forms';
import {
    Component, Input, Output, EventEmitter, OnChanges, OnInit, ContentChildren, QueryList, AfterContentInit,
} from '@angular/core';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import {
    RepeatingFieldResourceModel, RepeatingAddressFieldResourceModel,
} from '@app/resource-models/repeating-field.resource-model';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { AddressPart } from '@app/models/address-enum';
import { Permission } from '@app/helpers';
import { EntityEditFieldOption, FieldOption, FieldShowHideRule } from '@app/models/entity-edit-field-option';
import { IonicHelper } from '@app/helpers/ionic.helper';
import { contentAnimation } from '@assets/animations';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { Subscription } from 'rxjs';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DomSanitizer } from '@angular/platform-browser';

/**
 * A group of properties to edit.
 */
interface Group {
    icon: string;
    name: string;
    expanded: boolean;
    header?: string;
    subHeader?: string;
    items: Array<DetailsListFormItem>;
    isCompleted: boolean;
    isSubFormGroup?: boolean;
    allowedPermissions?: Permission | Array<Permission>;
    includeSectionIcons: boolean;
}

/**
 * Export details list item edit form component class
 * This class is for details list edit forms functions.
 */
@Component({
    selector: 'app-detail-list-item-edit-form',
    templateUrl: './detail-list-item-edit-form.component.html',
    styleUrls: ['./detail-list-item-edit-form.component.scss',
        '../../../assets/css/scrollbar-form.css',
        '../../../assets/css/form-toolbar.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
    animations: [contentAnimation],
})
export class DetailListItemsEditFormComponent implements OnInit, OnChanges, AfterContentInit {
    public itemGroups: Array<Group> = [];
    @Input() public items: Array<DetailsListFormItem>;
    @Input() public editForm: FormGroup;
    @Input() public model: any;
    @Input() public title: any;
    @Input() public isEdit: boolean;
    @Input() public repeatingFields: Array<RepeatingFieldResourceModel> = [];
    @Input() public fieldsOptions: Array<EntityEditFieldOption> = [];
    @Input() public saveTitle: string = null;
    @Input() public isLoading: boolean;
    @Input() public errorMessage: string;
    @Input() public fieldShowHideRules: Array<FieldShowHideRule> = [];
    @Output() public saveForm: EventEmitter<any> = new EventEmitter();
    @Output() public closeForm: EventEmitter<any> = new EventEmitter();
    @Input() public callbackCustomValidation: (value: any) => boolean = null;
    @Input() public useNativeDateTimeControls: boolean = true;

    @ContentChildren('content')
    public contentlist: QueryList<any>;
    public hasProjectedContent: boolean = false;

    public isFirstItemVisible: boolean;
    public fieldName: string; // need to check
    public isCustomLabelFocused: boolean;
    public setAriaLabel: any = IonicHelper.setAriaLabel;
    public itemGroupActionIcons: Array<any>;
    public isShowHiddenNameParts: boolean;
    public hasAccordionToggle: boolean;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    private editFormConstants: any = {
        required: "required",
        canShow: "canShow",
        options: "options",
        repeatingFields: "repeatingFields",
        groupValue: "groupValue",
        label: "_label",
        customLabel: "_customLabel",
        other: "other",
        address: "address",
        formContainer: "formContainer",
    };

    public constructor(public layoutManager: LayoutManagerService, private sanitizer: DomSanitizer) {
        // Nothing to do
    }

    public ngOnChanges(): void {
        this.initItemGroups();
    }

    public ngAfterContentInit(): void {
        this.hasProjectedContent = this.contentlist.length > 0;
    }

    public clearLabelWhenFieldEmpty(field: RepeatingFieldResourceModel): void {
        if (!this.editForm.get(field.name).value) {
            this.clearLabel(field, field.sequenceNo);
        }
    }

    public clearLabel(field: RepeatingFieldResourceModel, index: number): void {
        this.editForm.get(field.parentFieldName + this.editFormConstants.label + index).markAsUntouched();
    }

    public saveFormClicked(): void {
        if (!this.editForm) {
            return;
        }

        if (!this.callbackCustomValidation) {
            this.checkFormFieldsValidity();
            if (this.editForm.pending) {
                let sub: Subscription = this.editForm.statusChanges.subscribe((res: string) => {
                    if (res != "PENDING") {
                        sub.unsubscribe();
                        if (this.editForm.valid) {
                            this.saveForm.emit(this.editForm.value);
                        }
                    }
                });
            } else {
                if (this.editForm.valid) {
                    this.saveForm.emit(this.editForm.value);
                }
            }
        } else {
            if (this.callbackCustomValidation(this.editForm.value)) {
                this.saveForm.emit(this.editForm.value);
            }
        }
    }

    public closeFormClicked(): void {
        if (this.editForm) {
            this.closeForm.emit(this.editForm.value);
        }
    }

    public ngOnInit(): void {
        this.initItemGroups();
    }

    public onBlur(controlName: string, parentControlName?: string, item?: DetailsListFormItem): void {
        this.updateControlValueAndValidity(controlName, parentControlName);
    }

    public showValidationError(group: Group, controlName: string, item: DetailsListFormItem): boolean {
        this.checkGroupItems(group);
        if (controlName) {
            let shouldShow: boolean = this.editForm.get(controlName)
                && this.editForm.get(controlName).errors != null
                && !this.editForm.get(controlName).hasError(this.editFormConstants.required)
                && (this.editForm.get(controlName).touched || this.editForm.get(controlName).dirty);
            if (shouldShow) {
                item.FormControl.setErrors(this.editForm.get(controlName).errors);
            }
            return shouldShow;
        }

        return false;
    }

    public getProperMessageForRequiredField(item: DetailsListFormItem, key: string): string {
        const requiredMessage: string = `${item.Description} is required`;
        if (item.customErrorMessageOnRequiredField !== "") {
            return item.customErrorMessageOnRequiredField;
        }

        if (item.IsRepeating) {
            let isParent: boolean = this.repeatingFields.some((f: RepeatingFieldResourceModel) =>
                f.name == key);
            return isParent ? requiredMessage : `Label is required`;
        }
        return requiredMessage;
    }

    public toggleActionIcon(index: number, item: DetailsListFormItem): void {
        if (index > -1) {
            let groupItemIndex: number = 0;
            this.itemGroups[index].expanded = !this.itemGroups[index].expanded;
            this.itemGroups[index].items.forEach((item: DetailsListFormItem) => {
                item.Visible = this.canViewItem(this.itemGroups[index], item, groupItemIndex++);
            });
        }
        item.ActionIcon.iconIndex = item.ActionIcon.iconIndex === 0 ? 1 : 0;
    }

    public removeRepeatingField(field: RepeatingFieldResourceModel, index: number): void {
        let shouldBeRemoved: boolean = this.editForm.get(field.parentFieldName + field.sequenceNo) &&
            (field.referenceId != '-1' || (index == 0));
        if (!shouldBeRemoved) {
            return;
        }
        let remainingFields: Array<RepeatingFieldResourceModel> =
            this.repeatingFields.filter((f: RepeatingFieldResourceModel) =>
                f.parentFieldName === field.parentFieldName);

        // last record or no record at all
        if (remainingFields.length == 0
            || remainingFields.slice(-1)[0].sequenceNo == index) {
            return;
        }

        this.repeatingFields = this.repeatingFields.filter((f: RepeatingFieldResourceModel) =>
            f.name != field.name);
        this.editForm.removeControl(field.parentFieldName + index);
        this.editForm.removeControl(field.parentFieldName + this.editFormConstants.label + index);
        this.editForm.removeControl(field.parentFieldName + this.editFormConstants.customLabel + index);

        remainingFields = this.repeatingFields.filter((f: RepeatingFieldResourceModel) =>
            f.parentFieldName === field.parentFieldName);

        if (remainingFields.length === 1 && remainingFields[0].referenceId === '-1') {
            this.repeatingFields = this.repeatingFields.filter((f: RepeatingFieldResourceModel) =>
                f.name != remainingFields[0].name);
            this.addRepeatingField('', field.parentFieldName, field.parentFieldName + '0', '', '', 0, false);
            let item: DetailsListFormItem = this.items.filter((f: DetailsListFormItem) =>
                f.GroupName === field.parentFieldName)[0];
            this.editForm.removeControl(
                field.parentFieldName + remainingFields[0].sequenceNo,
            );
            this.editForm.removeControl(
                field.parentFieldName + this.editFormConstants.label + remainingFields[0].sequenceNo,
            );
            this.editForm.removeControl(
                field.parentFieldName + this.editFormConstants.customLabel + remainingFields[0].sequenceNo,
            );
            this.addRepeatingFieldControl(item, 0, true);
        }

        this.updateItemsRepeatingFields();
    }

    public matSelectionChange(parentName: string, selected: any, index: number): void {
        let labelName: string = parentName + this.editFormConstants.label + index;
        let customLabelName: string = parentName + this.editFormConstants.customLabel + index;
        if (selected.value === this.editFormConstants.other) {
            this.setFieldValidator(customLabelName, FormValidatorHelper.customLabel(true));
            this.updateControlValueAndValidity(customLabelName);
            this.editForm.get(customLabelName).setErrors(null);
        } else {
            if (this.editForm.get(customLabelName)) {
                this.setFieldValidator(customLabelName, []);
                this.editForm.get(customLabelName).setErrors(null);
                this.updateControlValueAndValidity(customLabelName);
            }
        }

        if (this.editForm.get(labelName)) {
            this.editForm.get(labelName).setErrors(null);
        }
    }

    public checkAndFixRepeatingFields(
        item: DetailsListFormItem,
        field: RepeatingFieldResourceModel,
        value: any,
        index: number,
        event?: any,
    ): void {
        // retrieves immediate value.
        if (field.parentFieldName != this.editFormConstants.address) {
            value = document.getElementById(field.name)
                ? document.getElementById(field.name)["value"]
                : null;
        }

        let fields: Array<RepeatingFieldResourceModel> =
            this.getRepeatingFieldByParentName(field.parentFieldName);
        if ((field.referenceId === "-1" || fields.length === 1) && value) {
            field.referenceId = "";
            field.value = value;
            this.setFieldValidator(
                item.Alias + index,
                item.Validator);
            this.setFieldValidator(
                item.Alias + this.editFormConstants.label + index,
                FormValidatorHelper.required());
            this.setFieldValidator(
                item.Alias + this.editFormConstants.customLabel + index,
                FormValidatorHelper.customLabel(true));
            let newIndex: number = fields[fields.length - 1].sequenceNo + 1;
            this.addRepeatingField("-1", item.Alias, item.Alias + newIndex, "", "", newIndex, false);
            this.addRepeatingFieldControl(item, newIndex, true);
        }
    }

    private hasFieldOptions(fieldName: string): boolean {
        return this.fieldsOptions.findIndex((f: EntityEditFieldOption) => f.name === fieldName) >= 0;
    }

    private getItemsByGroup(group: string): Array<DetailsListFormItem> {
        let items: Array<DetailsListFormItem> = this.items.filter((i: DetailsListFormItem) =>
            i.GroupName === group && i.Alias);
        return items;
    }

    private canViewItem(group: Group, item: DetailsListFormItem, index: number): boolean {
        let control: AbstractControl = this.editForm.get(item.Alias);
        let canView: boolean = group.name == item.GroupName
            && (item.IsInitVisible
                || (control
                    && control.value
                    && control.value.length > 0
                    && !item.IsInitVisible)
                || (this.isGroupExpanded(group.name)
                    && !item.IsInitVisible));
        if (index === 0) {
            this.isFirstItemVisible = canView;
        }

        return canView;
    }

    public isGroupExpanded(groupName: any): boolean {
        const groupActionIcon: Group = this.itemGroups.find((a: Group) => a.name === groupName);
        if (groupActionIcon) {
            return groupActionIcon.expanded;
        }
        return false;
    }

    public isGroupItemsComplete(group: Group): boolean {
        let isComplete: boolean = true;
        let items: Array<DetailsListFormItem> = this.getItemsByGroup(group.name);
        const formValue: any = this.editForm.value;
        if (items) {
            items.forEach((item: DetailsListFormItem) => {
                if (!formValue[item.Alias]) {
                    isComplete = false;
                }

            });
        }
        return isComplete;
    }

    public checkGroupItems(group: Group): void {
        const index: number = this.itemGroups.findIndex((a: Group) => a.name === group.name);
        if (index > -1) {
            const isCompleted: boolean = this.isGroupItemsComplete(group);
            if (this.itemGroups[index].isCompleted !== isCompleted) {
                this.itemGroups[index].isCompleted = isCompleted;
            }

            let items: Array<DetailsListFormItem> = this.itemGroups[index].items;
            let i: number = 0;
            items.forEach((item: DetailsListFormItem) => {
                let canViewItem: boolean = this.canViewItem(group, item, i);
                item.Visible = canViewItem && item.Visible;
                item[this.editFormConstants.canShow] = canViewItem;

                let prevItem: DetailsListFormItem = i > 0 ? items[i - 1] : null;
                item.canShowIcon = item.Icon &&
                    (i == 0 || prevItem && item.Icon === prevItem.Icon && !prevItem.Visible);

                const fieldOptions: any = this.getFieldOptionsByFieldName(item.Alias);
                if (fieldOptions) {
                    item[this.editFormConstants.options] = fieldOptions;
                }
                if (!item[this.editFormConstants.options]) {
                    item[this.editFormConstants.options] = [];
                }

                if (item.IsRepeating) {
                    let repeatingFields: Array<RepeatingFieldResourceModel> =
                        this.getRepeatingFieldByParentName(item.Alias);
                    item[this.editFormConstants.repeatingFields] = repeatingFields;
                    repeatingFields.forEach((field: RepeatingFieldResourceModel) => field.groupName = item.GroupName);
                }

                i++;
            });

        }
    }

    public repeatingFieldHasValue(field: RepeatingFieldResourceModel): boolean {
        let fieldName: string = field.name;
        // if field is address.
        if (field.parentFieldName == this.editFormConstants.address) {
            return this.addressHasValue(field);
        } else {
            return this.editForm.get(fieldName).value ? true : false;
        }
    }

    private addressHasValue(field: RepeatingFieldResourceModel): boolean {
        let fieldName: string = field.name;
        let _addressHasValue: boolean = false;
        let formControls: Array<AbstractControl> = this.editForm.get(fieldName)['controls'];

        Object
            .keys(formControls)
            .forEach((key: string) => {
                let val: string = this.editForm.get(fieldName).get(key).value;
                if (val) {
                    _addressHasValue = true;
                }
            });

        return _addressHasValue;
    }

    private checkAccordionToggle(): void {
        this.itemGroups.forEach((g: Group) => {
            if (g.items.length >= 0 && g.items[0].ActionIcon) {
                this.hasAccordionToggle = true;
            }
        });
    }

    private updateItemsRepeatingFields(): void {
        this.itemGroups.forEach((g: Group) => {
            let items: Array<DetailsListFormItem> = g.items;
            items.forEach((item: DetailsListFormItem) => {
                if (item.IsRepeating && item.FormControlType != 'group') {
                    let repeatingFields: Array<RepeatingFieldResourceModel> =
                        this.getRepeatingFieldByParentName(item.Alias);
                    item[this.editFormConstants.repeatingFields] = repeatingFields;
                    repeatingFields.forEach((field: RepeatingFieldResourceModel) => field.groupName = item.GroupName);
                }
            });
        });
    }

    private getFieldOptionsByFieldName(fieldName: string): Array<{ label: string; value: string }> {
        return this.hasFieldOptions(fieldName)
            ? this.fieldsOptions.filter((f: EntityEditFieldOption) => f.name === fieldName)[0].options
            : null;
    }

    public checkBoxEvent(fieldName: string, value: any, index: number): void {
        this.valueChanged(fieldName, value.label, (x: FieldOption) => {
            // set the value to the opposite of the current value
            const isTrueValue: boolean = value.value.toLowerCase() === 'true';
            x.value = (!isTrueValue).toString();
            // check if the field un/checked triggers another field to show/hide
            this.checkToggleCheckbox(x);
        });
    }

    public matOptionClickEvent(fieldName: string, value: any, index: number): void {
        this.valueChanged(fieldName, value.label, (x: FieldOption) => {
            // check if the chosen option triggers another field to show/hide
            this.checkFieldShowHideRules(fieldName, x);
        });
    }

    private valueChanged(fieldName: string, label: string, callback: (x: FieldOption) => void): void {
        // dropdown: gets the options of the field that are available for selection
        // checkbox: gets the label of the field
        const options: Array<FieldOption> = this.getFieldOptions(fieldName);
        if (options) {
            options.forEach((x: FieldOption) => {
                // if the option is equal to the current selection of the field(label), execute the callback
                if (x.label === label) {
                    callback(x);
                }
            });
        }
    }

    private getFieldOptions(fieldName: string): Array<FieldOption> {
        const fieldOption: EntityEditFieldOption | undefined = this.fieldsOptions.find(
            (f: EntityEditFieldOption) => f.name === fieldName,
        );
        return fieldOption?.options || [];
    }

    private checkFieldShowHideRules(fieldName: string, fieldOption: FieldOption): void {
        // gets a list of all the fields triggered by the fieldName
        const fieldsTriggered: Array<FieldShowHideRule> = this.getFieldShowHideRules(fieldName);
        fieldsTriggered?.forEach((fieldShowHideRule: FieldShowHideRule) => {
            this.toggleVisibility(fieldShowHideRule.fieldToHideOrShow);
        });
    }

    private checkToggleCheckbox(fieldOption: FieldOption): void {
        // gets a list of all the fields triggered by the fieldOption
        const fieldsTriggered: Array<FieldShowHideRule> = this.getFieldShowHideRules(fieldOption.label);
        fieldsTriggered?.forEach((fieldShowHideRule: FieldShowHideRule) => {
            this.toggleVisibility(fieldShowHideRule.fieldToHideOrShow);
        });
    }

    private getFieldShowHideRules(triggerField: string): Array<FieldShowHideRule> {
        return this.fieldShowHideRules.filter((to: FieldShowHideRule) => to.triggerField === triggerField);
    }

    private toggleVisibility(fieldToHideOrShow: string): void {
        const allTriggersOfTheFieldToHideOrShow: Array<FieldShowHideRule>
            = [...this.getAllTriggers(fieldToHideOrShow)];
        let allTrue: boolean = true;
        for (const toggle of allTriggersOfTheFieldToHideOrShow) {
            const fieldValue: string = this.getFieldValue(toggle.triggerField);
            const toggleVisibility: boolean = toggle.showWhenValueIs === fieldValue;
            if (!toggleVisibility) {
                allTrue = false;
            }
        }
        this.setVisibility(fieldToHideOrShow, allTrue);
    }

    private getAllTriggers(fieldToHideOrShow: string): Array<FieldShowHideRule> {
        return this.fieldShowHideRules.filter((to: FieldShowHideRule) => to.fieldToHideOrShow === fieldToHideOrShow);
    }

    private getFieldValue(fieldName: string): string {
        const formControl: AbstractControl = this.editForm.get(fieldName);
        return formControl ? formControl.value : this.getFieldOptionValue(fieldName);
    }

    private getFieldOptionValue(fieldName: string): string {
        const option: FieldOption | undefined = this.getFieldOption(fieldName);
        return option ? option.value : '';
    }

    private getFieldOption(fieldName: string): FieldOption | undefined {
        for (const group of this.fieldsOptions) {
            const option: FieldOption | undefined = group.options.find((opt: FieldOption) => opt.label === fieldName);
            if (option) {
                return option;
            }
        }
    }

    private setVisibility(fieldToHideOrShow: string, isVisible: boolean): void {
        let detailListItem: Array<DetailsListFormItem> = this.items.filter(
            (dli: DetailsListFormItem) => dli.Alias === fieldToHideOrShow,
        );

        // verify if the fieldToHideOrShow exists, then set its visibility
        if (detailListItem?.length > 0) {
            let itemGroup: Array<Group> = this.itemGroups.filter(
                (ig: Group) => ig.name === detailListItem[0].GroupName,
            );

            if (itemGroup?.length > 0) {
                let item: DetailsListFormItem = itemGroup[0].items.find(
                    (item: DetailsListFormItem) => item.Alias === fieldToHideOrShow,
                );

                if (item) {
                    item.Visible = isVisible;
                }
            }
        }
    }

    private initItemGroups(): void {
        let index: number = 0;
        let isEdit: boolean = this.isEdit;
        let _this: DetailListItemsEditFormComponent = this;
        this.repeatingFields = this.initRepeatingFields();
        if (this.items) {
            this.items.forEach((item: DetailsListFormItem) => {
                item["index"] = index;
                let i: number = _this.itemGroups.findIndex(
                    (g: Group) => g.name == item.GroupName,
                );
                if (i <= -1) {
                    let newGroup: Group = {
                        name: item.GroupName,
                        icon: item.Icon,
                        items: _this.getItemsByGroup(item.GroupName),
                        expanded: false,
                        isCompleted: false,
                        header: item.Header,
                        subHeader: item.SubHeader,
                        includeSectionIcons: item.IncludeSectionIcons,
                    };

                    _this.itemGroups.push(newGroup);
                    _this.checkGroupItems(newGroup);
                }
                let repeatingPerItem: Array<RepeatingFieldResourceModel> =
                    _this.getRepeatingFieldByParentName(item.Alias);
                if (item.IsRepeating && item.FormControlType != 'group' && (!isEdit || repeatingPerItem.length === 0)) {
                    _this.addRepeatingField("0", item.Alias, item.Alias + "0", "", "", 0, false);
                }

                index++;
            });

            this.updateItemsRepeatingFields();
            this.checkAccordionToggle();
            this.createRepeatingFieldFormControls();
        }
    }

    private initRepeatingFields(): Array<RepeatingFieldResourceModel> {
        this.repeatingFields = this.model ? this.model.repeatingFields ? this.model.repeatingFields : [] : [];
        let repeatingAddressFields: Array<RepeatingAddressFieldResourceModel> = this.model ?
            this.model.repeatingAddressFields ? this.model.repeatingAddressFields : [] : [];
        if (repeatingAddressFields.length > 0) {
            repeatingAddressFields.forEach((field: RepeatingAddressFieldResourceModel) => {
                let newRepeatingItem: RepeatingFieldResourceModel = {
                    parentFieldName: field.parentFieldName,
                    sequenceNo: field.sequenceNo,
                    referenceId: field.referenceId,
                    label: field.label,
                    customLabel: field.customLabel,
                    name: field.name,
                    value: field.value,
                    default: field.default,
                };
                newRepeatingItem[this.editFormConstants.groupValue] = {
                    address: field.address,
                    suburb: field.suburb,
                    state: field.state,
                    postcode: field.postcode,
                };
                this.repeatingFields.push(newRepeatingItem);
            });

        }
        return this.repeatingFields;
    }

    private getRepeatingFieldByParentName(fieldName: string): Array<RepeatingFieldResourceModel> {
        return this.repeatingFields ? this.repeatingFields.filter((f: RepeatingFieldResourceModel) =>
            f.parentFieldName === fieldName) : [];
    }

    private createRepeatingFieldFormControls(): void {
        let editForm: FormGroup = this.editForm;
        let _this: any = this;
        if (this.items) {
            this.items.forEach((item: DetailsListFormItem) => {
                if (item.IsRepeating && item.FormControlType != 'group') {
                    let repeatingFields: Array<RepeatingFieldResourceModel> =
                        _this.getRepeatingFieldByParentName(item.Alias);
                    let i: number = 0;
                    repeatingFields.forEach((field: RepeatingFieldResourceModel) => {
                        field.sequenceNo = i;
                        field.name = item.Alias + i;
                        _this.addRepeatingFieldControl(item, i);
                        editForm.controls[field.name].patchValue(
                            field.parentFieldName === _this.editFormConstants.address
                                ? field[_this.editFormConstants.groupValue]
                                : field.value);
                        if (field.label) {
                            editForm.controls[item.Alias + _this.editFormConstants.label + i]
                                .patchValue(field.label);
                        }
                        if (field.label === _this.editFormConstants.other) {
                            let customLabelControlId: string =
                                item.Alias + _this.editFormConstants.customLabel + i;
                            editForm.controls[customLabelControlId]
                                .patchValue(field.customLabel);

                            // make customLabel required of label has value 'Other'
                            if (editForm.get(customLabelControlId)) {
                                editForm.get(customLabelControlId)
                                    .setValidators(FormValidatorHelper.customLabel(true));
                            }
                        } else {
                            _this.setFieldValidator(item.Alias + _this.editFormConstants.customLabel + i, []);
                        }

                        i++;
                    });

                    let addressConstant: string = _this.editFormConstants.address;
                    let canSetLabelInitialValidator: boolean = repeatingFields.length === 1 &&
                        ((item.GroupName != addressConstant && !editForm.controls[item.Alias + "0"].value) ||
                            (item.GroupName === addressConstant &&
                                !editForm.controls[item.Alias + "0"].value[addressConstant]) &&
                            !editForm.controls[item.Alias + _this.editFormConstants.label + "0"].value);

                    if (canSetLabelInitialValidator) {
                        _this.setFieldValidator(item.Alias + _this.editFormConstants.label + "0", []);
                        _this.setFieldValidator(item.Alias + _this.editFormConstants.customLabel + "0", []);
                    }

                    let canAddExtraField: any = i > 0 &&
                        ((editForm.controls[item.Alias + (i - 1)].value &&
                            item.GroupName != addressConstant) ||
                            (item.GroupName === addressConstant &&
                                editForm.controls[item.Alias + (i - 1)].value[addressConstant]));
                    if (canAddExtraField) {
                        _this.addRepeatingField("-1", item.Alias, item.Alias + i, "", "", i, false);
                        _this.addRepeatingFieldControl(item, i, true);
                    }
                }
            });

        }
    }

    private checkFormFieldsValidity(): void {
        Object.keys(this.editForm.controls).forEach((key: string) => {
            let formControl: AbstractControl = this.editForm.get(key);
            let noIndexName: string = key.replace(/[0-9]/g, '');
            this.updateControlValueAndValidity(key);
            if (formControl.invalid && this.items) {
                formControl.markAsTouched();
                let field: Array<RepeatingFieldResourceModel> = this.repeatingFields.filter(
                    (f: RepeatingFieldResourceModel) => f.parentFieldName === noIndexName,
                );
                let repeatingItem: DetailsListFormItem = this.items.find((f: DetailsListFormItem) =>
                    f.IsRepeating === true && f.FormControlType != 'group' && f.Alias === noIndexName);
                if (field.length > 0 && repeatingItem) {
                    let index: string = this.getKeyIndex(key);
                    let labelControlName: string = noIndexName + this.editFormConstants.label + index;
                    let customLabelControlName: string = noIndexName + this.editFormConstants.customLabel + index;
                    if (this.editForm.get(labelControlName).value === this.editFormConstants.other) {
                        this.setFieldValidator(customLabelControlName, FormValidatorHelper.required(), true);
                    } else {
                        this.setFieldValidator(customLabelControlName, [], true);
                    }

                    this.setFieldValidator(labelControlName, FormValidatorHelper.required(), true);
                    if (noIndexName === this.editFormConstants.address) {
                        this.markControlAsDiryAndTouched(formControl.get(AddressPart.StreetAddress));
                        this.markControlAsDiryAndTouched(formControl.get(AddressPart.Suburb));
                        this.markControlAsDiryAndTouched(formControl.get(AddressPart.State));
                        this.markControlAsDiryAndTouched(formControl.get(AddressPart.Postcode));
                    }
                } else {

                    // clear label and custom label validation if value of field is empty.
                    if (repeatingItem
                        || key.indexOf(this.editFormConstants.label) > -1
                        || key.indexOf(this.editFormConstants.customLabel) > -1) {
                        let arrLabel: Array<string> = noIndexName.split('_');
                        if (arrLabel.length > 0) {
                            let controlIndex: string = this.getKeyIndex(key);
                            let control: AbstractControl = this.editForm.get(arrLabel[0] + controlIndex);
                            this.markControlAsDiryAndTouched(formControl);
                            let value: any = arrLabel[0] === this.editFormConstants.address ?
                                control.get(AddressPart.StreetAddress).value : control.value;

                            if (control.valid && !value) {
                                this.setFieldValidator(key, []);
                            }
                        }
                    }
                }
            }
        });
    }

    private markControlAsDiryAndTouched(formControl: AbstractControl): void {
        formControl.markAsTouched();
        formControl.markAsDirty();
    }

    private updateControlValueAndValidity(controlName: string, parentControlName?: string): void {
        // getting value directly from the dom
        // because, intermittently, the formControl value is not updating
        let domElement: any = parentControlName ?
            document.getElementById(parentControlName + controlName) : document.getElementById(controlName);
        let formControl: AbstractControl = this.editForm.get(controlName);
        if (formControl == null) {
            // this is a poor design issue with the way this component works, and will need to be fixed one day.
            console.log(`could not get form control with name "${controlName}".`);
            return null;
        }
        if (domElement) {
            if (parentControlName) {
                this.editForm.get(parentControlName).get(controlName).patchValue(domElement.value);
                this.editForm.get(parentControlName).get(controlName).updateValueAndValidity();
            } else {
                let controlIndex: string = this.getKeyIndex(controlName);
                if (domElement.nodeName.toLowerCase() == 'ion-list') {
                    let values: Array<boolean> = new Array<boolean>();
                    for (let element of domElement.children) {
                        if (element.nodeName.toLowerCase() == 'ion-checkbox') {
                            values.push(element.checked);
                        }
                    }
                    formControl.patchValue(values);
                } else if (domElement.nodeName.toLowerCase() == 'mat-radio-group') {
                    // do nothing - no patching needed.
                } else {
                    formControl.patchValue(domElement.value ? domElement.value : this.editForm.get(controlName).value);
                }
                let noIndexName: string = controlName.replace(/[0-9]/g, '');
                if (domElement.value) {
                    formControl.patchValue(domElement.value);
                    let item: DetailsListFormItem = this.items.find((f: DetailsListFormItem) =>
                        f.Alias + controlIndex == controlName);
                    if (item?.Validator?.length > 0) {
                        this.setFieldValidator(controlName, item.Validator);
                    }
                } else {
                    let shouldRemoveValidator: boolean = false;
                    if (controlIndex != "0" && controlIndex && controlName.toLowerCase().indexOf("label") < 0
                        && controlName.toLowerCase()
                            .indexOf(AdditionalPropertiesHelper.additionalPropertyPrefixId) < 0) {
                        shouldRemoveValidator = true;
                    } else if (controlName.toLowerCase().indexOf("label") > -1) {
                        let arrLabel: Array<string> = noIndexName.split('_');
                        let controlGrouplName: AbstractControl = this.editForm.get(arrLabel[0] + controlIndex);
                        if (!controlGrouplName.hasError('required') && !controlGrouplName.value) {
                            shouldRemoveValidator = true;
                        }
                    }
                    if (shouldRemoveValidator) {
                        let labelControlName: string = noIndexName +
                            this.editFormConstants.label + controlIndex;
                        let customLabelControlName: string = noIndexName +
                            this.editFormConstants.customLabel + controlIndex;
                        this.setFieldValidator(controlName, []);
                        this.setFieldValidator(labelControlName, []);
                        this.setFieldValidator(customLabelControlName, []);
                    }
                }
                formControl.updateValueAndValidity();
            }
        } else {
            let noIndexName: string = controlName.replace(/[0-9]/g, '');
            if (noIndexName === this.editFormConstants.address) {
                let streetAddress: any = this.editForm.get(controlName).get(AddressPart.StreetAddress).value;
                let suburb: any = this.editForm.get(controlName).get(AddressPart.Suburb).value;
                let state: any = this.editForm.get(controlName).get(AddressPart.State).value;
                let postcode: any = this.editForm.get(controlName).get(AddressPart.Postcode).value;
                formControl.patchValue({
                    address: streetAddress,
                    suburb: suburb,
                    state: state,
                    postcode: postcode,
                });
            }
            formControl.updateValueAndValidity();
        }
    }

    private setFieldValidator(
        controlName: string,
        validator: Array<ValidatorFn>,
        markAsUntouched: boolean = false,
    ): void {
        if (this.editForm.get(controlName)) {
            this.editForm.get(controlName).setValidators(validator);
            this.editForm.get(controlName).setErrors(null);
            if (markAsUntouched) {
                this.editForm.get(controlName).markAsUntouched();
            }
        }
    }

    private addRepeatingFieldControl(
        item: DetailsListFormItem,
        index: number,
        extra?: boolean,
    ): void {
        let field: RepeatingFieldResourceModel = this.repeatingFields.find((f: RepeatingFieldResourceModel) =>
            f.name === (item.Alias + index));
        let controlName: string = item.Alias + index;
        if (item.Alias === this.editFormConstants.address) {
            this.editForm.addControl(controlName, new FormGroup({
                address: this.createFormControl([], true),
                suburb: this.createFormControl([], true),
                state: this.createFormControl([]),
                postcode: this.createFormControl([], true),
            }));
        } else {
            let validator: Array<ValidatorFn> = field &&
                field.referenceId === "-1" ? [] : item.Validator;
            this.editForm.addControl(controlName, this.createFormControl(validator, true));
        }

        let labelControl: FormControl = this.createFormControl(extra ? [] :
            FormValidatorHelper.required());
        let customLabelControl: FormControl = this.createFormControl(extra ? [] :
            FormValidatorHelper.customLabel(true), true);

        let hiddenFieldControl: FormControl = this.createFormControl([]);
        this.editForm.addControl(item.Alias + this.editFormConstants.label + index, labelControl);
        this.editForm.addControl(item.Alias + this.editFormConstants.customLabel + index, customLabelControl);
        this.editForm.addControl(item.Alias + "_default" + index, hiddenFieldControl);
    }

    private createFormControl(validator: Array<ValidatorFn>, updateOnBlur: boolean = false): FormControl {
        let newControl: FormControl = new FormControl(
            '',
            updateOnBlur ? { validators: validator, updateOn: "blur" } : validator,
        );
        newControl.setErrors(null);
        return newControl;
    }

    private addRepeatingField(
        referenceId: string,
        parentFieldName: string,
        name: string,
        label: string,
        customLabel: string,
        index: number,
        isDefault: boolean,
    ): void {
        let repeatingField: RepeatingFieldResourceModel = {
            referenceId: referenceId,
            name: name,
            parentFieldName: parentFieldName,
            label: label,
            customLabel: customLabel,
            sequenceNo: index,
            value: "",
            default: isDefault,
        };

        if (parentFieldName === this.editFormConstants.address) {
            repeatingField[this.editFormConstants.groupValue] = {
                address: "",
                suburb: "",
                state: "",
                postcode: "",
            };
        }

        this.repeatingFields.push(repeatingField);
        this.updateItemsRepeatingFields();
    }

    private getKeyIndex(key: string): string {
        let index: any = key.match(/\d+/g);
        return index ? index[0] : '';
    }
}
