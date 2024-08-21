import { BooleanFieldFormatter } from "./boolean-field-formatter";

describe('BooleanFieldFormatter', () => {

    it('should format correctly', () => {
        const inputOutputs: Array<Array<string>> = [
            ["True", "Yes"],
            ["false", "No"],
            ["yes", "Yes"],
            ["No", "No"],
            ["True ", "Yes"],
            [" false", "No"],
            ["yes ", "Yes"],
            [" No", "No"],
            [" ", ""],
            ["", ""],
            [null, null],
            ["\t", ""],
            ["\n", ""],
        ];

        let formatter: BooleanFieldFormatter = new BooleanFieldFormatter();

        inputOutputs.forEach((inputOutput: Array<string>) => {
            let output: string = formatter.format(inputOutput[0], null);
            expect(output).toBe(inputOutput[1]);
        });
    });
});
