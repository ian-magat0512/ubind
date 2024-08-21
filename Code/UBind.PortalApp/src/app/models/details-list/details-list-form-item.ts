import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { Subject, Observable } from "rxjs";
import { ValidatorFn, AbstractControl, FormControl } from "@angular/forms";
import { ValidationMessages } from "../validation-messages";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { IconLibrary } from "../icon-library.enum";
import { DetailsListItem } from "./details-list-item";
import { DetailsListItemCard } from "./details-list-item-card";

/**
 * The base class for models of form input controls.
 */
export abstract class DetailsListFormItem {
    protected card: DetailsListItemCard;
    protected alias: string;
    protected displayValue: string;
    protected description: string;
    protected icon: string;
    protected iconLibrary: string;
    protected header: string;
    protected subHeader: string;
    protected groupName: string;
    protected isRoundIcon: boolean;
    protected actionIcon: DetailsListItemActionIcon;
    protected clickEventSubject: Subject<void>;
    protected isInitVisible: boolean;
    protected validator: Array<ValidatorFn>;
    protected isRepeating: boolean;
    protected formControlType: string;
    protected formControl: AbstractControl;
    protected visible: boolean = true;
    public customErrorMessageOnRequiredField: string = "";
    public customAdditionalPropertyField: boolean = false;
    public paragraphs: Array<string> = [];
    public hint: string;
    protected updateOn: 'change' | 'blur' | 'submit' = 'change';

    // if this item is a related entity.
    public relatedEntityType: RelatedEntityType;
    public relatedEntityOwnerId: string;
    public relatedEntityCustomerId: string;
    public relatedEntityOrganisationId: string;
    public includeSectionIcons: boolean = true;

    public canShow: boolean = true;
    public canShowIcon: boolean = true;

    public constructor(
        card: DetailsListItemCard,
        formControlType: string,
        groupName: string,
        alias: string,
        displayValue: string,
        description: string,
        icon: string,
        iconLibrary: string,
        header: string,
        validator: Array<ValidatorFn> = [],
        actionIcon: DetailsListItemActionIcon = null,
        isRoundIcon: boolean = false,
        visible: boolean = true,
        isRepeating: boolean = false,
        subheader: string = "",
        includeSectionIcons: boolean = true,
    ) {
        this.card = card;
        this.validator = validator;
        this.formControlType = formControlType;
        this.groupName = groupName || (card != null ? card.Name : null);
        this.alias = alias;
        this.displayValue = displayValue;
        this.description = description;
        this.icon = icon;
        this.iconLibrary = iconLibrary;
        this.header = header;
        this.isRoundIcon = isRoundIcon;
        this.actionIcon = actionIcon;
        this.clickEventSubject = new Subject<void>();
        this.visible = visible;
        this.isInitVisible = visible;
        this.isRepeating = isRepeating;
        this.subHeader = subheader;
        this.includeSectionIcons = includeSectionIcons;
    }

    public withAction(
        actionIcon: DetailsListItemActionIcon,
    ): DetailsListFormItem {
        this.actionIcon = actionIcon;
        return this;
    }

    public withGroupName<T extends DetailsListFormItem>(groupName: string): T {
        this.groupName = groupName;
        return this as unknown as T;
    }

    public withRelatedEntity(
        relatedEntityType: RelatedEntityType,
        relatedEntityOrganisationId: string,
        relatedEntityOwnerId: string,
        relatedEntityCustomerId: string,
    ): DetailsListFormItem {
        this.relatedEntityType = relatedEntityType;
        this.relatedEntityOrganisationId = relatedEntityOrganisationId;
        this.relatedEntityOwnerId = relatedEntityOwnerId;
        this.relatedEntityCustomerId = relatedEntityCustomerId;
        return this;
    }

    public withValidator<T extends DetailsListFormItem>(validator: Array<ValidatorFn>, updateOn?: string): T {
        this.validator = validator;
        return this as unknown as T;
    }

    public withUpdateOn(updateOn: 'change' | 'blur' | 'submit'): DetailsListFormItem {
        this.updateOn = updateOn;
        return this;
    }

    public roundIcon(): DetailsListFormItem {
        this.isRoundIcon = true;
        return this;
    }

    public visibleOnInit(): DetailsListFormItem {
        this.isInitVisible = true;
        return this;
    }

