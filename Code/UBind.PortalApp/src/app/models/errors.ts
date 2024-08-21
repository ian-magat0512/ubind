/* eslint-disable @typescript-eslint/naming-convention */
import { ProblemDetails } from '@app/models/problem-details';

/**
 * This class is a central place for all errors that can be generated within the application.
 */
export class Errors {
    public static get General(): GeneralErrors {
        return {
            Unexpected: (description: string = null): ProblemDetails => new ProblemDetails(
                "error.unexpected",
                "Something went wrong",
                "Something went wrong, and unfortunately this an unexpected situation. "
                + "We apologise for the inconvenience. "
                + "We would appreciate it if you would contact customer support and "
                + "provide a description of the steps you took to uncover this issue. "
                + "If you can provide a screenshot or video recording, and "
                + "details about your device or browser that would help us to resolve this quickly.",
                500,
                description != null ? new Array<string>(description) : null),
            NotImplemented: (description: string = null): ProblemDetails => new ProblemDetails(
                'not.implemented',
                "You're a forward thinker",
                "The operation or resource you've tried to access is not implemented at this time. "
                + description
                + "If you would like to access this feature please get in touch with support.",
                500),
            NotFound: (entityName: string, id: string): ProblemDetails => new ProblemDetails(
                "record.not.found",
                `We couldn't find that ${entityName}`,
                `When trying to find a ${entityName} with the ID "${id}", ` +
                `nothing came up. Please check you've entered the correct details. ` +
                `If you think this is bug, please contact customer support.`,
                404),
            UnexpectedEnumValue: (
                enumName: string,
                enumValue: string,
                operationDescription: string,
            ): ProblemDetails => new ProblemDetails(
                "error.unexpected.enum.value",
                "Unexpected enum value was discovered.",
                `When trying to ${operationDescription}, `
                + `When trying to ${operationDescription}, we were looking at an Enum called ${enumName}, `
                + `however it's value "${enumValue}" was not one of the values we could process, `
                + `so this is unexpected. This is something that our team will need to fix. `
                + `We apologise for the inconvenience. `
                + `Please report this to our support staff, and in the mean time, select a different option`,
                412,
            ),
        };
    }

