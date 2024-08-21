import { PolicyResourceModel } from "@app/resource-models/policy.resource-model";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { PolicyApiService } from "@app/services/api/policy-api.service";
import { ProblemDetails } from "@app/models/problem-details";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { LocalDateHelper } from "@app/helpers";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { Errors } from "@app/models/errors";
import { PolicyStatus } from "@app/models/policy-status.enum";
import { Injectable } from "@angular/core";
import { QuoteService } from "@app/services/quote.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { finalize } from "rxjs/operators";
import { QuoteCreateResultModel } from "@app/resource-models/quote.resource-model";
import { QuoteApiService } from "@app/services/api/quote-api.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { ErrorCodeTranslationHelper } from "@app/helpers/error-code-translation.helper";
import { ReleaseService } from "./release.service";

/**
 * Export policy service class.
 * TODO: Write a better class header: policy services functions.
 */
@Injectable({ providedIn: 'root' })
export class PolicyService {

    public constructor(
        protected sharedLoaderService: SharedLoaderService,
        private authService: AuthenticationService,
        protected navProxy: NavProxyService,
        protected layoutManager: LayoutManagerService,
        protected policyApiService: PolicyApiService,
        protected quoteApiService: QuoteApiService,
        protected userPath: UserTypePathHelper,
        protected sharedAlertService: SharedAlertService,
        protected quoteService: QuoteService,
        protected releaseService: ReleaseService,
    ) {
    }

    public async renewPolicy(policyId: string): Promise<void> {

        this.sharedAlertService.closeToast();
        await this.sharedLoaderService.present("Checking quote...");
        this.policyApiService.renewPolicy(policyId)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(
                (res: QuoteCreateResultModel) => {
                    this.navProxy.navigateForward([this.userPath.policy, policyId, 'renew', res.quoteId]);
                },
                (err: any) => {
                    if (ProblemDetails.isProblemDetailsResponse(err)) {
                        let appError: ProblemDetails = ProblemDetails.fromJson(err.error);

                        if (ErrorCodeTranslationHelper.isExpiredCancellationQuoteExistsWhenRenewing(appError.Code)
                            || ErrorCodeTranslationHelper
                                .isExpiredAdjustmentQuoteExistsWhenRenewing(appError.Code)) {
                            this.quoteService.discardQuote(appError.Data.quoteId).then(() => {
                                this.renewPolicy(policyId);
                            });
                        } else if (ErrorCodeTranslationHelper.isExpiredRenewalQuoteAlreadyExists(appError.Code)) {
                            this.handleExpiredRenewalQuoteAlreadyExists(policyId, appError);
                        } else if (ErrorCodeTranslationHelper.isRenewalQuoteAlreadyExists(appError.Code)) {
                            this.handleRenewalQuoteAlreadyExists(policyId, appError);
                        } else if (ErrorCodeTranslationHelper.isAdjustmentQuoteExistsWhenRenewing(appError.Code)) {
                            this.handleAdjustmentQuoteExistsWhenRenewing(policyId, appError);
                        } else if (ErrorCodeTranslationHelper.isCancellationQuoteExistsWhenRenewing(appError.Code)) {
                            this.handleCancellationQuoteExistsWhenRenewing(policyId, appError);
                        } else if (ErrorCodeTranslationHelper.isRenewalDisabled(appError.Code)) {
                            this.handleRenewalTransactionDisabled(policyId, appError);
                        } else if (ErrorCodeTranslationHelper.isProductReleaseNotFound(appError.Code)) {
                            this.releaseService.handleProductReleaseWasNotSet('policy',
                                'renewed',
                                appError,
                                policyId,
                                this.userPath.policy);
                        } else {
                            throw err;
                        }
                    } else {
                        throw err;
                    }
                },
            );
    }

