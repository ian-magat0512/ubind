import { Component, EventEmitter, Input, Output } from '@angular/core';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Represents an item which can be selected from the list of displayed items
 */
export interface SelectableItem {
    id: string;
    icon: string;
    iconLibrary: IconLibrary;
    name: string;
    searchableText: string;
}

/**
 * Component for filtering and selecting an item from a list of items.
 * It presents a list of items which can be filtered by typing in the filter input.
 * When an item is selected, the itemSelected event is emitted.
 */
@Component({
    selector: 'app-item-filter-select',
    templateUrl: './item-filter-select.component.html',
    styleUrls: ['./item-filter-select.component.scss'],
})
export class ItemFilterSelectComponent {

    @Input()
    public filteredItems: Array<SelectableItem> = new Array<SelectableItem>();

    @Input()
    public placeholder: string = 'Filter items';

    @Output()
    public itemSelected: EventEmitter<SelectableItem> = new EventEmitter<SelectableItem>();

    @Output()
    public filterChanged: EventEmitter<string> = new EventEmitter<string>();

    // eslint-disable-next-line @typescript-eslint/naming-convention
    public IconLibrary: typeof IconLibrary = IconLibrary;

    public onItemSelected(item: SelectableItem): void {
        this.itemSelected.emit(item);
    }

    public onFilterChanged(value: string): void {
        this.filterChanged.emit(value);
    }
}
