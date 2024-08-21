import { } from 'jasmine';
import { RegularExpressions } from './regular-expressions';

describe('RegularExpressions.HtmlElementTag', () => {
    it('should match element tags', () => {
        const regExp: RegExp = new RegExp(RegularExpressions.htmlElementTag);
        expect(regExp.test('<svg url="">')).toBe(true);
    });

});

describe('RegularExpressions.HtmlElementTag', () => {
    it('should match element tag inside text', () => {
        const regExp: RegExp = new RegExp(RegularExpressions.htmlElementTag);
        expect(regExp.test('foo<svg url="">bar')).toBe(true);
    });
});

describe('RegularExpressions.HtmlElementTag', () => {
    it('should match element with two element tag', () => {
        const regExp: RegExp = new RegExp(RegularExpressions.htmlElementTag);
        expect(regExp.test('<svg url=""><svg url="">')).toBe(true);
    });
});

describe('RegularExpressions.HtmlElementTag', () => {
    it('should match element tag with closing tag', () => {
        const regExp: RegExp = new RegExp(RegularExpressions.htmlElementTag);
        expect(regExp.test('<svg url=""/>')).toBe(true);
    });
});

describe('RegularExpressions.HtmlElementTag', () => {
    it('should match element tag with text inside', () => {
        const regExp: RegExp = new RegExp(RegularExpressions.htmlElementTag);
        expect(regExp.test('foo<svg url=""><bar>')).toBe(true);
    });
});

describe('RegularExpressions.HtmlElementTag', () => {
    const regExp: RegExp = new RegExp(RegularExpressions.htmlElementTag);
    const invalidElementTags: Array<string> = [
        '<svg url=""',
        'svg url="">',
        'foo',
        'foo bar',
        '<foo',
        '^foo>',
        '"foo"',
        '"foo"',
        '100%',
        '10.2',
        '10',
    ];

    invalidElementTags.forEach((invalidElementTag: any) => {
        it('should not match element tag: ' + invalidElementTag, () => {
            expect(regExp.test(invalidElementTag)).toBe(false);
        });
    });

});
