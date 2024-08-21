/**
 * Export credit card detail class.
 * TODO: Write a better class header: credit card details functions.
 */
export class CreditCardDetails {
    private _name: string;
    private _number: string;
    private _expiryDateMMyy: string;
    private _ccv: string;
    public constructor(
        name: string,
        number: string,
        expiryDateMMyy: string,
        ccv: string,
    ) {
        this._name = this.validateRequiredField(name, "name");

        // Clean and remove any non-valid characters from the card properties in case a product developer messes up the
        // input masking, leaving _ _ _ characters which make the numbers invalid
        this._number = this.validateRequiredField(number.replace(/\D/g, ''), "number");
        // Remove non-numeric characters except slashes
        const expiryDateMMyyCleaned: string = expiryDateMMyy.replace(/[^0-9/]/g, '')
            // Remove slashes after the first one
            .replace(/(\/)(.*)/, (p1: string, p2: string) => p1 + p2.replace(/\//g, ''));
        this._expiryDateMMyy = this.validateRequiredField(expiryDateMMyyCleaned, "expiry date");
        this._ccv = this.validateRequiredField(ccv.replace(/\D/g, ''), "CCV");
    }

    public get name(): string {
        return this._name;
    }

    public get number(): string {
        return this._number;
    }

    public get expiryMonthMM(): string {
        return this._expiryDateMMyy.substr(0, 2);
    }

    public get expiryYearYY(): string {
        return this._expiryDateMMyy.substr(3, 2);
    }

    public get expiryYearYYYY(): string {
        return "20" + this._expiryDateMMyy.substr(3, 2);
    }

    public get ccv(): string {
        return this._ccv;
    }

    private validateRequiredField(value: string, fieldName: string): string {
        if (!value) throw new Error(`Credit card ${fieldName} must be specified.`);
        return value;
    }
}
