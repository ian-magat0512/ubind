/* eslint-disable @typescript-eslint/naming-convention */
import { ProblemDetails } from '@app/models/problem-details';
import { FieldType } from './field-type.enum';

/**
 * Export error class.
 * TODO: Write a better class header: error model.
 * @dynamic
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
                + "If you can provide a screenshot or video recording, and details about your device or "
                + "browser that would help us to resolve this quickly.",
                500,
                description != null ? new Array<string>(description) : null),
            NotImplemented: (description: string = ''): ProblemDetails => new ProblemDetails(
                'not.implemented',
                "You're a forward thinker",
                "The operation or resource you've tried to access is not implemented at this time. "
                + description
                + "If you would like to access this feature please get in touch with support.",
                500),
            NotFound: (entityName: string, id: string): ProblemDetails => new ProblemDetails(
                "record.not.found",
                `We couldn't find that ${entityName}`,
                `When trying to find a ${entityName} with the ID "${id}", nothing came up. `
                + `Please check you've entered the correct details. `
                + `If you think this is bug, please contact customer support.`,
                404),
            InvalidEnumValue: (
                propertyName: string,
                attemptedValue: any,
                $enum: object,
                context: string,
            ): ProblemDetails => new ProblemDetails(
                'invalid.enum.value',
                "An invalid value was set",
                `An attempt was made to set the property "${propertyName}", to the value "${attemptedValue}", `
                + `however that was not one of the allowed values. This is a product configuration issue which a `
                + `product developer needs to fix.`,
                400,
                [
                    "Allowed values: " + Object.values($enum).join(", "),
                    "Context: " + context,
                ]),
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
            AccessDenied: (resourceName: string | null, id: any | null): ProblemDetails => new ProblemDetails(
                'access.denied',
                'You\'re not allowed access to that',
                'You have tried to access ' + (resourceName == null ?
                    'a resource' : `the resource ${resourceName}` + (id != null ? ` with id ${id}` : ''))
                + ' without the necessary permissions. If you think you should have access to this resource, '
                + 'please ask your administrator to grant you access.',
                403),
            Forbidden: (action: string | null, reason: string | null): ProblemDetails => new ProblemDetails(
                "action.forbidden",
                "You're not allowed to do that",
                "You can't " + (action == null ? ' do that' : action)
                + ", because " + (reason == null ? "you don't have the required permissions" : reason)
                + ". If you believe this is a mistake, or you would like assistance, please contact customer support.",
                403),
        };
    }

    public static get Product(): ProductErrors {
        return {
            Configuration: (
                message: string,
                additionalDetails: Array<string> = null): ProblemDetails => new ProblemDetails(
                'product.configuration.error',
                'This product needs work',
                message,
                400,
                additionalDetails),
            FieldOptionDataInvalid: (
                fieldPath: string,
                additionalDetails: Array<string>): ProblemDetails => new ProblemDetails(
                'product.field.option.data.invalid',
                'Field option data invalid',
                `When retrieving the options for the field "${fieldPath}", the option data `
                    + 'returned from the API request did not match the required format. Please ensure '
                    + 'your API endpoint is configured to output options in the correct format. '
                    + 'If you are unsure of the correct format, please review the uBind developer documentation.',
                400,
                additionalDetails),
            NoOptionsDefined: (fieldPath: string, fieldType: FieldType): ProblemDetails => new ProblemDetails(
                'product.no.options.defined',
                'You need to define some options for this field',
                `When trying to render a ${fieldType} field with fieldPath "${fieldPath}", there were no options `
                + `defined. It's ok not to define options if the field is supposed to get options from an API, `
                + `however in this case there was no configuration for a selectedOptionRequest. Please review the `
                + `configuration and either provide static options, or the configuration details to retreive them `
                + `from an API. If you need assistance, please get in touch with support.`,
                412,
                null,
                {
                    fieldType: fieldType,
                    fieldPath: fieldPath,
                }),
            OptionMissingLabel: (fieldPath: string,
                fieldType: FieldType, optionName: string, optionValue: string): ProblemDetails => new ProblemDetails(
                'product.no.option.label.defined',
                'You need to define a label for each and every option',
                `When trying to render a ${fieldType} field with fieldPath "${fieldPath}", we came across the `
                    + `option ${optionName} with value "${optionValue}" `
                    + `which didn't have a label. This is a product `
                    + `misconfiguration which a product developer needs to fix. `
                    + `Even if you intend to just display icons, you still need a label `
                    + `so that visually impaired users can have a screen reader tell them the option. `
                    + `If you are unsure how to resolve this, please get in touch with support.`,
                412,
                null,
                {
                    fieldType: fieldType,
                    fieldPath: fieldPath,
                    optionName: optionName,
                    optionValue: optionValue,
                }),
            JsonPointerHashSymbolNotSupportedWhenResolvingRelativeFieldPath:
                (fieldPath: string, jsonPointer: string): ProblemDetails => new ProblemDetails(
                    'product.misconfiguration.json.pointer.token.hash.symbol.'
                    + 'not.supported.when.resolving.relative.field.path',
                    'You can\'t use "#" when resolving a relative field path',
                    `When trying to apply the Relative JSON Pointer "${jsonPointer}" to the fieldPath "${fieldPath}, `
                    + `we found that you have used the # symbol, `
                    + `however we don't support that. Since we're applying the `
                    + `JSON Pointer to a fieldPath and not an object, there is no need to distinguish between `
                    + `property names and values, since we only considering property names in the path. Please `
                    + `remove the use of the # symbol. This is a product configuration issue, and so further work `
                    + `on the product is needed to resolve this issue. If you are unsure what this means, please `
                    + `get in touch with support.`,
                    400,
                    null,
                    {
                        fieldPath: fieldPath,
                        jsonPointer: jsonPointer,
                    }),
            RelativeJsonPointerIntegerPrefixIsNotANumber: (
                fieldPath: string,
                jsonPointer: string,
                jsonPointerPrefixSegment: string): ProblemDetails => new ProblemDetails(
                'product.misconfiguration.relative.json.pointer.integer.prefix.is.not.a.number',
                'The first part of a Relative JSON Pointer needs to be an integer',
                `When trying to apply the Relative JSON Pointer "${jsonPointer}" to the fieldPath "${fieldPath}, `
                    + `we found that the first part of the Relative JSON Pointer "${jsonPointerPrefixSegment}" was not `
                    + `an integer, and it needs to be an integer. `
                    + `This is a product configuration issue, and so further work on the product is needed to `
                    + `resolve this issue. If you are unsure what this means, please get in touch with support.`,
                400,
                null,
                {
                    fieldPath: fieldPath,
                    jsonPointer: jsonPointer,
                    jsonPointerPrefixSegment: jsonPointerPrefixSegment,
                }),
            InsufficientDataForPremiumCalculation: (): ProblemDetails => new ProblemDetails(
                'product.misconfiguration.insufficient.data.for.calculation',
                `We couldn't calculate a premium with that data`,
                'A calculation request was triggered prematurely, because there was insufficient data collected in '
                + 'order to calculate a premium. This is a product configuration issue which a product developer '
                + 'needs to fix. Please check that fields which are required for calculation are marked as required, '
                + 'and question sets which are required for calculation are designated as such in the workflow '
                + 'configuration.',
                412),
            CouldNotScrollToInvalidFieldInQuestionSet: (actionName: string, questionSetPaths: Array<string>,
            ): ProblemDetails => new ProblemDetails(
                'product.misconfiguration.could.not.scroll.to.field.in.question.set',
                `We couldn't find that invalid field or question set`,
                `When trying to perform the action "${actionName}", it was specified that certain `
                    + `question sets were required to be valid, however one of the question sets was not `
                    + `found, or if it was, no invalid field was found. The likely cause of this is a product `
                    + `misconfiguration, where the workflow action specifies question sets that are not actually `
                    + `rendered in "requiresValidQuestionSets". Either remove the un-rendered question sets from `
                    + `the list, or use "requiresValidOrHiddenQuestionSets" instead.`,
                400,
                [ `Question Sets: ${questionSetPaths.join(', ')}` ],
                questionSetPaths),
            ActionNotFound: (name: string): ProblemDetails => new ProblemDetails(
                'product.misconfiguration.action.not.found',
                `We couldn't find a workflow action with the name \"${name}\"`,
                `When trying to render an action widget with the name \"${name}\", there was no such `
                + 'action widget found for the current workflow step. '
                + `This is a product misconfiguration, which a product developer needs to fix. `
                + 'If you would like to report this or you need assistance, '
                + 'please get in touch with support.',
                400),
        };
    }

    public static get Expression(): ExpressionErrors {
        return {
            EvaluationFailed: (
                message: string,
                additionalDetails: Array<string>,
                data: any = null): ProblemDetails => new ProblemDetails(
                'product.misconfiguration.expression.evaluation.failed',
                `Expression evaluation failed`,
                'When trying to evaluate an expression, the following error occured: '
                    + (message.trim().endsWith('.') ? message.trim() + ' ' : message.trim() + '. ')
                    + 'This is a product misconfiguration which a product developer needs to fix. '
                    + 'Please check your expression carefully against the documentation. '
                    + 'If you would like to report this or you need assistance, '
                    + 'please get in touch with support.',
                400,
                additionalDetails,
                data),
            NoSuchExpressionMethod: (
                methodName: string,
                expressionSource: string,
                debugIdentifier: string): ProblemDetails => new ProblemDetails(
                'product.misconfiguration.no.such.expression.method',
                `That expression method doesn't exist`,
                `When trying to parse the expression identified by "${debugIdentifier}, we found that the `
                    + `expression method "${methodName}" was specified, however no such expression method exists. `
                    + `This is a product misconfiguration, which a product developer needs to fix. `
                    + `Please check if this is a mis-spelling and refer to the documented list of available expression `
                    + `methods to ensure you've entered it correctly. `
                    + `If you need assistance or would like to report this `
                    + ` to us, please contact customer support.`,
                400,
                [`Expression source: "${expressionSource}"`],
                {
                    methodName: methodName,
                    expressionSource: expressionSource,
                    debugIdentifier: debugIdentifier,
                }),
            GetRelativeFieldValueMismatchedArgument: (
                fieldPathArgument: string,
                expressionSource: string,
                debugIdentifier: string): ProblemDetails => new ProblemDetails(
                'product.misconfiguration.get.relative.field.value.mismatched.argument',
                `The expression argument was not found`,
                `When trying to parse the expression method identified by "${debugIdentifier}", we found a call to `
                    + `getRelativeFieldValue(...) where the first parameter was "${fieldPathArgument}, however a fixed `
                    + ` value for that parameter was not passed in to the expression as an argument. `
                    + `This is a product misconfiguration, which a product developer needs to fix.`
                    + `The first parameter must refer to a fixed argument which represents the fieldPath of the `
                    + `current field, and is typically called "fieldPath". If you need assistance or would like to `
                    + `report this, please contact customer support.`,
                400,
                [`Expression source: "${expressionSource}`],
                {
                    fieldPathArgument: fieldPathArgument,
                    expressionSource: expressionSource,
                    debugIdentifier: debugIdentifier,
                }),
        };
    }

    public static get Injection(): InjectionErrors {
        return {
            MissingParameter: (parameterName: string): ProblemDetails => new ProblemDetails(
                'injection.missing.parameter',
                'A required parameter is missing',
                `When trying to load this form, the parameter "${parameterName} was missing. `
                + ` We can't continue since this is a required parameter. Please ensure you include `
                + ` a value for ${parameterName} when configuring this form for injection into a page. `
                + ` If you need assistance, please don't hesitate to get in touch with support.`,
                412,
                null,
                {
                    parameterName: parameterName,
                }),
        };
    }

    public static get IQumulate(): IQumulateErrors {
        return {
            RequiredDataAbsentFromQuoteRequest: (responseDescription: string): ProblemDetails => new ProblemDetails(
                'iqumulate.required.data.missing.from.quote.request',
                'Required data missing from IQumulate quote request',
                'When trying to setup IQumulate premium funding, some data which is required was found to be missing. '
                + 'This is a product configuration issue which a product developer needs to fix. We would appreciate '
                + 'if you could report the details of this to customer support so we can diagnose the issue. '
                + 'We apologise for the inconvenience.',
                412,
                [`IQumulate response: ${responseDescription}`]),
            CreditCardProcessingError: (responseDescription: string): ProblemDetails => new ProblemDetails(
                'iqumulate.bambora.credit.card.processing.error',
                'IQumulate credit card processing error',
                'When trying to setup IQumulate premium funding, there was an issue trying to process your credit '
                + 'card. Please check your card details and try again. If this problem persists, please check '
                + 'with your bank to ensure there are no issues with your card.',
                412,
                [`IQumulate response: ${responseDescription}`]),
            UnknownResponseCode: (responseDescription: string): ProblemDetails => new ProblemDetails(
                'iqumulate.unknown.response.code',
                'IQumulate return an unknown response code',
                'When trying to setup IQumulate premium funding, an unknown response code was returned. '
                + 'This may indicate that IQumulate have updated their API. We would appreciate you reporting '
                + 'this to our customer support team. We apologise for the inconvenience.',
                406,
                [`IQumulate response: ${responseDescription}`]),
            NoDocumentsGenerated: (responseDescription: string): ProblemDetails => new ProblemDetails(
                'iqumulate.no.documents.generated',
                'IQumulate did not generate any documents',
                'During the processing of your premium funding application, IQumulate did not generate and return '
                + 'any documents. Something went wrong with your application. Please report '
                + 'this to our customer support team. We apologise for the inconvenience.',
                412,
                [`IQumulate response: ${responseDescription}`]),
        };
    }

    public static get Payment(): PaymentErrors {
        return {
            ConfigurationMissing: (): ProblemDetails => new ProblemDetails(
                'payment.configuration.missing',
                'Payment configuration missing',
                + `We could not find any payment configuration. Please ensure you have defined a payment.json `
                + `file with the relevant payment configuration settings. `
                + 'This is a product configuration issue which a product developer needs to fix. We would appreciate '
                + 'if you could report the details of this to customer support so we can diagnose the issue. '
                + 'We apologise for the inconvenience.',
                412,
            ),
            EnvironmentConfigurationMissing: (
                environment: string,
                providerName: string,
                settingName: string): ProblemDetails => new ProblemDetails
            (
                'payment.environment.configuration.missing',
                'Payment environment configuration missing',
                `When trying to process payment using the ${providerName} payment provider, `
                + `we could not find the configuration setting "${settingName}" for the ${environment} environment. `
                + `Please ensure you have defined settings for this environment, or a "default" setting for all `
                + `environments. `
                + 'This is a product configuration issue which a product developer needs to fix. We would appreciate '
                + 'if you could report the details of this to customer support so we can diagnose the issue. '
                + 'We apologise for the inconvenience.',
                412,
            ),
            ProviderError: (providerName: string, reason: string): ProblemDetails => new ProblemDetails
            (
                'payment.provider.error.on.front.end',
                'We couldn\'t process your payment',
                `We ran into an issue processing your payment with ${providerName}. `
                + `Please check your details and try again. If the problem persists, please `
                + `get in touch with your bank.`,
                402,
                [ reason ],
            ),
            PremiumFundingProposalMissing: (paymentMethod: string): ProblemDetails => new ProblemDetails(
                'premium.funding.proposal.missing',
                'Premium funding proposal missing',
                `When trying to process payment using the method "${paymentMethod}", there was no `
                + `premium funding proposal found. This is usually an indication the premium funding has not been `
                + `correctly configured for this product.`
                + 'This is a product configuration issue which a product developer needs to fix. We would appreciate '
                + 'if you could report the details of this to customer support so we can diagnose the issue. '
                + 'We apologise for the inconvenience.',
                412,
            ),
        };
    }

    public static get Attachments(): AttachmentErrors {
        return {
            InvalidResponseObtained: (fileName: string): ProblemDetails  => new ProblemDetails(
                'attachments.received.invalid.response',
                'Received an invalid response from the server',
                `We received an invalid response from the server when trying to upload ${fileName}. Please try `
                + 'again. If you need assistance, please contact support.',
                400,
            ),
            InvalidFileData: (): ProblemDetails => new ProblemDetails(
                'attachment.field.value.does.not.contain.file.data',
                'The attachment field value doesn\'t contain file data',
                `When attempting to parse the file data from an attachment field value, valid file data `
                + `was not detected.`
                + `Please ensure the field value is not empty, and you have specified an attachment field.`,
                400,
            ),
        };
    }

    public static get Configuration(): ConfigurationErrors {
        return {
            Version1NotSupported: (): ProblemDetails => new ProblemDetails(
                'configuration.v1.no.longer.supported',
                'Product configuration no longer supported',
                'The product configuration received was either invalid, or in the format of Version 1. '
                + 'Version 1 configuration is no longer supported. Please synchronise the product to generate '
                + 'a new configuration in the latest format.',
                409,
            ),
        };
    }

    public static get Workflow(): WorkflowErrors {
        return {
            StepNotFound: (stepName: string): ProblemDetails => new ProblemDetails(
                'workflow.step.not.found',
                'Workflow step not found',
                `When trying to transition to the workflow step "${stepName}", it was not found. `
                + 'This might happen if a workflow step was renamed, but '
                + 'one of the action navigation buttons was not updated to match. '
                + 'This is a product configuration issue which a product developer needs to fix. We would appreciate '
                + 'if you could report the details of this to customer support so we can have it fixed. '
                + 'We apologise for the inconvenience.',
                412,
            ),

            StartingDestinationHidden: (stepName: string): ProblemDetails => new ProblemDetails(
                'working.starting.destination.hidden',
                'Workflow starting destination hidden',
                `When trying to start the form editing session, the step "${stepName}" was found to be the `
                + 'starting step however there are either no articles or article elements visible within '
                + 'that step, so we are unable to start the form editing session. '
                + 'This is a product configuration issue which a product developer needs to fix. We would appreciate '
                + 'if you could report the details of this to customer support so we can have it fixed. '
                + 'We apologise for the inconvenience.',
                412,
            ),
        };
    }

    public static get Quote(): QuoteErrors {
        return {
            QuoteTypeMismatch: (
                quoteId: string,
                expectedQuoteType: string,
                actualQuoteType: string,
            ): ProblemDetails => new ProblemDetails(
                'quote.type.mismatch',
                'Quote type mismatch',
                `When trying to load the quote with ID "${quoteId}, it was expected to be a `
                + `"${expectedQuoteType}" quote, however it was actually a "${actualQuoteType}" quote. `
                + `Please get in touch with customer support. We apologise for the inconvenience.`,
                412,
            ),

            ApplicationModeModifyWithoutExistingQuoteOrPolicyAndQuoteType: (
                mode: string,
            ): ProblemDetails => new ProblemDetails(
                'application.mode.modify.without.existing.quote.or.policy',
                'You\'ve asked to make changes but not specified to which quote',
                `You've asked to make changes to a quote by specifying the application mode "${mode}", `
                + `however you have not specified a quote ID, or both a policy ID and quoteType. `
                + `If you want to make changes to a quote, you must either provide the quote ID, `
                + `or provide both the policy ID and the quote type.`,
                412,
            ),

            PolicyModificationRequestedWithoutPolicyId: (quoteType: string): ProblemDetails => new ProblemDetails(
                'policy.modification.requested.without.policy.id',
                'You need to provide the Policy ID',
                `You've asked to make changes to a policy by specifying the quote type "${quoteType}", `
                + `however you have not specified the ID of the policy to create this quote against. `
                + `If you want to make changes to a policy, you must specify which policy.`,
                412,
            ),

            PolicyModificationRequestedWithoutQuoteType: (policyId: string): ProblemDetails => new ProblemDetails(
                'policy.modification.requested.without.quote.type',
                'You need to provide a quote type',
                `You've asked to make changes to the policy with ID "${policyId}" `
                + `however you have not specified the the quote type for the policy transaction. `
                + `If you want to start a new quote against a policy, you need to specify the quote type.`,
                412,
            ),

            PolicyModificationRequestedInvalidQuoteType: (
                policyId: string,
                quoteType: string,
            ): ProblemDetails => new ProblemDetails(
                'policy.modification.requested.invalid.quote.type',
                'You need to provide a valid quote type',
                `You've asked to make changes to the policy with ID "${policyId}" `
                + `however you have specified the quote type "${quoteType}" for the policy transaction, `
                + `which is not a quote type that is valid for making changes to an existing policy.`,
                412,
            ),
        };
    }

    public static get Claim(): ClaimErrors {
        return {
            ApplicationModeModifyWithoutExistingClaim: (mode: string): ProblemDetails => new ProblemDetails(
                'application.mode.modify.without.existing.claim',
                'You\'ve asked to make changes but not specified to which claim',
                `You've asked to make changes to a claim by specifying the application mode "${mode}", `
                + `however you have not specified a claim ID. If you want to make changes to a claim, you `
                + `must provide the claim ID.`,
                412,
            ),
        };
    }
}

export interface GeneralErrors {
    Unexpected: (description: string) => ProblemDetails;
    NotImplemented: (description: string) => ProblemDetails;
    NotFound: (entityName: string, id: string) => ProblemDetails;
    InvalidEnumValue: (
        propertyName: string,
        attemptedValue: any,
        $enum: object,
        context: string) => ProblemDetails;
}

export interface UserErrors {
    Login: UserLoginErrors;
    AccessDenied: (resourceName: string | null, id?: any | null) => ProblemDetails;
    Forbidden: (action: string | null, reason?: string | null) => ProblemDetails;
}

export interface UserLoginErrors {
    Required: () => ProblemDetails;
}

export interface ProductErrors {
    Configuration: (
        message: string,
        additionalDetails?: Array<string>) => ProblemDetails;
    FieldOptionDataInvalid: (
        fieldPath: string,
        additionalDetails: Array<string>) => ProblemDetails;
    NoOptionsDefined: (fieldPath: string, fieldType: FieldType) => ProblemDetails;
    OptionMissingLabel: (fieldPath: string,
        fieldType: FieldType, optionName: string, optionValue: string) => ProblemDetails;
    JsonPointerHashSymbolNotSupportedWhenResolvingRelativeFieldPath: (
        fieldPath: string, jsonPointer: string) => ProblemDetails;
    RelativeJsonPointerIntegerPrefixIsNotANumber: (
        fieldPath: string,
        jsonPointer: string,
        jsonPointerPrefixSegment: string) => ProblemDetails;
    InsufficientDataForPremiumCalculation: () => ProblemDetails;
    CouldNotScrollToInvalidFieldInQuestionSet: (
        actionName: string,
        questionSetPaths: Array<string>) => ProblemDetails;
    ActionNotFound: (name: string) => ProblemDetails;
}

export interface ExpressionErrors {
    EvaluationFailed: (
        message: string,
        additionalDetails: Array<string>,
        data: any) => ProblemDetails;
    NoSuchExpressionMethod: (
        methodName: string,
        expressionSource: string,
        debugIdentifier: string) => ProblemDetails;
    GetRelativeFieldValueMismatchedArgument: (
        fieldPathArgument: string,
        expressionSource: string,
        debugIdentifier: string) => ProblemDetails;
}

export interface InjectionErrors {
    MissingParameter: (parameterName: string) => ProblemDetails;
}

export interface IQumulateErrors {
    RequiredDataAbsentFromQuoteRequest: (responseDescription: string) => ProblemDetails;
    CreditCardProcessingError: (responseDescription: string) => ProblemDetails;
    UnknownResponseCode: (responseDescription: string) => ProblemDetails;
    NoDocumentsGenerated: (responseDescription: string) => ProblemDetails;
}

export interface PaymentErrors {
    ConfigurationMissing: () => ProblemDetails;
    EnvironmentConfigurationMissing: (
        environment: string,
        providerName: string,
        settingName: string) => ProblemDetails;
    ProviderError: (providerName: string, reason: string) => ProblemDetails;
    PremiumFundingProposalMissing: (paymentMethod: string) => ProblemDetails;
}

export interface AttachmentErrors {
    InvalidResponseObtained: (fileName: string) => ProblemDetails;
    InvalidFileData: () => ProblemDetails;
}

export interface ConfigurationErrors {
    Version1NotSupported: () => ProblemDetails;
}

export interface WorkflowErrors {
    StepNotFound: (stepName: string) => ProblemDetails;
    StartingDestinationHidden: (stepName: string) => ProblemDetails;
}

export interface QuoteErrors {
    QuoteTypeMismatch: (
        quoteId: string,
        expectedQuoteType: string,
        actualQuoteType: string) => ProblemDetails;
    ApplicationModeModifyWithoutExistingQuoteOrPolicyAndQuoteType: (
        mode: string) => ProblemDetails;
    PolicyModificationRequestedWithoutPolicyId: (quoteType: string) => ProblemDetails;
    PolicyModificationRequestedWithoutQuoteType: (policyId: string) => ProblemDetails;
    PolicyModificationRequestedInvalidQuoteType: (
        policyId: string,
        quoteType: string) => ProblemDetails;
}

export interface ClaimErrors {
    ApplicationModeModifyWithoutExistingClaim: (mode: string) => ProblemDetails;
}