    public async adjustPolicy(policyResourceModel: PolicyResourceModel): Promise<void> {
        this.sharedAlertService.closeToast();
        if (policyResourceModel.status === PolicyStatus.Cancelled
            || policyResourceModel.status === PolicyStatus.Expired
        ) {
            this.sharedAlertService.showWithOk(
                `${this.authService.isMutualTenant() ? 'Protection' : 'Policy'} adjustment request invalid`,
                `${this.authService.isMutualTenant() ? 'Protections' : 'Policies'} that are neither Issued `
                + `nor Active cannot be adjusted. Please create a new quote instead.`,
            );
            return;
        }
        await this.sharedLoaderService.present("Checking quote...");
        this.policyApiService.adjustPolicy(policyResourceModel.id)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(
                (res: QuoteCreateResultModel) => {
                    this.navProxy.navigateForward([
                        this.userPath.policy,
                        policyResourceModel.id,
                        'adjust', res.quoteId]);
                },
                (err: any) => {
                    if (ProblemDetails.isProblemDetailsResponse(err)) {
                        const appError: ProblemDetails = ProblemDetails.fromJson(err.error);
                        if (ErrorCodeTranslationHelper.isExpiredCancellationQuoteExistsWhenAdjusting(appError.Code)
                            || ErrorCodeTranslationHelper
                                .isExpiredRenewalQuoteExistsWhenAdjusting(appError.Code)) {
                            this.quoteService.discardQuote(appError.Data.quoteId).then(() => {
                                this.adjustPolicy(policyResourceModel);
                            });
                        } else if (ErrorCodeTranslationHelper.isExpiredAdjustmentQuoteAlreadyExists(appError.Code)) {
                            this.handleExpiredAdjustmentQuoteAlreadyExists(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper.isAdjustmentQuoteAlreadyExists(appError.Code)) {
                            this.handleAdjustmentQuoteAlreadyExists(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper.isRenewalQuoteExistsWhenAdjusting(appError.Code)) {
                            this.handleRenewalQuoteExistsWhenAdjusting(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper.isCancellationQuoteExistsWhenAdjusting(appError.Code)) {
                            this.handleCancellationQuoteExistsWhenAdjusting(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper.isAdjustmentDisabled(appError.Code)) {
                            this.handleAdjustmentTransactionDisabled(policyResourceModel.id, appError);
                        } else if (ErrorCodeTranslationHelper.isProductReleaseNotFound(appError.Code)) {
                            this.releaseService.handleProductReleaseWasNotSet('policy',
                                'adjusted',
                                appError,
                                policyResourceModel.id,
                                this.userPath.policy);
                        } else {
                            throw err;
                        }
                    } else {
                        throw err;
                    }
                },
            );
    }

    public async cancelPolicy(policyResourceModel: PolicyResourceModel): Promise<void> {
        this.sharedAlertService.closeToast();
        if (policyResourceModel.status == PolicyStatus.Cancelled
            || policyResourceModel.status == PolicyStatus.Expired
        ) {
            if (this.authService.isMutualTenant()) {
                this.sharedAlertService.showWithOk(
                    'Policy cancellation request invalid',
                    'Policies that are neither Issued nor Active cannot be cancelled. '
                    + 'Please create a new quote instead.',
                );
            } else {
                this.sharedAlertService.showWithOk(
                    'Protection cancellation request invalid',
                    'Protections that are neither Issued nor Active cannot be cancelled. '
                    + 'Please create a new quote instead.',
                );
            }

            return;
        }
        await this.sharedLoaderService.present("Checking quote...");
        this.policyApiService.cancelPolicy(policyResourceModel.id)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(
                (res: QuoteCreateResultModel) => {
                    this.navProxy.navigateForward([
                        this.userPath.policy,
                        policyResourceModel.id,
                        'cancel', (<any>res).quoteId]);
                },
                (err: any) => {
                    if (ProblemDetails.isProblemDetailsResponse(err)) {
                        let appError: ProblemDetails = ProblemDetails.fromJson(err.error);
                        if (ErrorCodeTranslationHelper.isExpiredCancellationQuoteExistsWhenCancelling(appError.Code)) {
                            this.handleExpiredCancellationQuoteExistsWhenCancelling(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper.isCancellationQuoteExistsWhenCancelling(appError.Code)) {
                            this.handleCancellationQuoteExistsWhenCancelling(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper
                            .handleExpiredAdjustmentQuoteExistsWhenCancelling(appError.Code)
                            || ErrorCodeTranslationHelper
                                .isExpiredRenewalQuoteExistsWhenCancelling(appError.Code)) {
                            this.quoteService.discardQuote(appError.Data.quoteId).then(() => {
                                this.cancelPolicy(policyResourceModel);
                            });
                        } else if (ErrorCodeTranslationHelper.isAdjustmentQuoteExistsWhenCancelling(appError.Code)) {
                            this.handleAdjustmentQuoteExistsWhenCancelling(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper.isRenewalQuoteExistsWhenCancelling(appError.Code)) {
                            this.handleRenewalQuoteExistsWhenCancelling(policyResourceModel, appError);
                        } else if (ErrorCodeTranslationHelper.isCancellationDisabled(appError.Code)) {
                            this.handleCancellationTransactionDisabled(policyResourceModel.id, appError);
                        } else if (ErrorCodeTranslationHelper.isProductReleaseNotFound(appError.Code)) {
                            this.releaseService.handleProductReleaseWasNotSet('policy',
                                'cancelled',
                                appError,
                                policyResourceModel.id,
                                this.userPath.policy);
                        } else {
                            throw err;
                        }
                    } else {
                        throw err;
                    }
                },
            );
    }

    private async handleExpiredAdjustmentQuoteAlreadyExists(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const expiredAdjustmentQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Adjustment.ExpiredAdjustmentQuoteExists(
            expiredAdjustmentQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'New',
                    handler: (): any => {
                        this.quoteService.discardQuote(expiredAdjustmentQuoteId).then(() => {
                            this.adjustPolicy(policyResourceModel);
                        });
                    },
                },
                {
                    text: 'Copy',
                    handler: (): any => {
                        this.quoteApiService.clone(expiredAdjustmentQuoteId)
                            .subscribe((res: QuoteCreateResultModel) => {
                                this.continueAdjustmentQuote(policyResourceModel.id, (<any>res).quoteId);
                            });
                    },
                },
            ],
        });
    }

    private async handleAdjustmentQuoteAlreadyExists(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingAdjustmentQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Adjustment.AdjustmentQuoteExists(
            existingAdjustmentQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'New',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingAdjustmentQuoteId).then(() => {
                            this.adjustPolicy(policyResourceModel);
                        });
                    },
                },
                {
                    text: 'Resume',
                    handler: (): any => {
                        this.continueAdjustmentQuote(policyResourceModel.id, existingAdjustmentQuoteId);
                    },
                },
            ],
        });
    }

    private async handleExpiredRenewalQuoteAlreadyExists(
        policyId: string,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const expiredRenewalQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Renewal.ExpiredRenewalQuoteExists(
            expiredRenewalQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'New',
                    handler: (): any => {
                        this.quoteService.discardQuote(expiredRenewalQuoteId).then(() => {
                            this.renewPolicy(policyId);
                        });
                    },
                },
                {
                    text: 'Copy',
                    handler: (): any => {
                        this.quoteApiService.clone(expiredRenewalQuoteId)
                            .subscribe((res: QuoteCreateResultModel) => {
                                this.continueRenewalQuote(policyId, (<any>res).quoteId);
                            });
                    },
                },
            ],
        });
    }

    private async handleRenewalQuoteAlreadyExists(
        policyId: string,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingRenewalQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Renewal.RenewalQuoteExists(
            existingRenewalQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'New',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingRenewalQuoteId).then(() => {
                            this.renewPolicy(policyId);
                        });
                    },
                },
                {
                    text: 'Resume',
                    handler: (): any => {
                        this.continueRenewalQuote(policyId, existingRenewalQuoteId);
                    },
                },
            ],
        });
    }

    private async handleRenewalTransactionDisabled(policyId: string, appError: ProblemDetails): Promise<void> {
        let productName: string = appError.Data.productName;
        const title: string = `Renewal quotes are disabled for this product`;
        const message: string = `When trying to create a renewal quote for the ${productName} product, `
        + `the attempt failed because the product settings for ${productName} prevent the creation of `
        + `renewal quotes. To resolve this issue please enable renewal quotes in the product settings `
        + `for the ${productName} product. If you need further assistance please contact technical support.`;
        this.showModalMessage(title, message, policyId);
        console.log(message);
    }

    private async handleAdjustmentTransactionDisabled(policyId: string, appError: ProblemDetails): Promise<void> {
        let productName: string = appError.Data.productName;
        const title: string = `Adjustment quotes are disabled for this product`;
        const message: string = `When trying to create an adjustment quote for the ${productName} product, `
        + `the attempt failed because the product settings for ${productName} prevent the creation of `
        + `adjustment quotes. To resolve this issue please enable adjustment quotes in the product settings `
        + `for the ${productName} product. If you need further assistance please contact technical support.`;
        this.showModalMessage(title, message, policyId);
        console.log(message);
    }

    private async handleCancellationTransactionDisabled(policyId: string, appError: ProblemDetails): Promise<void> {
        let productName: string = appError.Data.productName;
        const title: string = `Cancellation quotes are disabled for this product`;
        const message: string = `When trying to create a cancellation quote for the ${productName} product, `
        + `the attempt failed because the product settings for ${productName} prevent the creation of `
        + `cancellation quotes. To resolve this issue please enable cancellation quotes in the product settings `
        + `for the ${productName} product. If you need further assistance please contact technical support.`;
        this.showModalMessage(title, message, policyId);
        console.log(message);
    }

    private showModalMessage(title: string, message: string, policyId: string): void {
        this.sharedAlertService.showWithActionHandler({
            header: title,
            subHeader: message,
            buttons: [
                {
                    text: 'OK', handler: (): void => {
                        this.navProxy.navigateBack([this.userPath.policy, policyId]);
                    },
                },
            ],
        });
    }

    private async handleCancellationQuoteExistsWhenRenewing(
        policyId: string,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingCancellationQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Renewal.CancellationQuoteExists(
            existingCancellationQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'Discard',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingCancellationQuoteId).then(() => {
                            this.renewPolicy(policyId);
                        });
                    },
                },
            ],
        });
    }

    private async handleAdjustmentQuoteExistsWhenRenewing(
        policyId: string,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingAdjustmentQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Renewal.AdjustmentQuoteExists(
            existingAdjustmentQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'Discard',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingAdjustmentQuoteId).then(() => {
                            this.renewPolicy(policyId);
                        });
                    },
                },
            ],
        });
    }

    private async handleRenewalQuoteExistsWhenAdjusting(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingRenewalQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Adjustment.RenewalQuoteExists(
            existingRenewalQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'Discard',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingRenewalQuoteId).then(() => {
                            this.adjustPolicy(policyResourceModel);
                        });
                    },
                },
            ],
        });
    }

    private async handleRenewalQuoteExistsWhenCancelling(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingRenewalQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Cancellation.RenewalQuoteExists(
            existingRenewalQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'Discard',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingRenewalQuoteId).then(() => {
                            this.cancelPolicy(policyResourceModel);
                        });
                    },
                },
            ],
        });
    }

    private async handleAdjustmentQuoteExistsWhenCancelling(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingAdjustmentQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Cancellation.AdjustmentQuoteExists(
            existingAdjustmentQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'Discard',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingAdjustmentQuoteId).then(() => {
                            this.cancelPolicy(policyResourceModel);
                        });
                    },
                },
            ],
        });
    }

    private async handleCancellationQuoteExistsWhenAdjusting(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingCancellationQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Adjustment.CancellationQuoteExists(
            existingCancellationQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'Discard',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingCancellationQuoteId).then(() => {
                            this.adjustPolicy(policyResourceModel);
                        });
                    },
                },
            ],
        });
    }

    private async handleExpiredCancellationQuoteExistsWhenCancelling(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const expiredCancellationQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Cancellation.ExpiredCancellationQuoteExists(
            expiredCancellationQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'New',
                    handler: (): any => {
                        this.quoteService.discardQuote(expiredCancellationQuoteId).then(() => {
                            this.cancelPolicy(policyResourceModel);
                        });
                    },
                }, {
                    text: 'Copy',
                    handler: (): any => {
                        this.quoteApiService.clone(expiredCancellationQuoteId)
                            .subscribe((res: QuoteCreateResultModel) => {
                                this.continueCancellationQuote(policyResourceModel.id, (<any>res).quoteId);
                            });
                    },
                },
            ],
        });
    }

    private async handleCancellationQuoteExistsWhenCancelling(
        policyResourceModel: PolicyResourceModel,
        appError: ProblemDetails,
    ): Promise<void> {
        const localDate: string = LocalDateHelper.toLocalDate(appError.Data.createdDateTime);
        const localTime: string = LocalDateHelper.convertToLocalAndGetTimeOnly(appError.Data.createdDateTime);
        const existingCancellationQuoteId: any = appError.Data.quoteId;
        let localError: ProblemDetails = <ProblemDetails>Errors.Policy.Cancellation.CancellationQuoteExists(
            existingCancellationQuoteId,
            localDate,
            localTime,
            appError.Data.policyNumber,
        );
        this.sharedAlertService.showWithActionHandler({
            header: this.replacePolicyWithProtectionKeyword(localError.Title),
            message: this.replacePolicyWithProtectionKeyword(localError.Detail),
            cssClass: "flex-direction-row",
            buttons: [
                { text: 'Cancel' },
                {
                    text: 'New',
                    handler: (): any => {
                        this.quoteService.discardQuote(existingCancellationQuoteId).then(() => {
                            this.cancelPolicy(policyResourceModel);
                        });
                    },
                },
                {
                    text: 'Resume',
                    handler: (): any => {
                        this.continueCancellationQuote(policyResourceModel.id, existingCancellationQuoteId);
                    },
                },
            ],
        });
    }

    private continueAdjustmentQuote(policyId: string, quoteId: string): void {
        this.sharedAlertService.closeToast();
        this.navProxy.navigateForward([this.userPath.policy, policyId, 'adjust', quoteId]);
    }

    private continueRenewalQuote(policyId: string, quoteId: string): void {
        this.sharedAlertService.closeToast();
        this.navProxy.navigateForward([this.userPath.policy, policyId, 'renew', quoteId]);
    }

    private continueCancellationQuote(policyId: string, quoteId: string): void {
        this.sharedAlertService.closeToast();
        this.navProxy.navigateForward([this.userPath.policy, policyId, 'cancel', quoteId]);
    }

    private replacePolicyWithProtectionKeyword(textWithKeywords: string): string {
        let replacementText: string = textWithKeywords;

        if (this.authService.isMutualTenant()) {
            replacementText = textWithKeywords
                .replace(/policy/g, 'protection')
                .replace(/Policy/g, 'Protection')
                .replace(/policies/g, 'protections')
                .replace(/Policies/g, 'Protections');
        }

        return replacementText;
    }
}
