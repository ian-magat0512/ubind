import { AbstractControl, FormArray, ValidatorFn } from "@angular/forms";
import { DetailsListFormItem } from "./details-list-form-item";
import { DetailsListFormItemArray } from "./details-list-form-item-array";
import { DetailsListItemCard } from "./details-list-item-card";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";

/**
 * Represents a repeating array of fields or field groups in a form.
 * This differs from DetailsListFormItemArray in that it is a repeating set of the same
 * set of fields, so when one has been entered, another set will appear automatically
 * allowing you to continue entering new instances.
 */
export class DetailsListFormItemRepeating extends DetailsListFormItemArray {
    private templateItem: DetailsListFormItem;

    /**
     * If true, the repeating instances will grow and shrink automatically to fit the data.
     */
    private autoGrowShrink: boolean = true;

    public constructor(
        card: DetailsListItemCard,
        groupName: string,
        alias: string,
        displayValue: string,
        description: string,
        templateItem: DetailsListFormItem,
        icon: string = null,
        iconLibrary: string = IconLibrary.IonicV4,
        header: string = '',
        validator: Array<ValidatorFn> = [],
        actionIcon: DetailsListItemActionIcon = null,
        isRoundIcon: boolean = false,
        isInitVisible: boolean = true,
        isRepeating: boolean = false,
        subheader: string = "",
        includeSectionIcons: boolean = true,
    ) {
        super(
            card,
            groupName,
            alias,
            displayValue,
            description,
            icon,
            iconLibrary,
            header,
            validator,
            actionIcon,
            isRoundIcon,
            isInitVisible,
            isRepeating,
            subheader,
            includeSectionIcons);
        this.templateItem = templateItem;
    }

    public static create(
        card: DetailsListItemCard,
        alias: string,
        description: string,
        templateItem: DetailsListFormItem = null,
    ): DetailsListFormItemRepeating {
        return new DetailsListFormItemRepeating(
            card,
            'default',
            alias,
            null,
            description,
            templateItem);
    }

    public get FormControl(): AbstractControl {
        if (this.formControl) {
            return this.formControl;
        }
        this.formControl = super.FormControl;
        this.addRepeatingInstance();
        this.listenForValueChangesAndAddOrRemoveRepeatingInstances();
        return this.formControl;
    }

    public addRepeatingInstance(): void {
        const newitem: DetailsListFormItem = this.templateItem.clone();
        this.items.push(newitem);
        if (this.formControl) {
            const formArray: FormArray = this.FormControl as FormArray;
            formArray.push(newitem.FormControl);
        }
    }

    public removeRepeatingInstance(index: number): void {
        this.items.splice(index, 1);
        if (this.formControl) {
            const formArray: FormArray = this.FormControl as FormArray;
            formArray.removeAt(index);
        }
    }

    public listenForValueChangesAndAddOrRemoveRepeatingInstances(): void {
        this.FormControl.valueChanges.subscribe((value: any) => {
            if (this.autoGrowShrink) {
                this.addOrRemoveRepeatingInstances();
            }
        });
    }

    public addOrRemoveRepeatingInstances(): void {
        let areAllItemsFullyFilled: boolean = true;
        let indexOfFirstCompletelyEmptyItem: number = -1;
        for (let i: number = 0; i < this.Items.length; i++) {
            let fullyUnfilled: boolean = this.Items[i].isValueCompletelyEmpty();
            let onlyPartiallyFilled: boolean = this.Items[i].isValueOnlyPartiallyFilled();
            if (fullyUnfilled) {
                indexOfFirstCompletelyEmptyItem = i;
            }
            if (onlyPartiallyFilled) {
                areAllItemsFullyFilled = false;
            }

            if (!areAllItemsFullyFilled && indexOfFirstCompletelyEmptyItem > 0) {
                break;
            }
        }

        if (areAllItemsFullyFilled) {
            this.addRepeatingInstance();
        }

        if (indexOfFirstCompletelyEmptyItem > 0 && indexOfFirstCompletelyEmptyItem < this.items.length - 1) {
            this.removeRepeatingInstance(indexOfFirstCompletelyEmptyItem);
        }
    }

    public setNumberOfInstances(numberOfInstances: number): void {
        this.autoGrowShrink = false;
        while (this.items.length < numberOfInstances) {
            this.addRepeatingInstance();
        }
        while (this.items.length > numberOfInstances) {
            this.removeRepeatingInstance(this.items.length - 1);
        }
        this.autoGrowShrink = true;
    }

    public clone(): DetailsListFormItem {
        let newItem: DetailsListFormItemRepeating =  new DetailsListFormItemRepeating(
            this.card,
            this.groupName,
            this.alias,
            this.displayValue,
            this.description,
            this.templateItem,
            this.icon,
            this.iconLibrary,
            this.header,
            this.validator,
            this.actionIcon,
            this.isRoundIcon,
            this.visible,
            this.isRepeating,
            this.subHeader,
            this.includeSectionIcons);
        for (let childItem of this.items) {
            newItem.items.push(childItem.clone());
        }
        return newItem;
    }
}
