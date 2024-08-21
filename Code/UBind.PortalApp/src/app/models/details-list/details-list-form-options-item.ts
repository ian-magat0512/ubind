import { ValidatorFn } from "@angular/forms";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListFormItem } from "./details-list-form-item";
import { DetailsListFormItemSelectOption } from "./details-list-form-item-select-option";
import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { DetailsListItemCard } from "./details-list-item-card";

/**
 * A base class for form input controls that have options, e.g. a select drop down, radio button, etc.
 */
export abstract class DetailsListFormOptionsItem extends DetailsListFormItem {
    public options: Array<DetailsListFormItemSelectOption>;

    public constructor(
        card: DetailsListItemCard,
        formControlType: string,
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
            formControlType,
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

    public withOption(option: DetailsListFormItemSelectOption): DetailsListFormOptionsItem {
        this.options = this.options || new Array<DetailsListFormItemSelectOption>();
        this.options.push(option);
        return this;
    }
}
