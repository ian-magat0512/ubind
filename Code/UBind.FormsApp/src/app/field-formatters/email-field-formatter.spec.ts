import { EmailFieldFormatter } from "./email-field-formatter";

describe('EmailFieldFormatter', () => {

    it('should format correctly', () => {
        const inputOutputs: Array<Array<string>> = [
            ["John.Doe@email.com", "john.doe@email.com"],
            ["Johnny145@EMAIL.com", "johnny145@email.com"],
            [" asdf@asdf.com ", "asdf@asdf.com"],
            ["", ""],
            [" ", ""],
            [null, null],
            ["\t", ""],
            ["\n", ""],
        ];

        let formatter: EmailFieldFormatter = new EmailFieldFormatter();

        inputOutputs.forEach((inputOutput: Array<string>) => {
            let output: string = formatter.format(inputOutput[0], null);
            expect(output).toBe(inputOutput[1]);
        });
    });
});
