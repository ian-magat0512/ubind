import { Component } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { EntityType } from '@app/models/entity-type.enum';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { Permission } from '@app/helpers';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Drop down menu to manage a persons account
 */
@Component({
    templateUrl: './popover-person.component.html',
})
export class PopoverPersonComponent {
    protected newStatusTitle: string;

    public entityType: EntityType;

    public shouldShowPopOverEdit: boolean = false;
    public shouldShowPopOverNewStatus: boolean = false;
    public shouldShowPopOverResendStatus: boolean = false;
    public shouldShowPopOverDisableStatus: boolean = false;
    public shouldShowPopOverEnableStatus: boolean = false;
    public shouldShowPopOverSetToPrimaryPerson: boolean = false;
    public shouldShowPopOverDeletePerson: boolean = false;
    public shouldShowPopOverCreateQuote: boolean = false;
    public shouldShowPopOverCreateClaim: boolean = false;
    public shouldShowPopOverCreatePerson: boolean = false;
    public shouldShowPopOverEditAdditionalProperties: boolean = false;
    public shouldShowPopOverAssignRole: boolean = false;

    public actions: Array<ActionButtonPopover> = [];
    public isDefaultOptionsEnabled: boolean = false;
    public permission: typeof Permission = Permission;
    public segment: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(public popOverCtrl: PopoverController, private navParams: NavParams) {
        this.newStatusTitle = navParams.get('newStatusTitle') || 'Send activation email';
        this.entityType = navParams.get('entityType');

        this.isDefaultOptionsEnabled = navParams.get('isDefaultOptionsEnabled');
        this.shouldShowPopOverEdit = navParams.get('shouldShowPopOverEdit');
        this.shouldShowPopOverEditAdditionalProperties =
            navParams.get('shouldShowPopOverEditAdditionalProperties');
        this.shouldShowPopOverNewStatus = navParams.get('shouldShowPopOverNewStatus');
        this.shouldShowPopOverResendStatus = navParams.get('shouldShowPopOverResendStatus');
        this.shouldShowPopOverDisableStatus = navParams.get('shouldShowPopOverDisableStatus');
        this.shouldShowPopOverEnableStatus = navParams.get('shouldShowPopOverEnableStatus');
        this.shouldShowPopOverSetToPrimaryPerson = navParams.get('shouldShowPopOverSetToPrimaryPerson');
        this.shouldShowPopOverDeletePerson = navParams.get('shouldShowPopOverDeletePerson');
        this.shouldShowPopOverCreateQuote = navParams.get('shouldShowPopOverCreateQuote');
        this.shouldShowPopOverCreateClaim = navParams.get('shouldShowPopOverCreateClaim');
        this.shouldShowPopOverCreatePerson = navParams.get('shouldShowPopOverCreatePerson');
        this.shouldShowPopOverAssignRole = navParams.get('shouldShowPopOverAssignRole');

        this.actions = navParams.get('actions') || [];
        this.segment = navParams.get('segment');
    }

    public close(action: string): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: action } });
    }
}

export enum PersonPopOverStatus {
    Activate = 'activate',
    ResendActivate = 'resendActivate',
    Disable = 'disable',
    Enable = 'enable',
    SetToPrimary = 'setToPrimary',
    Delete = 'delete',
    EditAdditionalPropertyValues = 'editAdditionalPropertyValues',
    Edit = 'edit',
    AssignRole = 'assignRole',
}
