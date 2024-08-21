import { Component, OnInit, Input } from "@angular/core";
import { PremiumResult } from "@app/models/premium-result";
import { AuthenticationService } from "@app/services/authentication.service";
import { PolicyHelper } from "@app/helpers";
import { CurrencyPipe } from "@app/pipes/currency.pipe";

/**
 * Renders a price breakdown for a policy.
 */
@Component({
    selector: "app-breakdown-policy-view",
    templateUrl: "./breakdown-policy-view.component.html",
})
export class BreakdownPolicyViewComponent implements OnInit {

    @Input() public refundOrPriceData: PremiumResult;
    public stampDutyTitleData: any;
    public stampDutyContentData: any = {};
    public eslTitleData: any;
    public eslContentData: any = {};
    public abs: any = Math.abs;
    public isCustomer: boolean = false;
    public isExpandable: boolean = false;
    public isRefund: boolean = false;
    public showTerrorismPremium: boolean = false;
    public showEsl: boolean = false;
    public showBrokerFee: boolean = false;
    public showCommision: boolean = false;
    public showUnderwriterFee: boolean = false;
    public showRoadsideAssistanceFee: boolean = false;
    public showPolicyFee: boolean = false;
    public showPartnerFee: boolean = false;
    public showAdministrationFee: boolean = false;
    public showEstablishmentFee: boolean = false;
    public showInterest: boolean = false;
    public showInterestGst: boolean = false;
    public showMerchantFees: boolean = false;
    public showMerchantFeesGst: boolean = false;
    public showServiceFees: boolean = false;
    public showTransactionCosts: boolean = false;
    public showTransactionCostsGst: boolean = false;
    public showTotalGst: boolean = false;
    public isMutual: boolean;

    public constructor(
        private authService: AuthenticationService,
        protected currencyFormatPipe: CurrencyPipe,
    ) { }

    public ngOnInit(): void {
        this.isRefund = this.refundOrPriceData.totalPayable < 0;
        this.refundOrPriceData = this.isRefund ?
            this.invertPremiumData(this.refundOrPriceData) : this.refundOrPriceData;
        this.isCustomer = this.authService.isCustomer();
        this.isExpandable = !this.isCustomer;
        this.showTerrorismPremium = this.refundOrPriceData.terrorismPremium !== 0;
        this.showEsl = this.refundOrPriceData.esl !== 0
            || this.refundOrPriceData.eslNsw !== 0
            || this.refundOrPriceData.eslTas !== 0;

        this.eslTitleData = {
            title: PolicyHelper.constants.Labels.Esl.Esl,
            titleText: this.currencyFormatPipe.transform(
                this.refundOrPriceData.esl,
                this.refundOrPriceData.currencyCode,
            ),
        };
        this.eslContentData = PolicyHelper.getEslData(this.refundOrPriceData, this.currencyFormatPipe);
        this.showBrokerFee = this.refundOrPriceData.brokerFee !== 0;
        this.showCommision = !this.isCustomer && this.refundOrPriceData.commission !== 0;
        this.showUnderwriterFee = this.refundOrPriceData.underwriterFee !== 0;
        this.showRoadsideAssistanceFee = this.refundOrPriceData.roadsideAssistanceFee !== 0;
        this.showPolicyFee = this.refundOrPriceData.policyFee !== 0;
        this.showPartnerFee = this.refundOrPriceData.partnerFee !== 0;
        this.showAdministrationFee = this.refundOrPriceData.administrationFee !== 0;
        this.showEstablishmentFee = this.refundOrPriceData.establishmentFee !== 0;
        this.showInterest = this.refundOrPriceData.interest !== 0;
        this.showInterestGst = this.refundOrPriceData.interestGst !== 0;
        this.showMerchantFees = this.refundOrPriceData.merchantFees !== 0;
        this.showMerchantFeesGst = this.refundOrPriceData.merchantFeesGst !== 0;
        this.showServiceFees = this.refundOrPriceData.serviceFees !== 0;
        this.showTransactionCosts = this.refundOrPriceData.transactionCosts !== 0;
        this.showTransactionCostsGst = this.refundOrPriceData.transactionCostsGst !== 0;
        this.showTotalGst = this.refundOrPriceData.totalGST !== 0;

        this.stampDutyTitleData = {
            title: PolicyHelper.constants.Labels.StampDuty.StampDuty,
            titleText: this.currencyFormatPipe.transform(
                this.refundOrPriceData.stampDutyTotal,
                this.refundOrPriceData.currencyCode,
            ),
        };
        this.stampDutyContentData = PolicyHelper.getStampDutyStateData(this.refundOrPriceData, this.currencyFormatPipe);
        this.isMutual = this.authService.isMutualTenant();
    }

    private invertPremiumData(refundOrPriceData: PremiumResult): PremiumResult {
        for (let property in refundOrPriceData) {
            if (Object.prototype.hasOwnProperty.call(refundOrPriceData, property)) {
                refundOrPriceData[property] = this.invertValue(Number(refundOrPriceData[property]));
            }
        }
        return refundOrPriceData;
    }

    private invertValue(value: number): number {
        return value <= 0 ? this.abs(value) : - this.abs(value);
    }
}
