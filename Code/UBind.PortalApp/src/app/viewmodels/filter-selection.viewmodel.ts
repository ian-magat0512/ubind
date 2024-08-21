import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Export filter selection class.
 * TODO: Write a better class header: view model of filter selection.
 */
export class FilterSelection {
    public value: string;
    public propertyName: string;
    public icon: string;
    public iconLibrary: string;
    public label: string;

    public constructor(
        propertyName: string,
        value: string,
        icon: string,
        label: string = value,
        iconLibrary: string = IconLibrary.IonicV4,
    ) {
        this.propertyName = propertyName;
        this.value = value;
        this.icon = icon;
        this.iconLibrary = iconLibrary;
        this.label = label;
    }
}

/**
 * Export search keyword filter selection class.
 * TODO: Write a better class header: view model of searching of keywork filter selection.
 */
export class SearchKeywordFilterSelection extends FilterSelection {
    public constructor(keyword: string) {
        super("search", keyword, "search");
    }
}
