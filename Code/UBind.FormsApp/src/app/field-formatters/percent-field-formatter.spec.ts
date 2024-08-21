import { QuestionMetadata } from "@app/models/question-metadata";
import { LocaleService } from "@app/services/locale.service";
import { PercentFieldFormatter } from "./percent-field-formatter";

describe('PercentFieldFormatter', () => {

    it('should format correctly', () => {
        const inputOutputs: Array<Array<string>> = [
            ["100%", "100%"],
            ["100", "100%"],
            ["99.5", "99.5%"],
            [".45", "0.45%"],
            ["1000", "1,000%"],
            ["1000.44%", "1,000.44%"],
            ["", ""],
            [" ", ""],
            [null, null],
            ["\t", ""],
            ["\n", ""],
        ];

        let formatter: PercentFieldFormatter = new PercentFieldFormatter(new LocaleService());

        inputOutputs.forEach((inputOutput: Array<string>) => {
            let questionMetadata: QuestionMetadata = {
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
            let output: string = formatter.format(inputOutput[0], questionMetadata);
            expect(output).toBe(inputOutput[1]);
        });
    });
});
