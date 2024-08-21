import { Component } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { Permission } from '@app/helpers';
import { PermissionService } from '@app/services/permission.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { PopoverCommand } from '@app/models/popover-command';

/**
 * Export popover contact details action class
 */
@Component({
    templateUrl: './popover-contact-details-actions.component.html',
})
export class PopoverContactDetailsActionsComponent {
    public shouldShowItemCall: boolean = false;
    public shouldShowItemSendText: boolean = false;
    public shouldShowItemCopyToClipboard: boolean = true;
    public shouldShowItemSetToDefault: boolean = false;
    public shouldShowItemEditPhone: boolean = false;
    public shouldShowItemSendEmail: boolean = false;
    public shouldShowItemEditEmail: boolean = false;
    public shouldShowItemOpenGoogleMap: boolean = false;
    public shouldShowItemEditAddress: boolean = false;
    public shouldShowItemOpenWebsite: boolean = false;
    public shouldShowItemEditWebsite: boolean = false;
    public shouldShowItemStartVideoCall: boolean = false;
    public shouldShowItemOpenChat: boolean = false;
    public shouldShowItemEditMessengerId: boolean = false;
    public shouldShowItemEditSocialId: boolean = false;
    public permission: typeof Permission = Permission;
    public hasCustomerEditPermission: boolean = false;

    public constructor(
private popOverCtrl: PopoverController,
navParams: NavParams,
        private permissionService: PermissionService,
        browserDetectionService: BrowserDetectionService,
    ) {
        let phone: any = navParams.get("phone");
        let email: any = navParams.get("email");
        let address: any = navParams.get("address");
        let website: any = navParams.get("website");
        let messenger: any = navParams.get("messenger");
        let social: any = navParams.get("social");
        let isDefault: any = navParams.get("default");
        let isMyAccount: any = navParams.get("isMyAccount");
        let label: any = navParams.get("label");
        this.hasCustomerEditPermission = this.permissionService.hasPermission(Permission.ManageCustomers);
        let isMobileDevice: boolean = browserDetectionService.isMobile;
        let canEdit: any = this.hasCustomerEditPermission || isMyAccount;
        if (phone) {
            this.shouldShowItemCall = true && isMobileDevice;
            this.shouldShowItemSendText = true && isMobileDevice;
            this.shouldShowItemEditPhone = canEdit;
        }

        if (email) {
            this.shouldShowItemSendEmail = true;
            this.shouldShowItemEditEmail = canEdit;
        }

        if (address) {
            this.shouldShowItemOpenGoogleMap = label.toLowerCase().indexOf("postal") < 0;
            this.shouldShowItemEditAddress = canEdit;
        }

        if (website) {
            this.shouldShowItemOpenWebsite = true;
            this.shouldShowItemEditWebsite = canEdit;
        }

        if (messenger) {
            this.shouldShowItemStartVideoCall = true;
            this.shouldShowItemOpenChat = true;
            this.shouldShowItemEditMessengerId = canEdit;
        }

        if (social) {
            this.shouldShowItemEditSocialId = true;
            this.shouldShowItemSetToDefault = false;
        }

        if (!isDefault && !social) {
            this.shouldShowItemSetToDefault = canEdit;
        }

        if (isMyAccount) {
            this.shouldShowItemCall = false;
            this.shouldShowItemSendText = false;
            this.shouldShowItemSendEmail = false;
            this.shouldShowItemStartVideoCall = false;
            this.shouldShowItemOpenChat = false;
        }
    }

    public close(command: PopoverCommand): void {
        this.popOverCtrl.dismiss(command);
    }

}

export enum ContactDetailsPopOverActions {
    Call = 'call',
    SendText = 'sendText',
    CopyToClipBoard = 'copyToClipBoard',
    SetToDefault = 'setToDefault',
    SendEmail = 'sendEmail',
    OpenGoogleMap = 'openGoogleMap',
    OpenWebsite = 'openWebsite',
    StartVideoCall = 'startVideoCall',
    OpenChat = 'openChat',
    EditField = 'editField'
}
