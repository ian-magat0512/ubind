/**
 * Export validation messages class.
 */
export class ValidationMessages {
    public static errorKey: any = {
        Name: "invalidName",
        Company: "invalidCompany",
        Email: "email",
        Mobile: "invalidMobile",
        Phone: "invalidPhone",
        URL: "invalidUrl",
        Alias: "invalidAlias",
        AlphaNumericSpace: "invalidAlphaNumericSpace",
        Checkbox: "checkbox",
        MimeTye: "invalidMimeType",
        LiquidTemplate: "invalidLiquidTemplate",
        DomainName: "invalidDomainName",
        SubDomain: "invalidSubDomain",
        FutureDateInvalid: "invalidFutureDateInvalid",
        NegativeValue: "invalidNegativeValue",
        ZeroValue: "invalidZeroValue",
        Amount: "invalidAmount",
        EntityName: "invalidEntityName",
        StylesheetUrl: "invalidStylesheetUrl",
        EmailSeparatedBySemiColon: "invalidEmailSeparatedBySemiColon",
        SmtpServerPort: "invalidSmtpServerPort",
        SmtpServerHost: "invalidSmtpServerHost",
        UniqueFieldNoDefaultValue: "uniqueFieldNoDefaultValue",
        Select: "invalidSelect",
        StreetAddress: "invalidStreetAddress",
        AddressSuburb: "invalidAddressSuburb",
        AddressPostcode: "invalidAddressPostcode",
        AddressStateAndPostcode: "invalidAddressStateAndPostcode",
        CustomLabel: "invalidCustomLabel",
        Required: "required",
        DuplicateCsv: "duplicateCsv",
        InvalidCsv: "invalidCsv",
        ExceedLimitCsv: "exceedLimitCsv",
        MissingBeginCertificate: "missingBeginCertificate",
        MissingEndCertificate: "missingEndCertificate",
        Uniqueness: "uniqueness",
        InvalidJson: "invalidJson",
        JsonAssertionFailed: "jsonAssertionFailed",
        InvalidSchema: "invalidSchema",
        PositiveWholeNumber: "positiveWholeNumber",
        WholeNumber: "wholeNumber",
    };

    private static invalidInputMessageAlphaNumericWithSpace: string =
        " must only contain letters, numbers and spaces";
    private static invalidInputMessageName: string =
        " may contain alphabetic characters, numbers, spaces, hyphens, apostrophes, commas, and periods." +
        " It must not start or end in hyphen, must not be the word 'null'," +
        " and must contain at least one alphabetic character";
    private static invalidInputMessageCompanyName: string =
        "Company/organization name may only " +
        "contain letters, numbers and selected special characters e.g. '.?!&+%:$-@\\/(),";
    private static invalidInputMessageEmail: string =
        "Email must be a valid email address";
    private static invalidInputMessageMobilePhone: string =
        "Must be a valid australian mobile number " +
        "(e.g. 04 xxxx xxxx, +61 4 xxxx xxxx, 05 xxxx xxxx, or +61 5 xxxx xxxx)";
    private static invalidInputMessagePhone: string =
        "Must be a valid phone number " +
        "(e.g. 0x xxxx xxxx, or +61 x xxxx xxxx)";
    private static invalidMessageURL: string =
        " must be a valid web address";
    private static invalidInputMessageCheckBox: string =
        "You must select atleast one from these selection/s";
    private static invalidInputMessageMimeType: string =
        "Please enter a valid mime-type";
    private static invalidInputMessageliquidTemplate: string =
        "Liquid template is required";
    private static invalidInputMessageSubDomain: string =
        "must contain valid list of entries" +
        " derived from the the Domain Name for this DKIM Configuration";
    private static invalidInputMessageAlias: string =
        " may contain lowercase alphabetic characters, numbers, and hyphens." +
        " It must not start or end in hyphen, must not be the word 'null'," +
        " and must contain at least one alphabetic character";
    private static invalidInputMessageEntityName: string =
        " must start with a letter, must only contain letters, " +
        " numbers, spaces, hyphens, apostrophes, commas and period characters" +
        " It must not start or end in hyphen";
    private static invalidAmount: string =
        'Amount must contain a valid currency value';
    private static uniqueFieldShouldNoDefaultValue: string =
        'An additional property with a default value cannot also be unique';
    private static invalidDate: string =
        ' must contain a valid date value';
    private static invalidStylesheetUrl: string = " must contain a valid stylesheet URL";
    private static invalidDomain: string = " must contain a valid domain name";
    private static invalidLiquidTemplate: string = " must contain a valid Liquid Template";
    private static invalidEmail: string = " must be a valid email address";
    private static invalidEmailSeparatedBySemiColonPattern: string
        = "must contain one or more valid email addresses separated by semi-colon";
    private static invalidSmtpServerPort: string = "must contain a valid port address";
    private static invalidSmtpServerHost: string = "must contain a valid host address";
    private static invalidInputMessageSelect: string =
        "You must select at least one from these selection/s";
    private static uniqueness: string = "must be unique";
    private static invalidJson: string = "must contain a valid JSON object";
    private static jsonAssertionFailed: string = "does not pass schema assertion";
    private static invalidSchema: string = "must contain a valid JSON schema";
    private static invalidCsv: string = "must contain a valid CSV payload";
    private static positiveWholeNumber: string = "must contain a positive none zero whole number (ex. 1-9999)";

