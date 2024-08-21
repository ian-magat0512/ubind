import { AbstractControl, FormGroup, ValidatorFn } from "@angular/forms";
import { DetailsListFormItem } from "./details-list-form-item";
import { DetailsListItemCard } from "./details-list-item-card";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { IconLibrary } from "../icon-library.enum";

/**
 * Represents a group of fields in a form (or a field set).
 * This is used to group related fields together, and also to allow the group to repeat.
 */
export class DetailsListFormItemGroup extends DetailsListFormItem {

    private items: Array<DetailsListFormItem>;

    /**
     * Whether or not the fields in this group should be displayed side-by-side in a line,
     * or stacked on top of each other.
     */
    public sideBySide: boolean = true;

    public constructor(
        card: DetailsListItemCard,
        groupName: string,
        alias: string,
        displayValue: string,
        description: string,
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
            'group',
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
    }

    public static create(
        card: DetailsListItemCard,
        alias: string,
        description: string,
    ): DetailsListFormItemGroup {
        return new DetailsListFormItemGroup(
            card,
            'default',
            alias,
            null,
            description);
    }

    public withItem(item: DetailsListFormItem): DetailsListFormItemGroup {
        if (!this.items) {
            this.items = new Array<DetailsListFormItem>();
        }
        this.items.push(item);
        return this;
    }

    public get FormControl(): AbstractControl {
        if (this.formControl) {
            return this.formControl;
        }
        let formGroup: FormGroup = new FormGroup(
            {},
            {
                validators: this.visible ? this.validator : null,
                updateOn: this.updateOn,
            });
        for (let item of this.items) {
            formGroup.addControl(item.Alias, item.FormControl);
        }

        this.formControl = formGroup;
        return this.formControl;
    }

    public clone(): DetailsListFormItem {
        let newItem: DetailsListFormItemGroup = new DetailsListFormItemGroup(
            this.card,
            this.groupName,
            this.alias,
            this.displayValue,
            this.description,
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
            newItem.withItem(childItem.clone());
        }
        return newItem;
    }

    public get Items(): Array<DetailsListFormItem> {
        return this.items;
    }

    public isValueCompletelyEmpty(): boolean {
        let isCompletelyEmpty: boolean = true;
        for (let item of this.items) {
            if (!item.isValueCompletelyEmpty()) {
                isCompletelyEmpty = false;
                break;
            }
        }
        return isCompletelyEmpty;
    }

    public isValueOnlyPartiallyFilled(): boolean {
        let isPartiallyFilled: boolean = false;
        for (let item of this.items) {
            if (item.isValueOnlyPartiallyFilled()) {
                isPartiallyFilled = true;
                break;
            }
        }
        return isPartiallyFilled;
    }
}
