import { ValidatorFn } from "@angular/forms";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListFormItem } from "./details-list-form-item";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { DetailsListItemCard } from "./details-list-item-card";

/**
 * A model for an address form input control.
 */
export class DetailsListFormAddressItem extends DetailsListFormItem {

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
            'address',
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
    ): DetailsListFormAddressItem {
        return new DetailsListFormAddressItem(
            card,
            'default',
            alias,
            null,
            description);
    }

    public clone(): DetailsListFormItem {
        return new DetailsListFormAddressItem(
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
