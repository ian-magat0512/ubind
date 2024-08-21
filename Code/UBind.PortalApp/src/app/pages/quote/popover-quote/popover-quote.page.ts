import { Component, OnInit } from "@angular/core";
import { NavParams, PopoverController } from "@ionic/angular";
import { QuoteState } from "@app/models";
import { Permission } from "@app/helpers";
import { AuthenticationService } from "@app/services/authentication.service";
import { PermissionService } from "@app/services/permission.service";
import { ActionButtonPopover } from "@app/models/action-button-popover";
import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Export popover quote page component class.
 * This class manage quote popover page function.
 */
@Component({
    selector: 'app-popover-quote',
    templateUrl: './popover-quote.page.html',
})
export class PopoverQuotePage implements OnInit {
    public action: string;
    public actionIcon: string;
    public status: string;
    public expiryEnabled: boolean;
    public canEditAdditionalPropertyValues: boolean;
    public permission: typeof Permission = Permission;
    public quoteState: any = QuoteState;
    public isCustomer: boolean;
    public hasEditAdditionalPropertyValuesPermission: boolean = false;
    public isDefaultOptionsEnabled: boolean = false;
    public actions: Array<ActionButtonPopover> = [];
    public canResumeQuote: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public canIssuePolicy: boolean = false;

    public constructor(
        private navParams: NavParams,
        public popOverCtrl: PopoverController,
        private authService: AuthenticationService,
        private permissionService: PermissionService,
    ) { }

    public ngOnInit(): void {
        this.isCustomer = this.authService.isCustomer();
        this.action = this.navParams.get('action');
        this.status = this.navParams.get('status');
        this.expiryEnabled = this.navParams.get('expiryEnabled');
        this.hasEditAdditionalPropertyValuesPermission =
            this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        this.actions = this.navParams.get('actions') || [];
        this.isDefaultOptionsEnabled = this.navParams.get('isDefaultOptionsEnabled');
        this.canEditAdditionalPropertyValues = this.navParams.get('canEditAdditionalPropertyValues');
        this.actionIcon = this.action == 'Review'
            ? 'feature-search'
            : this.action == 'Endorse'
                ? 'stamper'
                : '';
        this.canResumeQuote = this.navParams.get('canResumeQuote');
        this.canIssuePolicy = this.navParams.get('canIssuePolicy');
    }

    public doAction(actionName: string): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: actionName } });
    }
}
