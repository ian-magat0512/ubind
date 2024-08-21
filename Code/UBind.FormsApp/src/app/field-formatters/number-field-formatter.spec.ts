import { QuestionMetadata } from "@app/models/question-metadata";
import { LocaleService } from "@app/services/locale.service";
import { NumberFieldFormatter } from "./number-field-formatter";

describe('NumberFieldFormatter', () => {
    let localeService: LocaleService;
    let formatter: NumberFieldFormatter;
    let questionMetadata: QuestionMetadata;

    beforeEach(() => {
        localeService = new LocaleService();
        formatter = new NumberFieldFormatter(localeService);
        questionMetadata = {
            dataType: 'number',
            displayable: true,
            canChangeWhenApproved: true,
            private: false,
            resetForNewQuotes: false,
            resetForNewRenewalQuotes: false,
            resetForNewAdjustmentQuotes: false,
            resetForNewCancellationQuotes: false,
            resetForNewPurchaseQuotes: false,
            tags: null,
            currencyCode: null,
            name: '',
        };
    });

    it('should format comma thousand separators correctly', () => {
        // Arrange
        const inputOutputs: Array<Array<string>> = [
            ["123123", "123,123"],
            ["123123.45", "123,123.45"],
            ["12,3123.45", "123,123.45"],
            [".45", "0.45"],
            ["1000", "1,000"],
            ["1000.001", "1,000.001"],
            ["1000.5", "1,000.5"],
            ["1000.123456789", "1,000.123456789"],
            ["1000.00", "1,000"],
            ["", ""],
            [" ", ""],
            [null, null],
            ["\t", ""],
            ["\n", ""],
        ];

        // Act
        inputOutputs.forEach((inputOutput: Array<string>) => {
            let output: string = formatter.format(inputOutput[0], questionMetadata);

            // Assert
            expect(output).toBe(inputOutput[1]);
        });
    }, 10000);

    // Disabled test, to be fixed on UB-10997
    xit('should format period thousand separators correctly', async () => {
        // Arrange
        const inputOutputs: Array<Array<string>> = [
            ["123123", "123.123"],
            ["123123.45", "123.123,45"],
            ["12,3123.45", "123.123,45"],
            [".45", "0,45"],
            ["1000", "1.000"],
            ["1000.001", "1.000,001"],
            ["1000.5", "1.000,5"],
            ["1000.123456789", "1.000,123456789"],
            ["1000.00", "1.000"],
            ["", ""],
            [" ", ""],
            [null, null],
            ["\t", ""],
            ["\n", ""],
        ];

        // Act
        await localeService.initialiseBrowserLocaleAndCurrency('VND').then(() => {
            inputOutputs.forEach((inputOutput: Array<string>) => {
                let output: string = formatter.format(inputOutput[0], questionMetadata);

                // Assert
                expect(output).toBe(inputOutput[1]);
            });
        });
    }, 10000);
});
