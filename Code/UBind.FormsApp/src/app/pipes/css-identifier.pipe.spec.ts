import { } from "jasmine";
import { CssIdentifierPipe } from './css-identifier.pipe';

let currencyPipe: CssIdentifierPipe = new CssIdentifierPipe();

describe('CssIdentifierPipe.transform', () => {
    it('should convert a fieldpath to a valid css identifier', () => {
        expect(currencyPipe.transform('claims[0].amount')).toEqual('claims0-amount');
    });
    it('should not allow it to start with a digit, or a hypen followed by a digit ', () => {
        expect(currencyPipe.transform('123abc')).toEqual('_-123abc');
    });
    it('should not allow it to start with a two hypens', () => {
        expect(currencyPipe.transform('--123abc')).toEqual('_-123abc');
    });
});
