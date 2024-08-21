import { takeUntil } from 'rxjs/operators';
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { AuthenticationService } from '@app/services/authentication.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { PolicyStatus, PremiumResult } from '@app/models';
import { LocalDateHelper, PolicyHelper } from '@app/helpers';
import { Subject } from 'rxjs';
import {
    PolicyPremiumDetailResourceModel, PolicyTransactionResourceModel,
} from '@app/resource-models/policy.resource-model';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { DisplayText } from '@app/models/display-text';

/**
 * Export premium policy view component class
 * To load the premium policy details or transactions.
 */
@Component({
    selector: 'app-premium-policy-view',
    templateUrl: './premium-policy-view.component.html',
    styleUrls: [
        './premium-policy-view.component.scss',
    ],
})
export class PremiumPolicyViewComponent implements OnInit, OnDestroy {
    @Input() public policyId: string;
    public premiumData: PremiumResult;
    public lifetimeStampDutyTotal: DisplayText;
    public currentStampDutyTotal: DisplayText;
    public lifetimeEsl: DisplayText;
    public currentEsl: DisplayText;
    public lifetimeStampDutyPerState: Array<string> = [];
    public currentStampDutyPerState: Array<string> = [];
    public lifetimeEslPerState: Array<string> = [];
    public currentEslPerState: Array<string> = [];
    public isCustomer: boolean = false;
    public isExpandable: boolean = false;
    public hasRenewal: boolean = false;
    public showEsl: boolean = false;
    public isMutual: boolean = false;
    public mostRecentPolicyExpiryDate: string;
    public firstPolicyInceptionDate: string;
    public currentPolicyEffectiveDate: string;
    public currentPolicyEndDate: string;
    public lifetimePremiumSummary: PremiumResult = this.initialisePremiumResult();
    public premiumSummary: PremiumResult = this.initialisePremiumResult();
    public hasCoverageStarted: boolean = false;

    private destroyed: Subject<void>;