    public static get User(): UserErrors {
        return {
            Login: {
                Required: (): ProblemDetails => new ProblemDetails(
                    'user.login.required',
                    'Please login',
                    'You are required to login to access this resource.',
                    401),
            },
            AccessDenied: (resourceName?: string | null, id?: any | null): ProblemDetails => new ProblemDetails(
                'access.denied',
                'You\'re not allowed access to that',
                'You have tried to access ' + (resourceName == null ? 'a resource' :
                    `the resource ${resourceName}` + (id != null ? ` with id ${id}` : ''))
                + ' without the necessary permissions. If you think you should have access '
                + 'to this resource, please ask your administrator to grant you access.',
                403),
            Forbidden: (action: string | null, reason?: string | null): ProblemDetails => new ProblemDetails(
                "action.forbidden",
                "You're not allowed to do that",
                "You can't " + (action == null ? ' do that' : action) + ", " +
                "because " + (reason == null ? "you don't have the required permissions" : reason) + ". " +
                "If you believe this is a mistake, or you would like assistance, please contact customer support.",
                403),
            DataAccessPermissionMissing: (): ProblemDetails => new ProblemDetails(
                'data.access.permission.missing',
                'Data access permission missing',
                'Your user account does not have data access permission to any of the environments '
                + '(development, staging or production) and can therefore not access any data in the portal. '
                + 'Please contact your systems administrator for assistance.',
                403),
            CannotAccessAnyEnvironment: (): ProblemDetails => new ProblemDetails(
                'user.cannot.access.any.environment',
                'Your user account hasn\'t been configured properly',
                `You do not have permission to access any data environments. `
                + `In order to be able to log in, you must have access to at least one data environment. `
                + `Typically, you would have access to the Production data environment. Please contact your systems `
                + `administrator and ask to have the relevant role assigned to your user account.`,
                403),
            WrongPortal: (): ProblemDetails => new ProblemDetails(
                'user.wrong.portal',
                'You cannot access this portal',
                `You are trying to access an agent portal, but your user account does not have access to this portal. `
                + 'Please get in touch with customer support to ensure your account is configured with a customer '
                + 'portal so that you can log in to the correct location.',
                403),
            BrowserStorageNotWorking: (): ProblemDetails => new ProblemDetails(
                'login.browser.storage.not.working',
                'We couldn\'t persist your login information',
                'We logged you in but we could not persist your login information, which means you '
                + 'won\'t be able to access the portal. Please check that your browser allows storing '
                + 'information in browser storage.',
                500),
            CannotAccessAnotherOrganisation: (anotherOrganisationAlias: string): ProblemDetails => new ProblemDetails(
                'user.cannot.access.another.organisation',
                'You don\'t have access to that organisation',
                `You were trying to access the portal of the organisation ${anotherOrganisationAlias}, but you `
                 + `are not a member of that organisation. Please check that you are accessing the correct portal `
                 + `URL. If you think this is a mistake, please contact support.`,
                403),
            AgentCannotAccessCustomerPortal: (): ProblemDetails => new ProblemDetails(
                'user.agent.cannot.access.customer.portal',
                'Your user account can\'t login to a customer portal',
                "You've attempted to access a customer portal, however your user account is "
                + "an agent user account. Please ensure you login to the correct agent portal. "
                + "If you need guidance on where to log in, please get in touch with support.",
                403),
            NoPortal: (): ProblemDetails => new ProblemDetails(
                'user.no.portal',
                'We couldn\'t find a portal for you to access',
                'Your user account doesn\'t have a portal assigned to it, and there was no default portal '
                + 'configured for you to access. Please contact your systems administrator for assistance.',
                403),
        };
    }

    public static get Customer(): CustomerErrors {
        return {
            CreateUserAccount: {
                MissingEmailAddress: (): ProblemDetails => new ProblemDetails(
                    "customer.no.email.address",
                    "Email address required",
                    "This customer doesn't have an email address. "
                    + "Before creating a user account for a customer, "
                    + "you have to enter an email address.",
                    409,
                ),
            },
        };
    }

