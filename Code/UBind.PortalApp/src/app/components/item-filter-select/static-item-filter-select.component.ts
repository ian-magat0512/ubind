import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { IconLibrary } from '@app/models/icon-library.enum';
import { SelectableItem } from './item-filter-select.component';

/**
 * Component for filtering and selecting an item from a list of items.
 * It presents a list of items which can be filtered by typing in the filter input.
 * When an item is selected, the itemSelected event is emitted.
 */
@Component({
    selector: 'app-static-item-filter-select',
    templateUrl: './static-item-filter-select.component.html',
    styleUrls: ['./static-item-filter-select.component.scss'],
})
export class StaticItemFilterSelectComponent implements OnChanges {

    @Input()
    public items: Array<SelectableItem> = new Array<SelectableItem>();

    @Input()
    public placeholder: string = 'Filter items';

    @Output()
    public itemSelected: EventEmitter<SelectableItem> = new EventEmitter<SelectableItem>();

    private filterValue: string = '';
    public filteredItems: Array<SelectableItem> = new Array<SelectableItem>();
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public IconLibrary: typeof IconLibrary = IconLibrary;

    public ngOnChanges(changes: SimpleChanges): void {
        if (Object.prototype.hasOwnProperty.call(changes, 'items')) {
            this.filterItems();
        }
    }

    public onItemSelected(item: SelectableItem): void {
        this.itemSelected.emit(item);
    }

    public onFilterChanged(value: string): void {
        this.filterValue = value;
        this.filterItems();
    }

    public filterItems(): void {
        if (!this.filterValue) {
            this.filteredItems = this.items;
        } else {
            this.filteredItems = this.items.filter((item: SelectableItem) =>
                item.searchableText.toLowerCase().includes(this.filterValue.toLowerCase()));
        }
    }
}
