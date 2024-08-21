import { AbstractControl, FormArray, ValidatorFn } from "@angular/forms";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListItemCard } from "./details-list-item-card";
import { DetailsListFormOptionsItem } from "./details-list-form-options-item";
import { DetailsListFormItem } from "./details-list-form-item";

/**
 * A model for a checkbox form input control.
 */
export class DetailsListFormCheckboxGroupItem extends DetailsListFormOptionsItem {

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
        visible: boolean = true,
        isRepeating: boolean = false,
        subheader: string = "",
        includeSectionIcons: boolean = true,
    ) {
        super(
            card,
            'checkbox-group',
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
            visible,
            isRepeating,
            subheader,
            includeSectionIcons);
    }

    public static create(
        card: DetailsListItemCard,
        alias: string,
        description: string,
    ): DetailsListFormCheckboxGroupItem {
        return new DetailsListFormCheckboxGroupItem(
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
        return this.formControl;
    }

    public clone(): DetailsListFormItem {
        return new DetailsListFormCheckboxGroupItem(
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
            this.includeSectionIcons,
        );
    }

}
