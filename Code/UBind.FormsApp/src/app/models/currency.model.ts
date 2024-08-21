/**
 * Export Currency
 * Used to hold integer currency name and fractional currency name.
 * @dynamic
 */
export class Currency {
    private _integerCurrency: string;
    private _fractionalCurrency: string;
    public constructor(integerCurrency: string, fractionalCurrency: string) {
        this._integerCurrency = integerCurrency;
        this._fractionalCurrency = fractionalCurrency;
    }

    public get integerCurrency(): string {
        return this._integerCurrency;
    }

    public get fractionalCurrency(): string {
        return this._fractionalCurrency;
    }
}