    public static get Policy(): PolicyErrors {
        return {
            Renewal: {
                ExpiredRenewalQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    "policy.expired.renewal.quote.already.exists",
                    "Copy answers from expired renewal quote",
                    `A renewal quote created for this policy 
                        ${this.getCreatedDateOrTimeString(localDate, localTime)} has now expired. ` +
                        "Would you like to start a new renewal quote from scratch, or would you "
                        + "like to copy the question answers from the expired quote? "
                        + "Both actions will result in the expired quote being discarded",
                    409,
                    null,
                    { quoteId: quoteId },
                ),
                RenewalQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    "policy.renewal.quote.already.exists",
                    "Resume existing policy renewal quote",
                    `A renewal quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "Would you like to resume the existing renewal quote, or start a new renewal quote?",
                    409,
                    null,
                    { quoteId: quoteId },
                ),
                AdjustmentQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    'policy.adjustment.quote.exists.when.renewing',
                    'Discard policy adjustment quote in progress',
                    `An adjustment quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "A renewal cannot be performed while an adjustment is still in progress. "
                        + "By starting a renewal for this policy, the incomplete policy adjustment "
                        + "will therefore be discarded and can no longer be resumed. Would you like to proceed?",
                    409,
                    null,
                    { quoteId: quoteId },
                ),
                CancellationQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    'policy.cancellation.quote.exists.when.renewing',
                    'Discard policy cancellation quote in progress',
                    `A cancellation quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "A renewal cannot be performed while a cancellation is still in progress. "
                        + "By starting a renewal for this policy, the incomplete policy cancellation "
                        + "will therefore be discarded and can no longer be resumed. Would you like to proceed?",
                    409,
                    null,
                    { quoteId: quoteId }),
                NoCustomerToSendRenewalTo: (): ProblemDetails => new ProblemDetails(
                    'policy.cannot.send.renewal.no.customer',
                    'No customer to send renewal to',
                    "We cannot send a renewal invitation for this policy because "
                            + "there is no customer associated with the policy. "
                            + "Please create the customer and associate it with the policy first.",
                    412),
            },
            Adjustment: {
                ExpiredAdjustmentQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    'policy.expired.adjustment.quote.already.exists',
                    'Copy answers from expired adjustment quote',
                    `An adjustment quote created for this policy `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)} has now expired. `
                        + "Would you like to start a new adjustment quote from scratch, "
                        + "or would you like to copy the question answers from the expired quote? "
                        + "Both actions will result in the expired quote being discarded. ",
                    409,
                    null,
                    { quoteId: quoteId },
                ),
                AdjustmentQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    'policy.adjustment.quote.already.exists',
                    'Resume existing policy adjustment quote',
                    `An adjustment quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "Would you like to resume the existing adjustment quote, or start a new adjustment quote?",
                    409,
                    null,
                    { quoteId: quoteId },
                ),
                RenewalQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    "policy.renewal.quote.exists.when.adjusting",
                    "Discard policy renewal quote in progress",
                    `A renewal quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "An adjustment cannot be performed while a renewal is still in progress. "
                        + "By adjusting this policy, the incomplete policy renewal will therefore "
                        + "be discarded and can no longer be resumed. Would you like to proceed?",
                    409,
                    null,
                    { quoteId: quoteId }),
                CancellationQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    "policy.cancellation.quote.exists.when.adjusting",
                    "Discard policy cancellation quote in progress",
                    `A cancellation quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "By adjusting this policy, the incomplete policy cancellation will therefore "
                        + "be discarded and can no longer be resumed. Would you like to proceed?",
                    409,
                    null,
                    { quoteId: quoteId }),
            },
            Cancellation: {
                RenewalQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    "policy.renewal.quote.already.exists",
                    "Discard policy renewal quote in progress",
                    `A renewal quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "A cancellation cannot be performed while a renewal is still in progress. "
                        + "By starting a cancellation for this policy, the incomplete policy renewal "
                        + "will therefore be discarded and can no longer be resumed. Would you like to proceed?",
                    409,
                    null,
                    { quoteId: quoteId },
                ),
                AdjustmentQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    'policy.adjustment.quote.exists.when.renewing',
                    'Discard policy adjustment quote in progress',
                    `An adjustment quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "A cancellation cannot be performed while an adjustment is still in progress. "
                        + "By starting a cancellation for this policy, the incomplete policy adjustment will "
                        + "therefore be discarded and can no longer be resumed. Would you like to proceed?",
                    409,
                    null,
                    { quoteId: quoteId }),
                ExpiredCancellationQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    'policy.expired.cancellation.quote.exists.when.renewing',
                    'Copy answers from expired cancellation quote',
                    `A cancellation quote created for this policy `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)} has now expired. `
                        + "Would you like to start a new cancellation quote from scratch, "
                        + "or would you like to copy the question answers from the expired quote? "
                        + "Both actions will result in the expired quote being discarded.",
                    409,
                    null,
                    { quoteId: quoteId }),
                CancellationQuoteExists: (
                    quoteId: string,
                    localDate: string,
                    localTime: string,
                    policyNumber: string,
                ): ProblemDetails => new ProblemDetails(
                    'policy.cancellation.quote.exists.when.renewing',
                    'Resume existing policy cancellation quote',
                    `A cancellation quote for the policy ${policyNumber} was started `
                        + `${this.getCreatedDateOrTimeString(localDate, localTime)}, but never completed. `
                        + "Would you like to resume the existing policy cancellation or start a new one?",
                    409,
                    null,
                    { quoteId: quoteId }),
            },
        };
    }

    public static get Release(): ReleaseErrors {
        return {
            CreationFailed: (errors: Array<string>): ProblemDetails => new ProblemDetails(
                'release.creation.failed',
                'Release creation failed',
                "Unfortunately we couldn't initialise the release because there were one or more errors.",
                400,
                errors),
        };
    }

    public static get Popover(): PopoverErrors {
        return {
            UnknownAction: (command: any, context: string): ProblemDetails => new ProblemDetails(
                'portal.popover.action.unknown',
                'That action is unknown',
                "When attempting to process the command for a popover action, it was not one that is known to the "
                + "system. Popover actions can be hard coded, or may be supplemented using a portal page trigger. ",
                500,
                [ `Context: ${context}` ],
                command),
        };
    }

    public static get Claim(): ClaimErrors {
        return {
            CreationAgainstPolicyFailed: (productId: string, environment: string): ProblemDetails => new ProblemDetails(
                'claim.creation.against.policy.failed',
                `Claim creation against policy failed`,
                `We are unable to create the policy due to `
                + `the inability to locate the claim component `
                + `for the product ${productId} in the ${environment} environment.`,
                404,
            ),
        };
    }

    public static getCreatedDateOrTimeString(createdDate: string, createdTime: string): string {
        const date: Date = new Date(createdDate);
        const today: Date = new Date();

        const isCreatedToday: boolean =
            date.getDate() == today.getDate() &&
            date.getMonth() == today.getMonth() &&
            date.getFullYear() == today.getFullYear();

        if (isCreatedToday) {
            return "at " + createdTime;
        } else {
            return "on " + createdDate;
        }
    }
}

