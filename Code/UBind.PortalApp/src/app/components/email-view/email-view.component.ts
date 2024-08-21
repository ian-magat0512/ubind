import { Component, OnInit, Input } from '@angular/core';
import { ListFilter } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { EmailResourceModel } from '@app/resource-models/email.resource-model';
import { contentAnimation } from '@assets/animations';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { EntityType } from "@app/models/entity-type.enum";
import { PermissionService } from '@app/services/permission.service';
import { MessageService } from '@app/services/message.service';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { IconLibrary } from '@app/models/icon-library.enum';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { Observable } from "rxjs";

/**
 * Export email view component class
 * This is to load the policy emails.
 */
@Component({
    selector: 'app-email-view',
    templateUrl: './email-view.component.html',
    styleUrls: [
        '../../../assets/css/scrollbar-segment.css',
        '../../../assets/css/scrollbar-div.css',
    ],
    styles: [
        scrollbarStyle,
    ],
    animations: [contentAnimation],
})
export class EmailViewComponent implements OnInit {

    @Input() public entityType: EntityType;
    @Input() public entityId: string;
    public questionHeaders: any;
    public questionData: any;
    public repeatingData: any;
    public displayableFieldsModel: any;
    public listFilter: ListFilter<MessageResourceModel>;
    public email: EmailResourceModel;
    public emailsErrorMessage: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public messageTypeViewModel: typeof MessageViewModel = MessageViewModel;

    public constructor(
        public navProxy: NavProxyService,
        public policyApiService: PolicyApiService,
        private featureSettingService: FeatureSettingService,
        protected userPath: UserTypePathHelper,
        protected routeHelper: RouteHelper,
        private permissionService: PermissionService,
        private messageService: MessageService,
    ) {
    }

    public ngOnInit(): void {
        this.listFilter = new ListFilter<MessageResourceModel>();
        if (!this.canAccessEmail()) {
            this.emailsErrorMessage = 'You are not allowed to access emails';
            return;
        }
    }

    public userDidSelectMessage(messageViewModel: MessageViewModel): void {
        if (this.featureSettingService.hasEmailFeature()) {
            if (this.entityId) {
                this.navProxy.goToEntityMessageList(messageViewModel.id, messageViewModel.type);
                return;
            }
            this.navProxy.navigate([this.userPath.message, messageViewModel.id]);
        }
    }

    public getSegmentMessageList(params?: Map<string, string | Array<string>>,
    ): Observable<Array<MessageResourceModel>> {
        return this.messageService.getEntityEmails(this.entityType, this.entityId, params);
    }

    private canAccessEmail(): boolean {
        if (this.permissionService.hasViewEmailPermission()) {
            return true;
        }
        return false;
    }
}
