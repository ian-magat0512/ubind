import { Component, Input, ViewEncapsulation } from "@angular/core";
import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Export history policy view component class
 * This is to display the history of the policy.
 */
@Component({
    selector: 'library-icon',
    templateUrl: './library-icon.component.html',
    styleUrls: ['./library-icon.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class LibraryIconComponent {
    @Input() public name: string;
    @Input() public library: IconLibrary;
    @Input() public cssClass: string;
    @Input() public cssStyle: {[key: string]: string};
    @Input() public slot: string;
    @Input() public size: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor() {
    }
}
