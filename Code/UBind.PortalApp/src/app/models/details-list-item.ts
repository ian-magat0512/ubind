import { DetailsListItemActionIcon } from "./details-list-item-action-icon";
import { DetailsListItemCard } from "./details-list-item-card";
import { Subject, Observable } from "rxjs";
import { DetailsListItemGroupType } from "./details-list-item-type.enum";
import { PhoneNumberPipe } from "@app/pipes/phone-number.pipe";
import { Permission } from "@app/helpers";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { IconLibrary } from "./icon-library.enum";

/**
 * Export details list item class.
 * TODO: Write a better class header: list of the items details.
 */
export class DetailsListItem {

    public allowedPermissions: Permission | Array<Permission>;
    public toggleVisibility: boolean = true;
    public isCardView: false;

    private id: string;
    private displayValue: string;
    public fieldType: string;
    private description: string;
    private icon: string;
    private iconLibrary: string;
    private header: string;
    private groupName: string;
    private isRoundIcon: boolean;
    private actionIcon: DetailsListItemActionIcon;
    private actionIcons: Array<DetailsListItemActionIcon>;
    private shortcutIcon: DetailsListItemActionIcon;
    private card: DetailsListItemCard;
    private fieldName: string;
    private isDefault: boolean;
    public isListViewOnly: boolean;
    public multilineItems: Array<string>;
    private clickEventSubject: Subject<void>;
    private itemClass: string;
    private iconClass: string;
    private iconSize: string = 'medium';
    public maxLines: number = 2;
    public isMultiLine: boolean;
    public lineCount: number = 1;
    private hasLink: boolean;
    private isInitVisible: boolean;
    private isSubFormGroup: boolean = false;
    public includeSectionIcons: boolean = true;

    // if this item is a relationship item.
    public relatedEntityType: RelatedEntityType;
    public relatedEntityOwnerId: string;
    public relatedEntityCustomerId: string;
    public relatedEntityOrganisationId: string;

    private constructor(
        card: DetailsListItemCard,
        groupName: string,
        displayValue: string,
        description: string,
        icon: string,
        iconLibrary: string,
        header: string,
        hasLink: boolean = false,
        actionIcon: DetailsListItemActionIcon = null,
        isRoundIcon: boolean = false,
        isInitVisible: boolean = true,
        maxLines: number = 0,
        iconSize: string = 'medium',
        lineCount: number = 1,
        includeSectionIcons: boolean = true,
        isMultiLine: boolean = false,
    ) {

        this.groupName = groupName;
        this.displayValue = displayValue;
        this.description = description;
        this.icon = icon;
        this.iconLibrary = iconLibrary;
        this.header = header;
        this.isRoundIcon = isRoundIcon;
        this.actionIcon = actionIcon;
        this.card = card;
        this.iconSize = iconSize;
        this.maxLines = this.groupName == "address" ? 3 : maxLines;
        this.hasLink = hasLink;
        this.setCssClasses();
        this.clickEventSubject = new Subject<void>();
        this.isInitVisible = isInitVisible;
        this.lineCount = lineCount;
        this.isMultiLine = isMultiLine;
        this.multilineItems = isMultiLine && displayValue ? displayValue.split(",") : [];
        this.includeSectionIcons = includeSectionIcons;
    }

    public static createItem(
        card: DetailsListItemCard,
        groupName: string,
        displayValue: string,
        description: string,
        icon: string,
        iconLibrary: string = IconLibrary.IonicV4,
        header: string = "",
        maxLines: number = 2,
        lineCount: number = 0,
        includeSectionIcons: boolean = true,
        isMultiLine: boolean = false,
    ): DetailsListItem {
        let item: DetailsListItem = new DetailsListItem(
            card,
            groupName,
            displayValue,
            description,
            icon,
            iconLibrary,
            header,
            false,
            null,
            false,
            true,
            maxLines,
            'medium',
            lineCount,
            includeSectionIcons,
            isMultiLine,
        );
        return item;
    }

    public withAllowedPermissions(
        permission: Permission | Array<Permission>,
    ): this {
        this.allowedPermissions = permission;
        return this;
    }

    public withFieldType(
        fieldType: string,
    ): DetailsListItem {
        this.fieldType = fieldType;
        return this;
    }

    public withAction(
        actionIcon: DetailsListItemActionIcon,
    ): DetailsListItem {
        this.actionIcon = actionIcon;
        this.actionIcons = null;
        return this;
    }

    public withActions(
        ...actionIcons: Array<DetailsListItemActionIcon>
    ): DetailsListItem {
        this.actionIcons = actionIcons.filter((i: DetailsListItemActionIcon) => i !== null);
        this.actionIcon = null;
        return this;
    }

