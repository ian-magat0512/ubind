import { Component, Input, Output, EventEmitter } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { FilterSelection } from '@app/viewmodels/filter-selection.viewmodel';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export filter chips components class
 * This class is has a action to remove the filter
 * selections.
 */
@Component({
    selector: 'app-filter-chips',
    templateUrl: './filter-chips.component.html',
    styleUrls: [
        './filter-chips.component.scss',
    ],
    animations: [contentAnimation],
})
export class FilterChipsComponent {
    @Input()
    public filterSelections: Array<FilterSelection>;

    @Output()
    public filterSelectionRemovedEvent: EventEmitter<any> = new EventEmitter<any>();

    public iconLibrary: typeof IconLibrary = IconLibrary;

    public remove(filterSelection: FilterSelection): void {
        this.filterSelections.splice(this.filterSelections.indexOf(filterSelection, 0), 1);

        if (this.filterSelectionRemovedEvent) {
            this.filterSelectionRemovedEvent.emit(filterSelection);
        }
    }
}
