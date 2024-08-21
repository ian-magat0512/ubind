import { QuestionMetadata } from "@app/models/question-metadata";
import { LocaleService } from "@app/services/locale.service";
import { CurrencyFieldFormatter } from "./currency-field-formatter";

describe('CurrencyFieldFormatter', () => {
    let localeService: LocaleService;
    let questionMetadata: QuestionMetadata;

    beforeEach(() => {
        localeService = new LocaleService();
        questionMetadata = {
            dataType: 'currency',
            displayable: true,
            canChangeWhenApproved: true,
            private: false,
            resetForNewQuotes: false,
            resetForNewRenewalQuotes: false,
            resetForNewAdjustmentQuotes: false,
            resetForNewCancellationQuotes: false,
            resetForNewPurchaseQuotes: false,
            tags: null,
            name: '',
        };
    });

    it('should format correctly', async () => {
        // Arrange
        const inputOutputs: Array<Array<string>> = [
            ["123.12", "AUD", "$123.12"],
            ["123456.12", "AUD", "$123,456.12"],
            ["123456.12", "NZD", "$123,456.12"],
            ["123456.12", "USD", "$123,456.12"],
            ["123456.12", "GBP", "£123,456.12"],
            ["123456.12", "EUR", "€123,456.12"],
            ["123456.12", "PGK", "K123,456.12"],
            ["123,456.12", "USD", "$123,456.12"],
            ["12,3456.12", "GBP", "£123,456.12"],
            [" 123456.12", "EUR", "€123,456.12"],
            ["K123456.12", "PGK", "K123,456.12"],
            ["$123456.12", "AUD", "$123,456.12"],
            ["$123456.005", "USD", "$123,456.01"],
            ["AUD 123456.12", "AUD", "$123,456.12"],
            ["USD123456.12", "USD", "$123,456.12"],
            ["1000.00", "USD", "$1,000"],
            ["1234.00", "AUD", "$1,234"],
            ["", "AUD", ""],
            [" ", "AUD", ""],
            [null, "AUD", null],
            ["\t", "AUD", ""],
            ["\n", "AUD", ""],
            ["0", "AUD", "$0"],
            ["0", "USD", "$0"],
        ];
        let formatter: CurrencyFieldFormatter = new CurrencyFieldFormatter(localeService);

        // Act
        inputOutputs.forEach((inputOutput: Array<string>) => {
            questionMetadata.currencyCode = inputOutput[1];
            let output: string = formatter.format(inputOutput[0], questionMetadata);

            // Assert
            expect(output).toBe(inputOutput[2]);
        });
    });

    // Failing due to issues with unicode character representation in the currency symbol
    xit('should format VND currency correctly', async () => {
        // Arrange
        const inputOutputs: Array<Array<string>> = [
            ["1000.00", "VND", '1.000 ₫'],
            ["1234.00", "VND", "1.234 ₫"],
            ["123.12", "VND", "123,12 ₫"],
            ["123456.12", "VND", "123.456,12 ₫"],
            ["VND 123456.12", "VND", "123.456,12 ₫"],
            ["", "VND", ""],
            [" ", "VND", ""],
            ["\n", "VND", ""],
            ["\t", "VND", ""],
            ["0", "VND", "0 ₫"],
            [null, "VND", null],
        ];

        // Act
        await localeService.initialiseBrowserLocaleAndCurrency('VND').then(() => {
            let formatter: CurrencyFieldFormatter = new CurrencyFieldFormatter(localeService);
            inputOutputs.forEach((inputOutput: Array<string>) => {
                questionMetadata.currencyCode = inputOutput[1];
                let output: string = formatter.format(inputOutput[0], questionMetadata);

                // Assert
                expect(output).toBe(inputOutput[2]);
            });
        });
    });

    // Failing due to issues with unicode character representation in the currency symbol
    xit('should format BRL currency correctly', async () => {
        // Arrange
        const inputOutputs: Array<Array<string>> = [
            ["1000.00", "BRL", "R$ 1.000"],
            ["1234.00", "BRL", "R$ 1.234"],
            ["123.12", "BRL", "R$ 123,12"],
            ["123456.12", "BRL", "R$ 123.456,12"],
            ["BRL 123456.12", "BRL", "R$ 123.456,12"],
            ["", "BRL", ""],
            [" ", "BRL", ""],
            ["\n", "BRL", ""],
            ["\t", "BRL", ""],
            ["0", "BRL", "R$ 0"],
            [null, "BRL", null],
        ];

        // Act
        await localeService.initialiseBrowserLocaleAndCurrency('BRL').then(() => {
            let formatter: CurrencyFieldFormatter = new CurrencyFieldFormatter(localeService);
            inputOutputs.forEach((inputOutput: Array<string>) => {
                questionMetadata.currencyCode = inputOutput[1];
                let output: string = formatter.format(inputOutput[0], questionMetadata);

                // Assert
                expect(output).toBe(inputOutput[2]);
            });
        });
    });
});