    public withRelatedEntity(
        relatedEntityType: RelatedEntityType,
        relatedEntityOrganisationId: string,
        relatedEntityOwnerId: string,
        relatedEntityCustomerId: string,
    ): DetailsListItem {
        this.relatedEntityType = relatedEntityType;
        this.relatedEntityOrganisationId = relatedEntityOrganisationId;
        this.relatedEntityOwnerId = relatedEntityOwnerId;
        this.relatedEntityCustomerId = relatedEntityCustomerId;

        return this;
    }

    public withAShortcut(
        shortcutIcon: DetailsListItemActionIcon,
    ): DetailsListItem {
        this.shortcutIcon = shortcutIcon;
        return this;
    }

    public withFieldName(
        fieldName: string,
    ): DetailsListItem {
        this.fieldName = fieldName;
        return this;
    }

    public withLink(): DetailsListItem {
        this.hasLink = true;
        this.clickEventSubject = new Subject<void>();
        this.setCssClasses();
        return this;
    }

    public subFormGroup(): DetailsListItem {
        this.isSubFormGroup = true;
        return this;
    }

    public roundIcon(): DetailsListItem {
        this.isRoundIcon = true;
        return this;
    }

    public visibleOnInit(): DetailsListItem {
        this.isInitVisible = true;
        return this;
    }

    public hiddenOnInit(): DetailsListItem {
        this.isInitVisible = false;
        return this;
    }

    public setAsDefault(isDefault: boolean): DetailsListItem {
        this.isDefault = isDefault;
        return this;
    }

    public asListViewOnly(): DetailsListItem {
        this.isListViewOnly = true;
        return this;
    }

    public setId(id: string): DetailsListItem {
        this.id = id;
        return this;
    }

    public setDisplayValue(displayValue: string): DetailsListItem {
        this.displayValue = displayValue;
        return this;
    }

    public formatItemDisplay(): DetailsListItem {
        // format for display
        if (this.GroupName == DetailsListItemGroupType.Phone) {
            let phoneNumberPipe: PhoneNumberPipe = new PhoneNumberPipe();
            this.DisplayValue = this.DisplayValue ? phoneNumberPipe.transform(this.DisplayValue) : "";
        }

        if (this.GroupName == DetailsListItemGroupType.Email || this.GroupName == DetailsListItemGroupType.Account) {
            this.DisplayValue = this.DisplayValue ? this.DisplayValue.toLowerCase() : "";
        }
        return this;
    }

    public setCssClasses(): void {
        if (this.hasLink) {
            this.itemClass = 'details-item-' + this.maxLines + '-lines-link';
        } else {
            this.itemClass = this.icon != 'none' ?
                "details-item-" + this.maxLines + "-lines" : "details-item-" + this.maxLines + "-lines-no-icon";
        }

        this.iconClass = "details-item-icon-" + this.iconSize + "-" + this.maxLines + "-lines";
    }

    public get GroupName(): string {
        return this.groupName;
    }

    public get Id(): string {
        return this.id;
    }

    public get DisplayValue(): string {
        return this.displayValue;
    }

    public set DisplayValue(title: string) {
        this.displayValue = title;
    }

    public set Description(description: string) {
        this.description = description;
    }

    public get Description(): string {
        return this.description;
    }

    public get HasLink(): boolean {
        return this.hasLink;
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

    public get IsRoundIcon(): boolean {
        return this.isRoundIcon;
    }

    public get IncludeSectionIcons(): boolean {
        return this.includeSectionIcons;
    }

    public get IsSubFormGroup(): boolean {
        return this.isSubFormGroup;
    }

    public get IconClass(): string {
        return this.iconClass;
    }

    public get ItemClass(): string {
        return this.itemClass;
    }

    public get ClickEventObservable(): Observable<void> {
        return this.clickEventSubject;
    }

    public onClick(event: any, item: DetailsListItem): void {
        let params: any = {
            event: event,
            item: item,
            id: item.id,
        };

        this.clickEventSubject.next(params);
    }

    public get ActionIcon(): DetailsListItemActionIcon {
        return this.actionIcon;
    }

    public set ActionIcon(value: DetailsListItemActionIcon) {
        this.actionIcon = value;
    }

    public get ActionIcons(): Array<DetailsListItemActionIcon> {
        return this.actionIcons;
    }

    public set ActionIcons(array: Array<DetailsListItemActionIcon>) {
        this.actionIcons = array;
    }

    public get ShortcutIcon(): DetailsListItemActionIcon {
        return this.shortcutIcon;
    }

    public get Card(): DetailsListItemCard {
        return this.card;
    }

    public get IsInitVisible(): boolean {
        return this.isInitVisible;
    }

    public get FieldName(): string {
        return this.fieldName;
    }

    public get IsDefault(): boolean {
        return this.isDefault;
    }

    public get IsListViewOnly(): boolean {
        return this.isListViewOnly;
    }
}
