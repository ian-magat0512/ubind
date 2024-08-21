import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RoleResourceModel } from '@app/resource-models/role.resource-model';
import { Permission } from '@app/helpers/permissions.helper';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export user role view Component Class.
 * TODO: Write a better class header: view of user role.
 */
@Component({
    selector: 'app-user-role-view',
    templateUrl: 'user-role-view.component.html',
    styleUrls: ['user-role-view.component.scss'],
})

export class UserRoleViewComponent {

    @Input() public roles: Array<RoleResourceModel>;
    @Input() public isLoading: boolean = false;
    @Input() public showActionButton: boolean = false;
    @Output() public clickedRoleActionButton: EventEmitter<RoleResourceModel> = new EventEmitter<RoleResourceModel>();
    @Input() public errorMessage: string;

    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor() { }

    public clickRoleActionButton(role: RoleResourceModel): void {
        this.clickedRoleActionButton.emit(role);
    }
}
