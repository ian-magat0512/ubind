import { Injectable } from "@angular/core";
import { Errors } from "@app/models/errors";
import { ApplicationService } from "./application.service";
import { ConfigService } from "./config.service";

/**
 * Provides services needed for processing payments
 */
@Injectable({
    providedIn: 'root',
})
export class PaymentService {

    private paymentCardBin: string;
    private paymentCardNumberLength: number;
    private cardPaymentOptions: Array<string> = ['VISA', 'MASTERCARD', 'AMEX', 'DINERS CLUB', 'DIRECT DEBIT'];
    private fundingPaymentOptions: Array<string> = ['PREMIUM FUNDING'];

    public constructor(
        private configService: ConfigService,
        private applicationService: ApplicationService,
    ) {
    }

    public hasPaymentFormConfiguration(): boolean {
        return this.configService.settings.paymentForm != null;
    }

    public getPublicApiKey(): string {
        if (!this.configService.settings.paymentForm) {
            throw Errors.Payment.ConfigurationMissing();
        }
        let environment: string = this.applicationService.environment;
        let publicApiKey: string;
        let hasEnvironmentConfiguration: boolean = this.configService.settings?.paymentForm?.[environment] != null;
        let hasDefaultConfiguration: boolean = this.configService.settings?.paymentForm?.['default'] != null;
        if (hasEnvironmentConfiguration) {
            publicApiKey = this.configService.settings.paymentForm[environment]['publicApiKey'];
        }
        if (publicApiKey == null && hasDefaultConfiguration) {
            publicApiKey = this.configService.settings.paymentForm['default']['publicApiKey'];
        }
        return publicApiKey;
    }

    /**
     * Generates the BIN for a payment card, which is just the first 6 - 8 digits
     * @param paymentCardNumber the full payment card number
     * Stores the BIN as a string, or null if it cannot be generated for any reason
     * This will not generate any errors.
     * To retreive the BIN, call getPaymentCardBin();
     */
    public generatePaymentCardBin(paymentCardNumber: string): void {
        if (!paymentCardNumber) {
            this.paymentCardBin = null;
        } else {
            let cardNumberTrimmed: string = this.trimCardNumber(paymentCardNumber);
            if (!isNaN(<any>cardNumberTrimmed) && cardNumberTrimmed.length > 5) {
                this.paymentCardBin = cardNumberTrimmed.substring(0, 8);
            }
        }
    }

    public getPaymentCardBin(): string | null {
        return this.paymentCardBin;
    }

    /*
    * Sets the length of the payment card number
    * To retreive the length, call getPaymentCardNumberLength();
    */
    public determinePaymentCardNumberLength(paymentCardNumber: string): void {
        if (paymentCardNumber) {
            let cardNumberTrimmed: string = this.trimCardNumber(paymentCardNumber);
            this.paymentCardNumberLength = cardNumberTrimmed.length;
        } else {
            this.paymentCardNumberLength = null;
        }
    }

    public getPaymentCardNumberLength(): number | null {
        return this.paymentCardNumberLength;
    }

    public isCardPayment(paymentMethod: string): boolean {
        if (!paymentMethod) {
            return false;
        }
        return this.cardPaymentOptions.indexOf(paymentMethod.toUpperCase()) > -1;
    }

    public isFundingPayment(paymentMethod?: string): boolean {
        if (!paymentMethod) {
            return false;
        }
        return this.fundingPaymentOptions.indexOf(paymentMethod.toUpperCase()) > -1;
    }

    private trimCardNumber(paymentCardNumber: string): string {
        return paymentCardNumber.replace(/[\s\.\,]/g, '');
    }
}