    public constructor(
        public policyApiService: PolicyApiService,
        private authService: AuthenticationService,
        protected currencyFormatPipe: CurrencyPipe,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.loadPremiumPolicyDetails();
        this.isCustomer = this.authService.isCustomer();
        this.isExpandable = !this.isCustomer;
        this.isMutual = this.authService.isMutualTenant();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    private loadPremiumPolicyDetails(): void {
        this.policyApiService.getPolicyPremium(this.policyId)
            .pipe(takeUntil(this.destroyed))
            .subscribe((dt: PolicyPremiumDetailResourceModel) => {
                this.policyApiService.getPolicyHistoryList(this.policyId)
                    .subscribe((historyList: Array<PolicyTransactionResourceModel>) => {
                        this.hasCoverageStarted = dt.status != PolicyStatus.Issued;
                        this.premiumData = dt.premiumData;
                        this.retrievePremiumSummaries(historyList, this.premiumData.currencyCode);
                        // Stamp Duty
                        this.lifetimeStampDutyTotal = {
                            title: PolicyHelper.constants.Labels.StampDuty.StampDuty,
                            titleText: this.currencyFormatPipe.transform(
                                this.lifetimePremiumSummary.stampDutyTotal,
                                this.lifetimePremiumSummary.currencyCode,
                            ),
                        };
                        this.lifetimeStampDutyPerState = PolicyHelper.getStampDutyStateData(
                            this.lifetimePremiumSummary,
                            this.currencyFormatPipe,
                        );
                        this.currentStampDutyTotal = {
                            title: PolicyHelper.constants.Labels.StampDuty.StampDuty,
                            titleText: this.currencyFormatPipe.transform(
                                this.premiumSummary.stampDutyTotal,
                                this.premiumSummary.currencyCode,
                            ),
                        };
                        this.currentStampDutyPerState = PolicyHelper.getStampDutyStateData(
                            this.premiumSummary,
                            this.currencyFormatPipe,
                        );
                        // ESL
                        this.lifetimeEsl = {
                            title: PolicyHelper.constants.Labels.Esl.Esl,
                            titleText: this.currencyFormatPipe.transform(
                                this.lifetimePremiumSummary.esl,
                                this.lifetimePremiumSummary.currencyCode,
                            ),
                        };
                        this.lifetimeEslPerState = PolicyHelper.getEslData(
                            this.lifetimePremiumSummary,
                            this.currencyFormatPipe,
                        );
                        this.currentEsl = {
                            title: PolicyHelper.constants.Labels.Esl.Esl,
                            titleText: this.currencyFormatPipe.transform(
                                this.premiumSummary.esl,
                                this.premiumSummary.currencyCode,
                            ),
                        };
                        this.currentEslPerState = PolicyHelper.getEslData(
                            this.premiumSummary,
                            this.currencyFormatPipe,
                        );
                        this.showEsl = this.lifetimePremiumSummary ?
                            (this.lifetimePremiumSummary.esl) !== 0 : false;
                        this.configureDates(dt, historyList);
                    });
            });
    }

    private retrievePremiumSummaries(historyList: Array<PolicyTransactionResourceModel>, currencyCode: string): void {
        const currentActiveRenewalIndex: number = historyList.findIndex(
            this.currentActiveRenewalCallbackFn,
            historyList,
        );
        const currentActivePurchasedIndex: number = historyList.findIndex(
            this.currentPurchaseCallbackFn,
            historyList,
        );
        let lifetimeTransactionList: Array<PolicyTransactionResourceModel> = historyList.slice(0, historyList.length);
        this.lifetimePremiumSummary = this.getPremiumSummary(lifetimeTransactionList, currencyCode);
        const currentPolicyTransactionIndex: number = currentActiveRenewalIndex != -1
            ? currentActiveRenewalIndex
            : currentActivePurchasedIndex != -1
                ? currentActivePurchasedIndex
                : 0;
        let transactionList: Array<PolicyTransactionResourceModel> = this.hasCoverageStarted
            ? historyList
                .filter((p: PolicyTransactionResourceModel) => p.transactionStatus !== "Pending")
                .slice(0, currentPolicyTransactionIndex + 1)
            : historyList.slice(0, currentPolicyTransactionIndex + 1);
        this.premiumSummary = this.getPremiumSummary(transactionList, currencyCode);
    }

    private getPremiumSummary(
        transactionList: Array<PolicyTransactionResourceModel>,
        currencyCode: string,
    ): PremiumResult {
        let initialValue: PremiumResult = this.initialisePremiumResult();
        const premiumSummary: PremiumResult = transactionList.reduce((a: any, b: PolicyTransactionResourceModel) => {
            return {
                basePremium: a.basePremium + Number(b.premium.basePremium),
                terrorismPremium: (a.terrorismPremium ?? 0) + Number((b.premium.terrorismPremium ?? 0)),
                esl: a.esl + Number(b.premium.esl),
                eslNsw: (a.eslNsw ?? 0) + Number((b.premium.eslNsw ?? 0)),
                eslTas: (a.eslTas ?? 0) + Number((b.premium.eslTas ?? 0)),
                premiumGst: a.premiumGst + Number(b.premium.premiumGst),
                stampDutyAct: a.stampDutyAct + Number(b.premium.stampDutyAct),
                stampDutyNsw: a.stampDutyNsw + Number(b.premium.stampDutyNsw),
                stampDutyNt: a.stampDutyNt + Number(b.premium.stampDutyNt),
                stampDutyQld: a.stampDutyQld + Number(b.premium.stampDutyQld),
                stampDutySa: a.stampDutySa + Number(b.premium.stampDutySa),
                stampDutyTas: a.stampDutyTas + Number(b.premium.stampDutyTas),
                stampDutyVic: a.stampDutyVic + Number(b.premium.stampDutyVic),
                stampDutyWa: a.stampDutyWa + Number(b.premium.stampDutyWa),
                stampDutyTotal: a.stampDutyTotal + Number(b.premium.stampDutyTotal),
                totalPremium: a.totalPremium + Number(b.premium.totalPremium),
                currencyCode: currencyCode,
            };
        }, initialValue);
        return premiumSummary;
    }

    private configureDates(
        policyDetail: PolicyPremiumDetailResourceModel,
        historyList: Array<PolicyTransactionResourceModel>,
    ): void {
        this.mostRecentPolicyExpiryDate =
            LocalDateHelper.toLocalDate(policyDetail.expiryDateTime).trim();
        this.firstPolicyInceptionDate =
            LocalDateHelper.toLocalDate(policyDetail.inceptionDateTime).trim();

        this.hasRenewal = this.getRenewalTransaction(historyList) != null;
        let activeRenewal: PolicyTransactionResourceModel = this.getCurrentActiveRenewal(historyList);
        if (activeRenewal != null) {
            this.currentPolicyEffectiveDate =
                LocalDateHelper.toLocalDate(activeRenewal.effectiveDateTime).trim();
            this.currentPolicyEndDate = LocalDateHelper.toLocalDate(activeRenewal.expiryDateTime).trim();
        } else {
            let latestPurchased: PolicyTransactionResourceModel = this.getCurrentPurchased(historyList);
            this.currentPolicyEffectiveDate =
                LocalDateHelper.toLocalDate(latestPurchased.effectiveDateTime).trim();
            this.currentPolicyEndDate = LocalDateHelper.toLocalDate(latestPurchased.expiryDateTime).trim();
        }
    }

    private getCurrentActiveRenewal(
        historyList: Array<PolicyTransactionResourceModel>,
    ): PolicyTransactionResourceModel {
        // Note: transactions are ordered by descending creation dates.
        return historyList.find(this.currentActiveRenewalCallbackFn, historyList);
    }

    private getRenewalTransaction(historyList: Array<PolicyTransactionResourceModel>): PolicyTransactionResourceModel {
        // Note: transactions are ordered by descending creation dates.
        return historyList.find(this.renewalTransactionCallbackFn, historyList);
    }

    private getCurrentPurchased(historyList: Array<PolicyTransactionResourceModel>): PolicyTransactionResourceModel {
        // Note: transactions are ordered by descending creation dates.
        return historyList.find(this.currentPurchaseCallbackFn, historyList);
    }

    private renewalTransactionCallbackFn(value: PolicyTransactionResourceModel): boolean {
        return value.eventTypeSummary == 'Renewed';
    }

    private currentActiveRenewalCallbackFn(value: PolicyTransactionResourceModel): boolean {
        return value.eventTypeSummary == 'Renewed' && value.transactionStatus == 'Active';
    }

    private currentPurchaseCallbackFn(value: PolicyTransactionResourceModel): boolean {
        return value.eventTypeSummary == 'Purchased';
    }

    private initialisePremiumResult(): PremiumResult {
        return {
            basePremium: 0,
            esl: 0,
            premiumGst: 0,
            stampDutyAct: 0,
            stampDutyNsw: 0,
            stampDutyNt: 0,
            stampDutyQld: 0,
            stampDutySa: 0,
            stampDutyTas: 0,
            stampDutyVic: 0,
            stampDutyWa: 0,
            stampDutyTotal: 0,
            totalPremium: 0,
            terrorismPremium: 0,
            eslNsw: 0,
            eslTas: 0,
            stampDuty: 0,
            commission: 0,
            commissionGst: 0,
            brokerFee: 0,
            brokerFeeGst: 0,
            underwriterFee: 0,
            underwriterFeeGst: 0,
            roadsideAssistanceFee: 0,
            roadsideAssistanceFeeGst: 0,
            policyFee: 0,
            policyFeeGst: 0,
            partnerFee: 0,
            partnerFeeGst: 0,
            administrationFee: 0,
            administrationFeeGst: 0,
            establishmentFee: 0,
            establishmentFeeGst: 0,
            interest: 0,
            interestGst: 0,
            merchantFees: 0,
            merchantFeesGst: 0,
            transactionCosts: 0,
            transactionCostsGst: 0,
            serviceFees: 0,
            totalGST: 0,
            totalPayable: 0,
            currencyCode: '',
        };
    }
}
