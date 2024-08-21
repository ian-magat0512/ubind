import { FormType } from "@app/models/form-type.enum";
import { QuestionMetadata } from "@app/models/question-metadata";
import { ApplicationService } from "@app/services/application.service";
import { FieldFormatter } from "./field-formatter";

/**
 * Formats attachments into a nice download link
 */
export class AttachmentFieldFormatter implements FieldFormatter {

    public constructor(private applicationService: ApplicationService) {
    }

    public format(value: string, metadata: QuestionMetadata = null): string {
        let fileProperties: Array<string> = value.split(':');
        let filename: string = fileProperties[0];
        let attachmentId: string = fileProperties[2];
        let fileContentApiUrl: string = '';
        let tenant: string = this.applicationService.tenantId ?? this.applicationService.tenantAlias;
        switch (this.applicationService.formType) {
            case FormType.Quote: {
                let quoteId: string = this.applicationService.quoteId;
                fileContentApiUrl = `/api/v1/quote/${quoteId}/attachment/${attachmentId}/content?tenant=${tenant}`;
                break;
            }
            case FormType.Claim: {
                let claimId: string = this.applicationService.claimId;
                fileContentApiUrl = `/api/v1/claim/${claimId}/attachment/${attachmentId}/content?tenant=${tenant}`;
                break;
            }
            default:
                throw new Error("Could not determine the quote type, whether it's a quote or claim.");
        }
        return `<a href="${fileContentApiUrl}">${filename}</a>`;
    }
}
