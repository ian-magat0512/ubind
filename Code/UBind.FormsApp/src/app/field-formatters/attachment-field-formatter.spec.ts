import { FormType } from "@app/models/form-type.enum";
import { AttachmentFieldFormatter } from "./attachment-field-formatter";

describe('AttachmentFieldFormatter', () => {

    let applicationServiceStub: any = {
        quoteId: 'abcd-1234',
        formType: FormType.Quote,
        tenantAlias: "test-tenant",
    };

    it('should format correctly', () => {
        const inputOutputs: Array<Array<string>> = [
            [
                "avatar.png:image/png:ffff-1111:230:230",
                `<a href="/api/v1/quote/${applicationServiceStub.quoteId}/attachment/ffff-1111/content`
                    + `?tenant=${applicationServiceStub.tenantAlias}">avatar.png</a>`,
            ],
        ];

        let formatter: AttachmentFieldFormatter = new AttachmentFieldFormatter(applicationServiceStub);

        inputOutputs.forEach((inputOutput: Array<string>) => {
            let output: string = formatter.format(inputOutput[0], null);
            expect(output).toBe(inputOutput[1]);
        });
    });
});
