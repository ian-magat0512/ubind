import { } from "jasmine";
import { CurrencyPipe } from './currency.pipe';

let currencyPipe: CurrencyPipe = new CurrencyPipe();

describe('CurrencyPipe.transform', () => {
    it('should transform numeric string to currency', () => {
        expect(currencyPipe.transform('123')).toEqual('123');
        expect(currencyPipe.transform('123,456')).toEqual('123,456');
        expect(currencyPipe.transform('123.123')).toEqual('123.123');
        expect(currencyPipe.transform('1234567.89')).toEqual('1,234,567.89');
        expect(currencyPipe.transform('  123  ')).toEqual('123');
        expect(currencyPipe.transform('')).toEqual('');
        expect(currencyPipe.transform('.999')).toEqual('0.999');
        expect(currencyPipe.transform('.0005')).toEqual('0.0005');
        expect(currencyPipe.transform('1234.0005')).toEqual('1,234.0005');
        expect(currencyPipe.transform('0')).toEqual('0');
    });
    it('should not transform non-numeric string', () => {
        expect(currencyPipe.transform('123abc')).toEqual('123abc');
        expect(currencyPipe.transform('abc')).toEqual('abc');
        expect(currencyPipe.transform('abc123')).toEqual('abc123');
        expect(currencyPipe.transform('123.45.67')).toEqual('123.45.67');
        expect(currencyPipe.transform('.999')).toEqual('0.999');
        expect(currencyPipe.transform('213.0005abc')).toEqual('213.0005abc');
    });
});

describe('CurrencyPipe.restore', () => {
    it('should restore currency string to numeric', () => {
        expect(currencyPipe.restore('123')).toEqual('123');
        expect(currencyPipe.restore('123,456.00')).toEqual('123456');
        expect(currencyPipe.restore('123.123')).toEqual('123.123');
        expect(currencyPipe.restore('1,234,567.89')).toEqual('1234567.89');
        expect(currencyPipe.restore('123.00')).toEqual('123');
        expect(currencyPipe.restore('.999')).toEqual('0.999');
        expect(currencyPipe.restore('0.0')).toEqual('0');
        expect(currencyPipe.restore('1230.0005')).toEqual('1230.0005');
        expect(currencyPipe.restore('1230.00050')).toEqual('1230.00050');
        expect(currencyPipe.restore('123,230.0005')).toEqual('123230.0005');
    });
    it('should not restore non-numeric string', () => {
        expect(currencyPipe.restore('123abc')).toEqual('123abc');
        expect(currencyPipe.restore('abc')).toEqual('abc');
        expect(currencyPipe.restore('abc123')).toEqual('abc123');
        expect(currencyPipe.restore('123.45.67')).toEqual('123.45.67');
        expect(currencyPipe.restore('')).toEqual('');
        expect(currencyPipe.restore('1230.0005abc')).toEqual('1230.0005abc');
    });
});
