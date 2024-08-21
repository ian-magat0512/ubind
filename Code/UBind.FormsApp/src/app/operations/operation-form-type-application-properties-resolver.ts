import { Injectable } from "@angular/core";
import { FormType } from "@app/models/form-type.enum";
import { ApplicationService } from "@app/services/application.service";

/**
 * This is needed to work out whether the include the quoteId or the claimId in an 
 * operation payload, depending upon the Form Type.
 */
@Injectable()
export class FormTypeApplicationPropertiesResolver {

    public constructor(protected applicationService: ApplicationService) {
    }

    public getApplicationPropertyNamesForFormType(): Array<string> {
        let applicationPropertyNames: Array<string> = new Array<string>();
        if (this.applicationService.formType == FormType.Quote) {
            applicationPropertyNames.push('quoteId');
        } else if (this.applicationService.formType == FormType.Claim) {
            applicationPropertyNames.push('claimId');
        } else {
            throw new Error("Could not determine the quote type, whether it's a quote or claim.");
        }
        return applicationPropertyNames;
    }
}
