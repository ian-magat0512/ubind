import { Component, OnInit, Input, EventEmitter, Output, OnDestroy } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { PolicyTransactionResourceModel } from '@app/resource-models/policy.resource-model';
import { PolicyHelper } from '@app/helpers';
import { Subscription } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { PolicyTransactionViewModel } from '@app/viewmodels/policy-transaction.viewmodel';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Export history policy view component class
 * This is to display the history of the policy.
 */
@Component({
    selector: 'app-history-policy-view',
    templateUrl: './history-policy-view.component.html',
})
export class HistoryPolicyViewComponent implements OnInit, OnDestroy {

    @Input() public policyId: string;
    @Output() public policyHistoryEvent: EventEmitter<string> = new EventEmitter<string>();
    public selectedId: string;
    public historyData: Array<PolicyTransactionViewModel>;
    public historyDataDates: Array<string>;
    public transactionStatus: string;
    public isMutual: boolean;
    private isTransaction: boolean = false;
    private getPolicyHistoryListSubscription: Subscription;

    public constructor(
        private navProxy: NavProxyService,
        private route: ActivatedRoute,
        public policyApiService: PolicyApiService,
        protected routeHelper: RouteHelper,
        protected userPath: UserTypePathHelper,
        protected authService: AuthenticationService,
    ) {
    }

    public ngOnInit(): void {
        this.policyId = this.routeHelper.getParam('policyId');
        this.historyDataDates = [];
        this.getPolicyHistoryListSubscription = this.policyApiService.getPolicyHistoryList(this.policyId)
            .subscribe((policyTransactions: Array<PolicyTransactionResourceModel>) => {
                // PolicyTransactions = policyTransactions.sort((a, b) => PolicyHelper.sortDate(a, b));
                this.historyData = new Array<PolicyTransactionViewModel>();
                policyTransactions.forEach((pt: PolicyTransactionResourceModel) =>
                    this.historyData.push(new PolicyTransactionViewModel(pt)));
                this.historyDataDates = Array.from(
                    new Set(this.historyData.map((item: PolicyTransactionViewModel) =>
                        PolicyHelper.formatPolicyDate(item.createdDate))),
                );
                this.policyHistoryEvent.emit(this.historyData[0].policyNumber);
            });

        this.isTransaction = this.route.snapshot.url.toString().search(/transaction/g) >= 0;
        if (this.isTransaction) {
            this.selectedId = this.route.snapshot.paramMap.get("transactionId");
            this.transactionStatus = this.route.snapshot.paramMap.get("status");
        }

        this.isMutual = this.authService.isMutualTenant();
    }

    public ngOnDestroy(): void {
        this.getPolicyHistoryListSubscription.unsubscribe();
    }

    public getPolicyTransactionListByDate(dateString: string): Array<PolicyTransactionViewModel> {
        return this.historyData
            .filter((x: PolicyTransactionViewModel) => PolicyHelper.formatPolicyDate(x.createdDate) === dateString)
            .sort((a: PolicyTransactionViewModel, b: PolicyTransactionViewModel) => PolicyHelper.sortDate(a, b));
    }

    public goToPage(transaction: PolicyTransactionViewModel): void {
        this.selectedId = transaction.transactionId;
        this.navProxy.navigate([this.userPath.policy, this.policyId, 'transaction', this.selectedId]);
    }

    public isCancelled(policyTransaction: PolicyTransactionViewModel): boolean {
        return policyTransaction.eventTypeSummary === PolicyHelper.constants.Labels.Status.Cancelled;
    }

    public isNew(policyTransaction: PolicyTransactionViewModel): boolean {
        return policyTransaction.eventTypeSummary === PolicyHelper.constants.Labels.Status.Purchased;
    }

    public isRenewed(policyTransaction: PolicyTransactionViewModel): boolean {
        return policyTransaction.eventTypeSummary === PolicyHelper.constants.Labels.Status.Renewed;
    }

    public isAdjusted(policyTransaction: PolicyTransactionViewModel): boolean {
        return policyTransaction.eventTypeSummary === PolicyHelper.constants.Labels.Status.Adjusted;
    }
}
