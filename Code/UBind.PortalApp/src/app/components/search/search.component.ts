import { Component, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { IonSearchbar } from '@ionic/angular';
import { searchAnimation } from '@assets/animations';

/**
 * Export search component class
 * This class is manage the searching.
 */
@Component({
    selector: 'search',
    templateUrl: './search.component.html',
    animations: [searchAnimation],
})
export class SearchComponent {

    @ViewChild('searchbar', { static: true }) public searchbar: IonSearchbar;

    @Input() public placeholder: string;
    @Output() public searchTerm: EventEmitter<string> = new EventEmitter<string>();
    @Output() public cancel: EventEmitter<void> = new EventEmitter<void>();

    public constructor() { }

    public search(event: any): void {
        if (event != undefined && event != null && event.keyCode == 13) {
            let searchString: string = event.target.value;
            if (searchString) {
                // Search terms with multiple words will be considered as a single term and 
                // treated as a single search filter. 
                const term: any = searchString.trim();
                this.searchTerm.emit(term);
            }
            this.onCancel();
        }
    }

    public onCancel(): void {
        this.cancel.emit();
    }

    public setFocus(): void {
        this.searchbar.setFocus();
    }
}
