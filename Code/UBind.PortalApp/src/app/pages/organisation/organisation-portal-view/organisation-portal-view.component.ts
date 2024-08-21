import { Component, Input, Output, EventEmitter } from '@angular/core';
import { IconLibrary } from '@app/models/icon-library.enum';
import { PortalViewModel } from '@app/viewmodels/portal.viewmodel';

/**
 * Export Organisation User view component class
 * TODO: Write a better class header: user view in organisation.
 */
@Component({
    selector: 'app-organisation-portal-view',
    templateUrl: 'organisation-portal-view.component.html',
})

export class OrganisationPortalViewComponent {

    @Input() public portalViewModels: Array<PortalViewModel>;
    @Input() public isLoading: boolean = false;
    @Input() public errorMessage: string;
    @Output() public clickedPortalActionButton: EventEmitter<PortalViewModel> = new EventEmitter<PortalViewModel>();

    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor() { }

    public userDidSelectItem(portalViewModel: PortalViewModel): void {
        this.clickedPortalActionButton.emit(portalViewModel);
    }
}
