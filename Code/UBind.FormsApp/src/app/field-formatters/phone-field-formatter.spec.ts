import { PhoneFieldFormatter } from "./phone-field-formatter";

describe('PhoneFieldFormatter', () => {

    it('should format correctly', () => {
        const inputOutputs: Array<Array<string>> = [
            ["0400123 123", "0400 123 123"],
            ["+613880-11234", "+61 3 8801 1234"],
            ["+6140-0123123", "+61 400 123 123"],
            ["039(123) 1234", "(03) 9123 1234"],
            ["1800 123123", "1800 123 123"],
            ["1300123123", "1300 123 123"],
            ["139876", "13 98 76"],
            ["", ""],
            [" ", ""],
            [null, null],
            ["\t", ""],
            ["\n", ""],
        ];

        let formatter: PhoneFieldFormatter = new PhoneFieldFormatter();

        inputOutputs.forEach((inputOutput: Array<string>) => {
            let output: string = formatter.format(inputOutput[0], null);
            expect(output).toBe(inputOutput[1]);
        });
    });
});
