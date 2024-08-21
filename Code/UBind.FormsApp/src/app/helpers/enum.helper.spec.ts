import { EnumHelper } from './enum.helper';
import { QuoteType } from '@app/models/quote-type.enum';
import { QuoteState } from '@app/models/quote-state.enum';

describe('EnumHelper', () => {
    const quoteTypeTestCases: Array<any> = [
        { input: 'newBusiness', expectedResult: QuoteType.NewBusiness },
        { input: 'newbusiness', expectedResult: QuoteType.NewBusiness },
        { input: 'NewBusiness', expectedResult: QuoteType.NewBusiness },
        { input: 0, expectedResult: QuoteType.NewBusiness },
        { input: QuoteType.NewBusiness, expectedResult: QuoteType.NewBusiness },
    ];

    const invalidQuoteTypeTestCases: Array<any> = [
        { input: 'newBusinesses', expectedResult: null },
        { input: 5, expectedResult: null },
        { input: null, expectedResult: null },
    ];

    const quoteStateTestCases: Array<any> = [
        { input: 'endorsement', expectedResult: QuoteState.Endorsement },
        { input: 'Endorsement', expectedResult: QuoteState.Endorsement },
        { input: 3, expectedResult: QuoteState.Endorsement },
        { input: QuoteState.Endorsement, expectedResult: QuoteState.Endorsement },
    ];

    quoteTypeTestCases.forEach((testCase: any) =>{
        it(`parseOrNull should parse '${testCase.input}' into QuoteType enum with value '${testCase.expectedResult}'`,
            () => {
                // Arrange
                let valueToParse: string | number | null = testCase.input;

                // Act
                let expectedResult: any = EnumHelper.parseOrNull(QuoteType, valueToParse);

                // Assert
                expect(expectedResult).toBe(testCase.expectedResult);
            });
    });

    invalidQuoteTypeTestCases.forEach((testCase: any) =>{
        it(`parseOrNull should parse '${testCase.input}' into '${testCase.expectedResult}' if parsing has failed`,
            () => {
                // Arrange
                let valueToParse: string | number | null = testCase.input;

                // Act
                let expectedResult: any = EnumHelper.parseOrNull(QuoteType, valueToParse);

                // Assert
                expect(expectedResult).toBe(testCase.expectedResult);
            });
    });

    quoteStateTestCases.forEach((testCase: any) =>{
        it(`parseOrNull should parse '${testCase.input}' into QuoteState enum with value '${testCase.expectedResult}'`,
            () => {
                // Arrange
                let valueToParse: string | number | null = testCase.input;

                // Act
                let expectedResult: any = EnumHelper.parseOrNull(QuoteState, valueToParse);

                // Assert
                expect(expectedResult).toBe(testCase.expectedResult);
            });
    });
});