    public static getValidationMessageByType(type: string, controlName: string, errorData?: any): string {
        let description: string = controlName.toLocaleLowerCase();
        description = description.charAt(0).toLocaleUpperCase() + description.slice(1);
        switch (type) {
            case ValidationMessages.errorKey.SubDomain:
                return `${controlName} ${this.invalidInputMessageSubDomain}`;
            case ValidationMessages.errorKey.Name:
            case ValidationMessages.errorKey.EntityName:
                return controlName + " " + this.invalidInputMessageName;
            case ValidationMessages.errorKey.Company:
                return this.invalidInputMessageCompanyName;
            case ValidationMessages.errorKey.Email:
                return this.invalidInputMessageEmail;
            case ValidationMessages.errorKey.Mobile:
                return this.invalidInputMessageMobilePhone;
            case ValidationMessages.errorKey.Phone:
                return this.invalidInputMessagePhone;
            case ValidationMessages.errorKey.UniqueFieldNoDefaultValue:
                return this.uniqueFieldShouldNoDefaultValue;
            case ValidationMessages.errorKey.FutureDateInvalid:
                return `${description} must not be in the future`;
            case ValidationMessages.errorKey.NegativeValue:
            case ValidationMessages.errorKey.ZeroValue:
                return `${description} must contain a positive currency value`;
            case ValidationMessages.errorKey.Amount:
                return this.invalidAmount;
            case ValidationMessages.errorKey.URL:
                return `${controlName} ${this.invalidMessageURL}`;
            case ValidationMessages.errorKey.Alias:
                return `${controlName} ${this.invalidInputMessageAlias}`;
            case ValidationMessages.errorKey.AlphaNumericSpace:
                return `${controlName} ${this.invalidInputMessageAlphaNumericWithSpace}`;
            case ValidationMessages.errorKey.Checkbox:
                return this.invalidInputMessageCheckBox;
            case ValidationMessages.errorKey.MimeTye:
                return this.invalidInputMessageMimeType;
            case ValidationMessages.errorKey.LiquidTemplate:
                return `${controlName} ${this.invalidLiquidTemplate}`;
            case ValidationMessages.errorKey.StylesheetUrl:
                return `${controlName} ${this.invalidStylesheetUrl}`;
            case ValidationMessages.errorKey.DomainName:
                return `${controlName} ${this.invalidDomain}`;
            case ValidationMessages.errorKey.EmailSeparatedBySemiColon:
                return `${controlName} ${this.invalidEmailSeparatedBySemiColonPattern}`;
            case ValidationMessages.errorKey.SmtpServerPort:
                return `${controlName} ${this.invalidSmtpServerPort}`;
            case ValidationMessages.errorKey.SmtpServerHost:
                return `${controlName} ${this.invalidSmtpServerHost}`;
            case ValidationMessages.errorKey.Select:
                return this.invalidInputMessageSelect;
            case ValidationMessages.errorKey.MissingBeginCertificate:
                return `${controlName} needs to contain -----BEGIN CERTIFICATE-----`;
            case ValidationMessages.errorKey.MissingEndCertificate:
                return `${controlName} needs to contain -----END CERTIFICATE-----`;
            case ValidationMessages.errorKey.InvalidCsv:
                return `${controlName} ${this.invalidCsv}`;
            case ValidationMessages.errorKey.DuplicateCsv:
                return `CSV contains duplicate header`;
            case ValidationMessages.errorKey.ExceedLimitCsv:
                return `Total size must not exceed 20 MB. `
                     + `If you wish to store more than 20 MB, please disable caching.`;
            case ValidationMessages.errorKey.Uniqueness:
                return controlName + " " + this.uniqueness;
            case ValidationMessages.errorKey.InvalidJson:
                return controlName + " " + this.invalidJson;
            case ValidationMessages.errorKey.JsonAssertionFailed:
                return controlName + " " + this.jsonAssertionFailed;
            case ValidationMessages.errorKey.InvalidSchema:
                return controlName + " " + this.invalidSchema;
            case ValidationMessages.errorKey.PositiveWholeNumber:
                return `${controlName} ${this.positiveWholeNumber}`;
            case ValidationMessages.errorKey.Required:
                return `${controlName} is required`;
            default:
                return `${controlName} is invalid (${type})`;
        }
    }
}