export interface GeneralErrors {
    Unexpected: (description: string) => ProblemDetails;
    NotImplemented: (description: string) => ProblemDetails;
    NotFound: (entityName: string, id: string) => ProblemDetails;
    UnexpectedEnumValue: (
        enumName: string,
        enumValue: string,
        operationDescription: string) => ProblemDetails;
}

export interface UserErrors {
    Login: UserLoginErrors;
    AccessDenied: (resourceName?: string | null, id?: any | null) => ProblemDetails;
    Forbidden: (action: string | null, reason?: string | null) => ProblemDetails;
    DataAccessPermissionMissing: () => ProblemDetails;
    CannotAccessAnyEnvironment: () => ProblemDetails;
    WrongPortal: () => ProblemDetails;
    BrowserStorageNotWorking: () => ProblemDetails;
    CannotAccessAnotherOrganisation: (anotherOrganisationAlias: string) => ProblemDetails;
    AgentCannotAccessCustomerPortal: () => ProblemDetails;
    NoPortal: () => ProblemDetails;
}

export interface UserLoginErrors {
    Required: () => ProblemDetails;
}

export interface CustomerErrors {
    CreateUserAccount: CustomerCreateUserAccountErrors;
}

export interface CustomerCreateUserAccountErrors {
    MissingEmailAddress: () => ProblemDetails;
}

export interface PolicyErrors {
    Renewal: PolicyRenewalErrors;
    Adjustment: PolicyAdjustmentErrors;
    Cancellation: PolicyCancellationErrors;
}

export interface PolicyRenewalErrors {
    ExpiredRenewalQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    RenewalQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    AdjustmentQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    CancellationQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    NoCustomerToSendRenewalTo: () => ProblemDetails;
}

export interface PolicyAdjustmentErrors {
    ExpiredAdjustmentQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    AdjustmentQuoteExists: (quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    RenewalQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    CancellationQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
}

export interface PolicyCancellationErrors {
    RenewalQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    AdjustmentQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    ExpiredCancellationQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
    CancellationQuoteExists: (
        quoteId: string,
        localDate: string,
        localTime: string,
        policyNumber: string) => ProblemDetails;
}

export interface ReleaseErrors {
    CreationFailed: (errors: Array<string>) => ProblemDetails;
}

export interface PopoverErrors {
    UnknownAction: (command: any, context: string) => ProblemDetails;
}

export interface ClaimErrors {
    CreationAgainstPolicyFailed: (productId: string, environment: string) => ProblemDetails;
}