    public hiddenOnInit(): DetailsListFormItem {
        this.isInitVisible = false;
        return this;
    }

    public asRepeating<T extends DetailsListFormItem>(): T {
        this.isRepeating = true;
        return this as unknown as T;
    }

    public withIcon<T extends DetailsListFormItem>(icon: string, iconLibrary: string = IconLibrary.IonicV4): T {
        this.icon = icon;
        this.iconLibrary = iconLibrary;
        return this as unknown as T;
    }

    public withHeader<T extends DetailsListFormItem>(heading: string): T {
        this.header = heading;
        return this as unknown as T;
    }

    public withSubHeader(subHeader: string): DetailsListFormItem {
        this.subHeader = subHeader;
        return this;
    }

    public withParagraph<T extends DetailsListFormItem>(paragraph: string): T {
        this.paragraphs.push(paragraph);
        return this as unknown as T;
    }

    public withHint<T extends DetailsListFormItem>(hint: string): T {
        this.hint = hint;
        return this as unknown as T;
    }

    public withoutSectionIcons<T extends DetailsListFormItem>(): T {
        this.includeSectionIcons = false;
        return this as unknown as T;
    }

    public get GroupName(): string {
        return this.groupName;
    }

    public get Alias(): string {
        return this.alias;
    }

    public set Alias(alias: string) {
        this.alias = alias;
    }

    public get DisplayValue(): string {
        return this.displayValue;
    }

    public set DisplayValue(displayValue: string) {
        this.displayValue = displayValue;
    }

    public get Description(): string {
        return this.description;
    }

    public get Icon(): string {
        return this.icon;
    }

    public set Icon(icon: string) {
        this.icon = icon;
    }

    public get IconLibrary(): string {
        return this.iconLibrary;
    }

    public get Header(): string {
        return this.header;
    }

    public get SubHeader(): string {
        return this.subHeader;
    }

    public get Paragraphs(): Array<string> {
        return this.paragraphs;
    }

    public get Hint(): string {
        return this.hint;
    }

    public get IncludeSectionIcons(): boolean {
        return this.includeSectionIcons;
    }

    public get IsRoundIcon(): boolean {
        return this.isRoundIcon;
    }

    public get Validator(): Array<ValidatorFn> {
        return this.validator;
    }

    public set Validator(validator: Array<ValidatorFn>) {
        this.validator = validator;
    }

    public set Visible(visible: boolean) {
        this.visible = visible;
        if (this.formControl) {
            if (visible) {
                this.formControl.setValidators(this.validator);
            } else {
                this.formControl.clearValidators();
            }
        }
    }

    public get Visible(): boolean {
        return this.visible;
    }

    public get ClickEventObservable(): Observable<void> {
        return this.clickEventSubject;
    }

    public onClick(): void {
        this.clickEventSubject.next();
    }

    public get ActionIcon(): DetailsListItemActionIcon {
        return this.actionIcon;
    }

    public get IsInitVisible(): boolean {
        return this.isInitVisible;
    }

    public get IsRepeating(): boolean {
        return this.isRepeating;
    }

    public get FormControl(): AbstractControl {
        if (this.formControl) {
            return this.formControl;
        }
        this.formControl = new FormControl(
            "",
            {
                validators: this.visible ? this.validator : null,
                updateOn: this.updateOn,
            });
        return this.formControl;
    }

    public get FormControlType(): string {
        return this.formControlType;
    }

    public get ValidationMessage(): string {
        if (this.formControl && this.formControl.errors) {
            let errorKeys: Array<string> = Object.keys(this.formControl.errors);
            let firstErrorKey: string = errorKeys[0];
            let errorData: any = this.formControl.errors[firstErrorKey];
            return ValidationMessages.getValidationMessageByType(firstErrorKey, this.description, errorData);
        }
        return null;
    }

    public toDetailsListItem(): DetailsListItem {
        let item: DetailsListItem = DetailsListItem.createItem(
            this.card,
            this.groupName,
            this.alias,
            this.description,
            this.icon,
            this.iconLibrary,
            this.header);
        if (this.DisplayValue) {
            item.setDisplayValue(this.DisplayValue);
        }
        return item;
    }

    public abstract clone(): DetailsListFormItem;

    public isValueCompletelyEmpty(): boolean {
        return !this.formControl.value;
    }

    public isValueOnlyPartiallyFilled(): boolean {
        return !this.formControl.value;
    }
}
