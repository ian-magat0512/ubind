import { AbstractControl, FormControl, ValidatorFn } from "@angular/forms";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListItemCard } from "./details-list-item-card";
import { DetailsListFormItem } from "./details-list-form-item";

/**
 * A model for a checkbox form input control.
 */
export class DetailsListFormCheckboxItem extends DetailsListFormItem {

    public initiallyChecked: boolean = false;

    public constructor(
        card: DetailsListItemCard,
        groupName: string,
        alias: string,
        displayValue: string,
        description: string,
        initiallyChecked: boolean,
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
            'checkbox',
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
        this.initiallyChecked = initiallyChecked;
    }

    public static create(
        card: DetailsListItemCard,
        alias: string,
        description: string,
        initiallyChecked: boolean = false,
    ): DetailsListFormCheckboxItem {
        return new DetailsListFormCheckboxItem(
            card,
            'default',
            alias,
            null,
            description,
            initiallyChecked);
    }

    public get FormControl(): AbstractControl {
        if (this.formControl) {
            return this.formControl;
        }

        this.formControl = new FormControl(this.initiallyChecked);
        return this.formControl;
    }

    public clone(): DetailsListFormItem {
        return new DetailsListFormCheckboxItem(
            this.card,
            this.groupName,
            this.alias,
            this.displayValue,
            this.description,
            this.initiallyChecked,
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
    }
}
