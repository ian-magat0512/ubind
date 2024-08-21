import { ValidatorFn } from "@angular/forms";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListFormItem } from "./details-list-form-item";
import { DetailsListItemCard } from "./details-list-item-card";

/**
 * A model for a date form input control.
 */
export class DetailsListFormDateItem extends DetailsListFormItem {

    private minValue: string;
    private maxValue: string;

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
            'datepicker',
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
    ): DetailsListFormDateItem {
        return new DetailsListFormDateItem(
            card,
            'default',
            alias,
            null,
            description);
    }

    /**
     * To be used to set a min/max date limit for the date picker.
     * @param minValue can be set to null to use the default ion-datepicker minValue.
     * @param maxValue can be set to null to use the default ion-datepicker maxValue.
     */
    public withDateRange(
        minValue: string,
        maxValue: string,
    ): DetailsListFormItem {
        this.minValue = minValue;
        this.maxValue = maxValue;
        return this;
    }

    public clone(): DetailsListFormItem {
        return new DetailsListFormDateItem(
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
    }
}
