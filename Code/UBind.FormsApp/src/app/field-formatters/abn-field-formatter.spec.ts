import { AbnFieldFormatter } from "./abn-field-formatter";

describe('AbnFieldFormatter', () => {

    it('should format correctly', () => {
        const inputOutputs: Array<Array<string>> = [
            ["12312312311", "12 312 312 311"],
            ["1 231 231 2311", "12 312 312 311"],
            ["1 2 3 1 2 3 1 2 3 1 1", "12 312 312 311"],
            ["1 2 3 123 1 2 3 1 1", "12 312 312 311"],
            ["11-222-333-444", "11 222 333 444"],
            ["01234567890", "01 234 567 890"],
            ["01234567890 ", "01 234 567 890"],
            ["", ""],
            [" ", ""],
            [null, null],
            ["\t", ""],
            ["\n", ""],
        ];

        let formatter: AbnFieldFormatter = new AbnFieldFormatter();

        inputOutputs.forEach((inputOutput: Array<string>) => {
            let output: string = formatter.format(inputOutput[0], null);
            expect(output).toBe(inputOutput[1]);
        });
    });
});
