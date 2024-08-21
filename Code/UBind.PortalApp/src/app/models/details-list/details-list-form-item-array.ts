import { AbstractControl, FormArray, ValidatorFn } from "@angular/forms";
import { DetailsListFormItem } from "./details-list-form-item";
import { DetailsListItemCard } from "./details-list-item-card";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { IconLibrary } from "../icon-library.enum";

/**
 * Represents a repeating array of fields or field gtoups in a form.
 */
export class DetailsListFormItemArray extends DetailsListFormItem {

    protected items: Array<DetailsListFormItem> = new Array<DetailsListFormItem>();

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
            'array',
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
    ): DetailsListFormItemArray {
        return new DetailsListFormItemArray(
            card,
            'default',
            alias,
            null,
            description);
    }

    public get FormControl(): AbstractControl {
        if (this.formControl) {
            return this.formControl;
        }
        this.formControl = new FormArray([]);
        for (const item of this.items) {
            const formArray: FormArray = this.formControl as FormArray;
            formArray.push(item.FormControl);
        }
        return this.formControl;
    }

    public withItem(item: DetailsListFormItem): DetailsListFormItemArray {
        this.items.push(item);
        return this;
    }

    public get Items(): Array<DetailsListFormItem> {
        return this.items;
    }

    public clone(): DetailsListFormItem {
        let newItem: DetailsListFormItemArray =  new DetailsListFormItemArray(
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
            newItem.items.push(childItem.clone());
        }
        return newItem;
    }
}
