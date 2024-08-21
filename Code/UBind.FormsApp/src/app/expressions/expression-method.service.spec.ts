import { ExpressionMethodService } from "./expression-method.service";
import { UserService } from "@app/services/user.service";
import { ApplicationService } from "@app/services/application.service";
import { ResumeApplicationService } from "@app/services/resume-application.service";
import { ResilientStorage } from '@app/storage/resilient-storage';
import { SessionDataManager } from '@app/storage/session-data-manager';
import { storageHelper } from '@app/helpers/storage.helper';
import { ExpressionInputSubjectService } from './expression-input-subject.service';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { FormService } from "@app/services/form.service";
import { QuestionsWidget } from "@app/components/widgets/questions/questions.widget";
import { ConfigService } from "@app/services/config.service";
import { MatchingFieldsSubjectService } from "./matching-fields-subject.service";
import { EventService } from "@app/services/event.service";
import { TriggerDisplayConfig } from "@app/models/trigger-display-config";
import { TriggerService } from "@app/services/trigger.service";
import { FieldFormatterResolver } from "@app/field-formatters/field-formatter-resolver";
import { FieldMetadataService } from "@app/services/field-metadata.service";
import { FakeConfigServiceForMetadata } from "@app/services/test/fake-config-service-for-metadata";
import { FormType } from "@app/models/form-type.enum";
import { QuoteState } from "@app/models/quote-state.enum";
import { CalculationState } from "@app/models/calculation-result-state";
import { SourceRatingSummaryItem } from "@app/models/source-rating-summary-item";
import { Errors } from "@app/models/errors";
import { UnifiedFormModelService } from "@app/services/unified-form-model.service";
import { PaymentService } from "@app/services/payment.service";
import { TextCase } from "@app/models/text-case.enum";
import { RevealGroupTrackingService } from "@app/services/reveal-group-tracking.service";
import { ContextEntityService } from "@app/services/context-entity.service";
import { FakeContextEntityService } from "@app/services/fakes/fake-context-entity.service";
import { AttachmentFileProperties } from "@app/models/attachment-file-properties";
import { AttachmentService } from "@app/services/attachment.service";
import { OperationInstructionService } from "@app/services/operation-instruction.service";
import { FakeOperationFactory } from "@app/operations/fakes/fake-operation-factory";
import { OperationFactory } from "@app/operations/operation.factory";
import { OperationInstruction } from "@app/models/operation-instruction";
import { fakeAsync, tick } from "@angular/core/testing";
import { OperationStatusService } from "@app/services/operation-status.service";
import { LocaleService } from "@app/services/locale.service";
import { UserType } from "@app/models/user-type.enum";
import { LoggerService } from "@app/services/logger.service";
// import { LoggerService } from "@app/services/logger.service";

/**
 * Fake ConfigService which provides question metadata
 */

describe('ExpressionMethodService', () => {
    let service: ExpressionMethodService;
    let formService: FormService;
    let expressionInputSubjectService: ExpressionInputSubjectService;
    let resumeApplicationService: ResumeApplicationService;
    let workflowStatusService: WorkflowStatusService;
    let applicationService: ApplicationService;
    let eventService: EventService;
    let matchingFieldsSubjectService: MatchingFieldsSubjectService;
    let fieldMetadataService: FieldMetadataService;
    let configService: ConfigService;
    let configServiceFake: FakeConfigServiceForMetadata;
    let fieldFormatterResolver: FieldFormatterResolver;
    let unifiedFormModelService: UnifiedFormModelService;
    let paymentService: PaymentService;
    let fieldGroupTrackingService: RevealGroupTrackingService;
    let contextEntityService: ContextEntityService;
    let operationInstructionService: OperationInstructionService;
    let operationStatusService: OperationStatusService;
    let localeService: LocaleService;

    beforeEach(() => {
        configServiceFake = new FakeConfigServiceForMetadata();
        contextEntityService = <ContextEntityService><any>new FakeContextEntityService();
        localeService = new LocaleService();
        configService = <ConfigService><any>configServiceFake;
        eventService = new EventService();
        expressionInputSubjectService = new ExpressionInputSubjectService();
        applicationService = new ApplicationService();
        (<any>applicationService)._formType = FormType.Quote;
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'test-tenant',
            'test-tenant',
            'test-organisation',
            'test-organisation',
            false,
            'test-productId',
            'test-product',
            'production',
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "2.3.6");
        resumeApplicationService = new ResumeApplicationService(
            new ResilientStorage(),
            new SessionDataManager(),
            applicationService);
        workflowStatusService = new WorkflowStatusService();
        matchingFieldsSubjectService = new MatchingFieldsSubjectService(eventService, expressionInputSubjectService);
        fieldMetadataService = new FieldMetadataService(configService, eventService, expressionInputSubjectService);
        fieldFormatterResolver = new FieldFormatterResolver(applicationService, localeService);
        applicationService.latestQuoteResult = <any>{
            quoteState: QuoteState.Incomplete,
            calculationState: CalculationState.Incomplete,
            ratingSummaryItems: new Array<SourceRatingSummaryItem>(),
        };
        unifiedFormModelService
            = new UnifiedFormModelService(configService, expressionInputSubjectService, eventService);
        paymentService = new PaymentService(configService, applicationService);
        formService = new FormService(
            configService, null, null, unifiedFormModelService, null, null, paymentService, eventService);
        fieldGroupTrackingService = new RevealGroupTrackingService(eventService);
    });

    it('userHasPermission should return true if permission argument is in permission list', () => {
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.tenantAlias) {
                    return "test-tenant";
                }

                if (key == storageHelper.user.permissions) {
                    return JSON.parse("[\"viewMyAccount\", \"editMyAccount\", \"canDoThis\"]");
                }
            },
        };

        let userService: UserService = new UserService(session as SessionDataManager, applicationService);
        userService.retrieveLoggedInUserData();
        service = new ExpressionMethodService(null, null, null, null, null, userService,
            resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.userHasPermission('canDoThis')).toBe(true);
    });

    it('userHasPermission should return false if permission argument is not listed in permission list', () => {
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.tenantAlias) {
                    return "test-tenant";
                }

                if (key == storageHelper.user.permissions) {
                    return JSON.parse("[\"viewMyAccount\", \"editMyAccount\", \"canDoThis\"]");
                }
            },
        };

        let userService: UserService = new UserService(session as SessionDataManager, applicationService);
        userService.retrieveLoggedInUserData();
        service = new ExpressionMethodService(null, null, null, null, null, userService,
            resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.userHasPermission('cannotDoThis')).toBe(false);
    });

    it('getUserType should return `customer` if userService.isCustomer is true', () => {
        const userService: any = {
            isCustomer: true,
        };
        service = new ExpressionMethodService(null, null, null, null, null, userService as UserService,
            resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.getUserType()).toBe('customer');
    });

    it('getUserType should return `client` if userService.isCustomer is false', () => {
        const userService: any = {
            isCustomer: false,
        };
        service = new ExpressionMethodService(null, null, null, null, null, userService as UserService,
            resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.getUserType()).toBe('client');
    });

    it('quoteHasCustomer should return `true` if quote has customer id', () => {
        applicationService.customerId = '123123';
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.quoteHasCustomer()).toBe(true);
    });

    it('quoteHasCustomer should return `false` if quote has no customer id', () => {
        applicationService.customerId = null;
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.quoteHasCustomer()).toBe(false);
    });

    it('substring should return ic given the string sacrifice', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substring(str, 6, 8)).toBe('ic');
    });

    it('substring should throw Error when str is empty', () => {
        const str: string = '';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);

        expect(() => service.substring(str, 6, 8))
            .toThrow(new Error(`Value for 'str' cannot be empty.`));
    });

    it('substring should return first character of string when start position is 0 and end position is 1', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substring(str, 0, 1)).toBe('s');
    });

    it('substring should return first 2 character of string when start position is 0 and end position is 2', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null,  null, null);
        expect(service.substring(str, 0, 2)).toBe('sa');
    });

    it('substring should return empty when start and end position are both 0', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substring(str, 0, 0)).toBe('');
    });

    it('substring should return from first character in the string when start position is negative', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substring(str, -2, 3)).toBe('sam');
    });

    it('substring should return from start position character up to the last character when end'+
    ' position is not provided', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substring(str, 2)).toBe('mple');
    });

    it('substring should reverse the start position and end position when start position is greater than' +
    ' end position', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substring(str, 4, 2)).toBe('mp');
    });

    it('substring should return ice given the string sacrifice and start position is 6 and' +
    ' end position is 9', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substring(str, 6, 9)).toBe('ice');
    });

    it('substringNew should return first character of string when start position is 0 and end position is 1', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substringNew(str, 0, 1)).toBe('s');
    });

    it('substringNew should return first 2 character of string when start position is 0 and end position is 2', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substringNew(str, 0, 2)).toBe('sa');
    });

    it('substringNew should return empty when start and end position are both 0', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substringNew(str, 0, 0)).toBe('');
    });

    it('substringNew should return from first character in the string when start position is negative', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substringNew(str, -2, 3)).toBe('sam');
    });

    it('substringNew should return from start position character up to the last character when end'+
    ' position is not provided', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substringNew(str, 2)).toBe('mple');
    });

    it('substringNew should reverse the start position and end position when start position is greater than' +
    ' end position', () => {
        const str: string = 'sample';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substringNew(str, 4, 2)).toBe('mp');
    });

    it('substringNew should return ice given the string sacrifice and start position is 6 and' +
    ' end position is 9', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.substringNew(str, 6, 9)).toBe('ice');
    });

    it('substringNew should throw Error when str is empty', () => {
        const str: string = '';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);

        expect(() => service.substringNew(str, 6, 8))
            .toThrow(new Error(`Value for 'str' cannot be empty.`));
    });

    it('indexOf should return 6 given the string sacrifice and searchstring ice', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.indexOf(str, 'ice')).toBe(6);
    });

    it('length should return 9 given the string sacrifice', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.length(str)).toBe(9);
    });

    it('indexOf should throw Error when str is empty', () => {
        const str: string = '';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(() => service.indexOf(str, 'i'))
            .toThrow(new Error(`Value for 'str' cannot be empty.`));
    });

    it('indexOf should throw Error when searchstring is empty', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(() => service.indexOf(str, ''))
            .toThrow(new Error(`Value for 'searchString' cannot be empty.`));
    });

    it('indexOf should throw Error when searchstring s length is more than the str length', () => {
        const str: string = 'sacrifice';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(() => service.indexOf(str, 'sacrificer'))
            .toThrow(new Error(`Length of 'searchString' cannot be greater than the length of 'str'.`));
    });

    it('currencyAsNumber should return the number value of the given currency string', () => {
        const currencyString: string = '$4,000.50';
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.currencyAsNumber(currencyString)).toEqual(4000.5);
    });

    it('currencyAsString should include minor units when includeMinorUnits is not set'
        + 'only if it\'s not a whole number', () => {
        service =
            new ExpressionMethodService(
                null, formService, null, null, null, null, resumeApplicationService, null, workflowStatusService,
                null, configService, null, null, null, matchingFieldsSubjectService, null, null, null, localeService);
        expect(service.currencyAsString(123.45, null, null)).toEqual('$123.45');
        expect(service.currencyAsString(123.0, null, null)).toEqual('$123');
    });

    it('currencyAsSentence should include minor units when includeMinorUnits is set and '
    + 'only if it\'s not a whole number', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        expect(service.currencyAsSentence(1.45, true)).toEqual('One dollar and forty-five cents');
        expect(service.currencyAsSentence(20, true)).toEqual('Twenty dollars');
    });

    it('currencyAsSentence should not include minor units when includeMinorUnits is not set', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        expect(service.currencyAsSentence(1.45, false)).toEqual('One dollar');
        expect(service.currencyAsSentence(20.45, false)).toEqual('Twenty dollars');
    });

    it('currencyAsSentence should support negative value', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        expect(service.currencyAsSentence(-12)).toEqual('Minus twelve dollars');
        expect(service.currencyAsSentence(-12.24)).toEqual('Minus twelve dollars and twenty-four cents');
    });

    it('currencyAsSentence returns correct noun', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        const testCases: any = [
            { input: 1.45, result: 'One dollar and forty-five cents' },
            { input: 1.01, result: 'One dollar and one cent' },
            { input: 1.1, result: 'One dollar and ten cents' },
            { input: 1.02, result: 'One dollar and two cents' },
            { input: 2.02, result: 'Two dollars and two cents' },
        ];
        testCases.forEach((testCase: any) => {
            expect(service.currencyAsSentence(testCase.input))
                .toEqual(testCase.result);
        });

    });

    it('currencyAsSentence should round up to two decimal places only.', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        const testCases: any = [
            { input: 1.7890, result: 'One dollar and seventy-nine cents' },
            { input: 1.89, result: 'One dollar and eighty-nine cents' },
            { input: 1.99, result: 'One dollar and ninety-nine cents' },
        ];
        testCases.forEach((testCase: any) => {
            expect(service.currencyAsSentence(testCase.input))
                .toEqual(testCase.result);
        });
    });

    it('currencyAsSentence should return zero when currency input is null', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        expect(service.currencyAsSentence(null))
            .toEqual('Zero dollars');
    });

    it('currencyAsSentence should return correct number in words for different place value', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);

        const testCases: any = [
            { input: 8, result: 'Eight dollars' },
            { input: 12, result: 'Twelve dollars' },
            { input: 120, result: 'One hundred twenty dollars' },
            { input: 1053, result: 'One thousand fifty-three dollars' },
            { input: 524653, result: 'Five hundred twenty-four thousand six hundred fifty-three dollars' },
            { input: 1524653, result: 'One million five hundred twenty-four thousand six hundred fifty-three dollars' },
            { input: 81524653, result: 'Eighty-one million five hundred twenty-four thousand six ' +
            'hundred fifty-three dollars' },
            { input: 7581524650, result: 'Seven billion five hundred eighty-one million five ' +
            'hundred twenty-four thousand six hundred fifty dollars' },
        ];
        testCases.forEach((testCase: any) => {
            expect(service.currencyAsSentence(testCase.input, false, 'USD'))
                .toEqual(testCase.result);
        });
    });

    it('currencyAsSentence should return correct number in words for dollar', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        let currencies: Array<string> = ['AUD', 'USD', 'BSD', 'BBD', 'BMD', 'BND', 'CAD', 'KYD', 'XCD', 'SVC',
            'FJD', 'GYD', 'HKD', 'LRD', 'NAD', 'NZD', 'SGD', 'SBD', 'SRD', 'TVD'];
        currencies.forEach((currency: any) => {
            const testCases: any = [
                { input: 1.45, result: 'One dollar and forty-five cents' },
                { input: 1.01, result: 'One dollar and one cent' },
                { input: 1.1, result: 'One dollar and ten cents' },
                { input: 1.02, result: 'One dollar and two cents' },
                { input: 2.02, result: 'Two dollars and two cents' },
                {
                    input: 12345678, includeMinorUnits: false,
                    result: 'Twelve million three hundred forty-five thousand six hundred seventy-eight dollars' },

                {
                    input: 12345678.90, includeMinorUnits: true,
                    result: `Twelve million three hundred forty-five thousand ` +
                     `six hundred seventy-eight dollars and ninety cents`,
                },
            ];
            testCases.forEach((testCase: any) => {
                expect(service.currencyAsSentence(testCase.input, testCase.includeMinorUnits, currency))
                    .toEqual(testCase.result);
            });
        });
    });

    it('currencyAsSentence should return correct number in words for euro', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);

        const testCases: any = [
            { input: 1.45, result: 'One euro and forty-five cents' },
            { input: 1.01, result: 'One euro and one cent' },
            { input: 1.1, result: 'One euro and ten cents' },
            { input: 1.02, result: 'One euro and two cents' },
            { input: 2.02, result: 'Two euros and two cents' },
            { input: 12345678, includeMinorUnits: false,
                result: 'Twelve million three hundred forty-five thousand six hundred seventy-eight euros' },
            { input: 12345678.90, includeMinorUnits: true,
                result: `Twelve million three hundred forty-five thousand ` +
                 `six hundred seventy-eight euros and ninety cents` },
        ];

        testCases.forEach((testCase: any) => {
            expect(service.currencyAsSentence(testCase.input, testCase.includeMinorUnits, 'EUR'))
                .toEqual(testCase.result);
        });

    });

    it('currencyAsSentence should return correct number in words for pesos', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        let currencies: Array<string> = ['ARS', 'CLP', 'COP'];
        currencies.forEach((currency: any) => {
            const testCases: any = [
                { input: 1.45, result: 'One peso and forty-five centavos' },
                { input: 1.01, result: 'One peso and one centavo' },
                { input: 1.1, result: 'One peso and ten centavos' },
                { input: 1.02, result: 'One peso and two centavos' },
                { input: 2.02, result: 'Two pesos and two centavos' },

                { input: 12345678, includeMinorUnits: false, result: `Twelve million three hundred forty-five ` +
                `thousand six hundred seventy-eight pesos` },

                { input: 12345678.90, includeMinorUnits: true, result: `Twelve million three hundred forty-five ` +
                `thousand six hundred seventy-eight pesos and ninety centavos` },
            ];
            testCases.forEach((testCase: any) => {
                expect(service.currencyAsSentence(testCase.input, testCase.includeMinorUnits, currency))
                    .toEqual(testCase.result);
            });
        });
    });

    it('currencyAsSentence should return correct number in words for kina', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);

        const testCases: any = [
            { input: 1.45, result: 'One kina and forty-five toea' },
            { input: 1.01, result: 'One kina and one toea' },
            { input: 1.1, result: 'One kina and ten toea' },
            { input: 1.02, result: 'One kina and two toea' },
            { input: 2.02, result: 'Two kina and two toea' },
            {
                input: 12345678, includeMinorUnits: false,
                result: 'Twelve million three hundred forty-five thousand six hundred seventy-eight kina' },

            { input: 12345678.90, includeMinorUnits: true,
                result: 'Twelve million three hundred forty-five thousand six hundred seventy-eight kina '
                    + 'and ninety toea' },
        ];

        testCases.forEach((testCase: any) => {
            expect(service.currencyAsSentence(testCase.input, testCase.includeMinorUnits, 'PGK'))
                .toEqual(testCase.result);
        });
    });

    it('currencyAsSentence should return correct number in words for pounds', () => {
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, configService, null, null, null, matchingFieldsSubjectService,
            null, null, null, localeService);
        let currencies: Array<string> = ['GBP', 'GGP', 'FKP', 'GIP', 'IMP', 'JEP', 'SHP'];

        currencies.forEach((currency: any) => {
            const testCases: any = [
                { input: 1.45, result: 'One pound and forty-five pence' },
                { input: 1.01, result: 'One pound and one penny' },
                { input: 1.1, result: 'One pound and ten pence' },
                { input: 1.02, result: 'One pound and two pence' },
                { input: 2.02, result: 'Two pounds and two pence' },

                { input: 12345678, includeMinorUnits: false,
                    result: 'Twelve million three hundred forty-five thousand six hundred seventy-eight pounds' },

                { input: 12345678.90, includeMinorUnits: true,
                    result: 'Twelve million three hundred forty-five thousand six hundred seventy-eight pounds and '
                        + 'ninety pence',
                },
            ];

            testCases.forEach((testCase: any) => {
                expect(service.currencyAsSentence(testCase.input, testCase.includeMinorUnits, currency))
                    .toEqual(testCase.result);
            });

        });
    });

    it('isCurrency should return `true` when string parameter is a currency', () => {
        const currencyString1: string = '$4,000.50';
        const currencyString2: string = '$ 4,000.50';
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.isCurrency(currencyString1)).toBe(true);
        expect(service.isCurrency(currencyString2)).toBe(true);
    });

    it('getPreviousWorkflowStep should return the current step when the parameter is 0', () => {
        workflowStatusService.workflowStepHistory.push('1');
        workflowStatusService.workflowStepHistory.push('2');
        workflowStatusService.workflowStepHistory.push('current');
        service = new ExpressionMethodService(applicationService, null, null, null, null, null,
            resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousWorkflowStep(0)).toEqual('current');
    });

    it('generatePremiumFundingContractPdfUrl should return correct premium funding pdf download link.', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null, null,
            resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.generatePremiumFundingContractPdfUrl('12345', '67890')).
            toEqual('https://api.premiumfunding.net.au/contract/12345/PDF/67890');
    });

    it('getPreviousWorkflowStep should return the step as per number sequence provided in the parameter', () => {
        workflowStatusService.workflowStepHistory.push('1');
        workflowStatusService.workflowStepHistory.push('2');
        workflowStatusService.workflowStepHistory.push('3');
        workflowStatusService.workflowStepHistory.push('4');
        workflowStatusService.workflowStepHistory.push('current');
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousWorkflowStep(1)).toEqual('4');
        expect(service.getPreviousWorkflowStep(3)).toEqual('2');
        expect(service.getPreviousWorkflowStep(4)).toEqual('1');
        expect(service.getPreviousWorkflowStep(5)).toEqual('');
    });

    it('decimalNumber should return the parse the passed string to a float if string represents a valid number', () => {
        const currentString: string = '10.1';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.decimalNumber(currentString)).toEqual(10.1);
    });

    it('decimalNumber should return zero when the passed string is NaN', () => {
        const emptyString: string = '';
        const notANumberString: string = 'awe1.1';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.decimalNumber(emptyString)).toEqual(0);
        expect(service.decimalNumber(notANumberString)).toEqual(0);
    });

    it('userHasAccount should return true when the current logged in user is a client or admin '
        + 'and the quote is loaded', () => {
        workflowStatusService.isApplicationLoaded = true;
        const userServiceMock: object = {
            isCustomerOrClientLoggedIn: true,
        };
        service = new ExpressionMethodService(null, null, null, null, null, userServiceMock as UserService,
            resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.userHasAccount()).toBe(true);
    });

    it('customerHasAccount should return true when the quote has customer that is associated to a user', () => {
        applicationService.customerId = '123123';
        const userServiceMock: any = {
            isLoadedCustomerHasUser: true,
        };
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            userServiceMock as UserService, resumeApplicationService, null, workflowStatusService, null, null, null,
            null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.customerHasAccount()).toBe(true);
    });

    it('number should return number when the quote has customer that is associated to a user', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.number('1,000,000')).toEqual(1000000);
        expect(service.number('1,000,000.00')).toEqual(1000000);
        expect(service.number('1000000.00')).toEqual(1000000);
        expect(service.number('1000000')).toEqual(1000000);
    });

    it('isName should return true for valid name', () => {
        let validName1: string = "Shaquille O'neal";
        let validName2: string = "Shaquille O-neal";
        let validName3: string = "Shaquille O-neal Jr.";
        let validName4: string = "O'neal, Shaquille  Jr.";
        let invalidName1: string = "O'neal, Shaquille  1st";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.isName(validName1)).toBe(true);
        expect(service.isName(validName2)).toBe(true);
        expect(service.isName(validName3)).toBe(true);
        expect(service.isName(validName4)).toBe(true);
        expect(service.isName(invalidName1)).toBe(false);
    });

    it('getUserType should return "customer" when the logged in user is an Agent from a different tenant', () => {
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.tenantAlias) {
                    return "different-tenant";
                }

                if (key == storageHelper.user.userType) {
                    return UserType.Client;
                }
            },
        };

        testGetUserType(session, applicationService, "customer");
    });

    it('getUserType should return "customer" when the logged in user is a Customer from the same tenent)', () => {
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.tenantAlias) {
                    return "test-tenant";
                }

                if (key == storageHelper.user.userType) {
                    return UserType.Customer;
                }
            },
        };

        testGetUserType(session, applicationService, "customer");
    });

    it('getUserType should return "client" when the user is logged is an a Agent from the same tenant', () => {
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.tenantAlias) {
                    return "test-tenant";
                }

                if (key == storageHelper.user.userType) {
                    return UserType.Client;
                }
            },
        };

        testGetUserType(session, applicationService, "client");
    });

    it('getUserType should return "client" when the user is logged in as a ClientAdmin from the same tenant', () => {
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.tenantAlias) {
                    return "test-tenant";
                }

                if (key == storageHelper.user.userType) {
                    return UserType.Client;
                }
            },
        };

        testGetUserType(session, applicationService, "client");
    });

    // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
    function testGetUserType(
        session: any,
        applicationService: ApplicationService,
        expectedResult: string,
    ): void {
        let userService: UserService = new UserService(session as SessionDataManager, applicationService);
        userService.retrieveLoggedInUserData();
        service = new ExpressionMethodService(null, null, null, null, null, userService, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getUserType()).toBe(expectedResult);
    }

    it('getPreviousQuoteId should return a quoteId when one is set', () => {
        resumeApplicationService.saveQuoteIdForLater('123123', 1);
        const userServiceMock: any = {
            isClientLoggedIn: false,
            isCustomerLoggedIn: false,
        };
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            userServiceMock as UserService, resumeApplicationService, null, workflowStatusService, null, null, null,
            null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousQuoteId()).toBe('123123');
    });

    it('getPreviousQuoteId should return the previous quoteId even if there is a current quoteId', () => {
        resumeApplicationService.saveQuoteIdForLater('123123', 1);
        applicationService.quoteId = '555555';
        const userServiceMock: any = {
            isClientLoggedIn: false,
            isCustomerLoggedIn: false,
        };
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            userServiceMock as UserService, resumeApplicationService, null, workflowStatusService, null, null, null,
            null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousQuoteId()).toBe('123123');
    });

    it('getPreviousQuoteId should return null when there is no previous quote', () => {
        resumeApplicationService.deleteQuoteId();
        const userServiceMock: any = {
            isClientLoggedIn: false,
            isCustomerLoggedIn: false,
        };
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            userServiceMock as UserService, resumeApplicationService, null, workflowStatusService, null, null, null,
            null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousQuoteId()).toBe(null, null);
    });

    it('getPreviousClaimId should return a claimId when one is set', () => {
        resumeApplicationService.saveClaimIdForLater('123123', 1);
        const userServiceMock: any = {
            isClientLoggedIn: false,
            isCustomerLoggedIn: false,
        };
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            userServiceMock as UserService, resumeApplicationService, null, workflowStatusService, null, null, null,
            null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousClaimId()).toBe('123123');
    });

    it('getPreviousClaimId should return the previous claimId event if there is a current claimId', () => {
        resumeApplicationService.saveClaimIdForLater('123123', 1);
        applicationService.claimId = '666666';
        const userServiceMock: any = {
            isClientLoggedIn: false,
            isCustomerLoggedIn: false,
        };
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            userServiceMock as UserService, resumeApplicationService, null, workflowStatusService, null, null, null,
            null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousClaimId()).toBe('123123');
    });

    it('getPreviousClaimId should return null when there is no previous claim', () => {
        resumeApplicationService.deleteClaimId();
        const userServiceMock: any = {
            isClientLoggedIn: false,
            isCustomerLoggedIn: false,
        };
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            userServiceMock as UserService, resumeApplicationService, null, workflowStatusService, null, null, null,
            null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getPreviousClaimId()).toBe(null, null);
    });

    it('getBaseUrl should return baseUrl for url with path', () => {
        const baseUrl: string = "https://app.ubind.com.au";
        const baseUrlWithPath: string = "https://app.ubind.com.au/portal/ubind";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrlWithPath)).toBe(baseUrl);
    });

    it('getBaseUrl should return baseUrl for url with query', () => {
        const baseUrl: string = "https://app.ubind.com.au";
        const baseUrlWithQuery: string = "https://app.ubind.com.au?tenantId=tenantId";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrlWithQuery)).toBe(baseUrl);
    });

    it('getBaseUrl should return baseUrl for url with fragment', () => {
        const baseUrl: string = "https://app.ubind.com.au";
        const baseUrlWithFragment: string = "https://app.ubind.com.au#tenant";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrlWithFragment)).toBe(baseUrl);
    });

    it('getBaseUrl should return baseUrl for url with path, query and fragment', () => {
        const baseUrl: string = "https://app.ubind.com.au";
        const baseUrlWithPathQueryAndFragment: string
            = "https://app.ubind.com.au/portal/ubind?tenantId=tenantId#target";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrlWithPathQueryAndFragment)).toBe(baseUrl);
    });

    it('getBaseUrl should return baseUrl for url with no path, no query and no fragment', () => {
        const baseUrl: string = "https://app.ubind.com.au";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrl)).toBe(baseUrl);
    });

    it('getBaseUrl should return baseUrl for url with path and any protocol casing', () => {
        const baseUrl: string = "https://app.ubind.com.au";
        const baseUrlWithPath: string = "https://app.ubind.com.au/portal/ubind";
        const baseUrlWithPathUpperCaseProtocol: string = "HTTPS://app.ubind.com.au/portal/ubind";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrlWithPathUpperCaseProtocol)).toBe(baseUrl);
        expect(service.getBaseUrl(baseUrlWithPath)).toBe(baseUrl);
    });

    it('getBaseUrl should return baseUrl for url with path and http protocol with any protocol casing', () => {
        const baseUrl: string = "http://app.ubind.com.au";
        const baseUrlWithPath: string = "http://app.ubind.com.au/portal/ubind";
        const baseUrlWithPathUpperCaseProtocol: string = "HTTP://app.ubind.com.au/portal/ubind";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrlWithPathUpperCaseProtocol)).toBe(baseUrl);
        expect(service.getBaseUrl(baseUrlWithPath)).toBe(baseUrl);
    });

    it('getBaseUrl should return baseUrl for url with unusual path', () => {
        const baseUrl: string = "http://app.ubind.com.au";
        const baseUrlWithPath: string = "http://app.ubind.com.au/https://";
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getBaseUrl(baseUrlWithPath)).toBe(baseUrl);
    });

    it('questionSetsAreValid should return true when a question set is valid', () => {
        formService.setQuestionSetValidity('ratingPrimary', true);
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.questionSetsAreValid(['ratingPrimary'])).toBeTruthy();
    });

    it('questionSetsAreValid should return false when a question set is not valid', () => {
        configServiceFake.debug = true;
        let questionsWidget: QuestionsWidget = new QuestionsWidget(null, null, configService, formService, null,
            null, null, null, applicationService, null, null, fieldGroupTrackingService);
        questionsWidget.name = 'ratingPrimary';
        questionsWidget.visible = true;
        formService.setQuestionSetValidity('ratingPrimary', false);
        formService.registerQuestionSet(questionsWidget);
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.questionSetsAreValid(['ratingPrimary'])).toBeFalsy();
    });

    it('questionSetsAreValidOrHidden should return true when a question set is hidden', () => {
        configServiceFake.debug = true;
        let questionsWidget: QuestionsWidget = new QuestionsWidget(null, null, configService as ConfigService,
            formService, null, null, null, null, applicationService, null, null, fieldGroupTrackingService);
        questionsWidget.name = 'ratingPrimary';
        questionsWidget.visible = false;
        formService.setQuestionSetValidity('ratingPrimary', false);
        formService.registerQuestionSet(questionsWidget);
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.questionSetsAreValidOrHidden(['ratingPrimary'])).toBeTruthy();
    });

    it('getFieldPropertyValue should return the property value when it\'s set', () => {
        formService.setFieldData('myField', { cheese: "cheddar" });
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getFieldPropertyValue('myField', 'cheese')).toBe('cheddar');
    });

    it('sum should return the sum of values', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([10, 20])).toBe(30.00);
    });

    it('sum should return 0 for an empty array', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([])).toBe(0.00);
    });

    it('sum should return the sum of values with decimal places equals to 1150.13', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([1000.05, 150.08])).toBe(1150.13);
    });

    it('sum should return the sum of values with decimal places equals to 0.13', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([0.05, 0.08])).toBe(0.13);
    });

    it('sum should return the sum of values with decimal places equals to 100.60', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([50.10, 50.50])).toBe(100.60);
    });

    it('sum should return the sum of values with decimal places equals to 150.801', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([100.301, 50.50])).toBe(150.801);
    });

    it('sum should return the sum of values with decimal places equals to 1150.805', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([1000.305, 150.50])).toBe(1150.805);
    });

    it('sum should return 0 for an null values of array', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.sum([null, null])).toBe(0.00);
    });

    it('isPaymentCardExpiryDate should return false when the the parameter is null.', () => {
        const expiryDate: any = null;
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.isPaymentCardExpiryDate(expiryDate)).toBeFalsy();
    });

    it('isPaymentCardExpiryDate should return true when the payment card expiry date is valid', () => {
        const expiryDate: string = "02/25";
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.isPaymentCardExpiryDate(expiryDate)).toBeTruthy();
    });

    it('isPaymentCardExpiryDate should return false when the expiry date is invalid.', () => {
        const expiryDate: string = "02/2025";
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.isPaymentCardExpiryDate(expiryDate)).toBeFalsy();
    });

    it('generateSummaryTableOfActiveTriggersOfType should return a html table of the triggers with messages.', () => {
        let triggerService: object = {
            getActiveTriggersByType: (triggerType: string): Array<TriggerDisplayConfig> => {
                let result: Array<TriggerDisplayConfig> = new Array<TriggerDisplayConfig>();
                result.push({
                    name: "ageOver80",
                    type: "review",
                    header: null,
                    title: null,
                    message: null,
                    displayPrice: true,
                    reviewerExplanation: "We cannot insure drivers over the age of 80.",
                });
                return result;
            },
        };
        service = new ExpressionMethodService(null, null, null, null, null, null, null, null, null,
            triggerService as TriggerService, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.generateSummaryTableOfActiveTriggersOfType('review'))
            .toContain('<td class="summary-value">We cannot insure drivers over the age of 80.</td>');
    });

    it('getFieldValues formats the fields using question metadata', () => {
        expressionInputSubjectService.getFieldValueSubject('carValue', 66000);
        expressionInputSubjectService.getFieldValueSubject('isCharity', true);
        configServiceFake.addBooleanMetadataResult();
        configServiceFake.addCurrencyMetadataResult();
        service = new ExpressionMethodService(null, null, null, null, null, null, null, null, null, null,
            configService, expressionInputSubjectService, fieldFormatterResolver, fieldMetadataService,
            matchingFieldsSubjectService, null, null, null, null);
        const result: any = service.getFieldValues(['carValue', 'isCharity']);
        expect(result[0]).toBe('$66,000');
        expect(result[1]).toBe('Yes');
    });

    it('getFieldValue formats the field using question metadata', () => {
        expressionInputSubjectService.getFieldValueSubject('carValue', 66000);
        configServiceFake.addCurrencyMetadataResult();
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getFieldValue('carValue')).toBe('$66,000');
    });

    it('generateSummaryTableOfFields generates the correct table', () => {
        expressionInputSubjectService.getFieldValueSubject('carValue', 66000);
        expressionInputSubjectService.getFieldValueSubject('hasClaims', true);
        configServiceFake.addBooleanMetadataResult();
        configServiceFake.addCurrencyMetadataResult();
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        let result: any = service.generateSummaryTableOfFields(['carValue', 'hasClaims']);
        expect(result).toContain('<td class="summary-value">Yes</td></tr>');
    });

    it('generateSummaryTable generates the correct table when data is in rows', () => {
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        let result: any = service.generateSummaryTable([['carValue', '$66,000'], ['numberOfDrivers', '3']]);
        expect(result).toContain('<td class="summary-value">3</td></tr>');
    });

    it('generateSummaryTable generates the correct table when data is in columns', () => {
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        let result: any = service.generateSummaryTable([['carValue', 'numberOfDrivers'], ['$66,000', '3']], false);
        expect(result).toContain('<tr><td class="summary-name">numberOfDrivers</td><td');
    });

    it('generateSummaryTable generates the correct number of column headers when the data is for row', () => {
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        let result: any = service.generateSummaryTable(
            [['A1', 'A2', 'A3', 'A4'], ['B1', 'B2', 'B3', 'B4'], ['C1', 'C2', 'C3', 'C4']],
            true,
            ['H1', 'H2', 'H3', 'H4']);
        expect(result).toContain('<table class="summary-table custom"><tr><th class="summary-name">H1</th>'
            + '<th class="summary-value">H2</th><th class="summary-value">H3</th>'
            + '<th class="summary-value">H4</th></tr>');
    });

    it('generateSummaryTable throw error when incorrect number of column headers was passed in and the data is for row',
        () => {
            service = new ExpressionMethodService(null, null, null, null, null,
                null, null, null, null, null, configService, expressionInputSubjectService,
                fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
            let errorMessage: string = 'The expression method generateSummaryTable takes an optional parameter '
            + '"headers", which is expected to be an array of strings, with the same number of items as the number of '
            + 'columns of data (4). However the headers array had 5 '
            + 'items which does not match.';
            expect(() => service.generateSummaryTable(
                [['A1', 'A2', 'A3', 'A4'], ['B1', 'B2', 'B3', 'B4'], ['C1', 'C2', 'C3', 'C4']],
                true,
                ['H1', 'H2', 'H3', 'H4', 'H5']))
                .toThrow(Errors.Product.Configuration(errorMessage));
        });

    it('generateSummaryTable skips row when there is an empty or null cell',
        () => {
            service = new ExpressionMethodService(null, null, null, null, null,
                null, null, null, null, null, configService, expressionInputSubjectService,
                fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
            let result: any = service.generateSummaryTable(
                [['A1', 'A2', 'A3'], ['B1', 'B2', null],  ['C1', 'C2', '']],
                true,
                ['H1', 'H2', 'H3']);
            expect(result).toContain('<table class="summary-table custom"><tr><th class="summary-name">H1</th>'
            + '<th class="summary-value">H2</th><th class="summary-value">H3</th></tr><tr>'
            + '<td class="summary-name">A1</td><td class="summary-value">A2</td>'
            + '<td class="summary-value">A3</td></tr></table>');
        });

    it('generateSummaryTable generates the correct number of column headers when the data is for column', () => {
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        let result: any = service.generateSummaryTable(
            [['A1', 'A2', 'A3', 'A4'], ['B1', 'B2', 'B3', 'B4'], ['C1', 'C2', 'C3', 'C4']],
            false,
            ['H1', 'H2', 'H3']);
        expect(result).toContain('<table class="summary-table custom"><tr><th class="summary-name">H1</th>'
            + '<th class="summary-value">H2</th><th class="summary-value">H3</th></tr>');
    });

    it('generateSummaryTable throw error when incorrect number of column headers was passed in ' +
        'and the data is for column',
    () => {
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        let errorMessage: string = 'The expression method generateSummaryTable takes an optional parameter '
                + '"headers", which is expected to be an array of strings, with the same number of items as '
                + 'the number of columns of data (3). However the headers array had 4 '
                + 'items which does not match.';
        expect(() => service.generateSummaryTable(
            [['A1', 'A2', 'A3', 'A4'], ['B1', 'B2', 'B3', 'B4'], ['C1', 'C2', 'C3', 'C4']],
            false,
            ['H1', 'H2', 'H3', 'H4']))
            .toThrow(Errors.Product.Configuration(errorMessage));
    });

    it('generateSummaryTable skips empty cells when asked to do so', () => {
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        let result: any = service.generateSummaryTable([['a', '1'], ['b', '']], true, null, null, true);
        expect(result).toContain('<td class="summary-value">1</td></tr></table>');
    });

    it('getAssetsFolderUrl returns the correct folder url', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        let result: string = service.getAssetsFolderUrl();
        const url: string = `${document.location.origin}/api/v1`
            + `/tenant/${applicationService.tenantAlias}`
            + `/product/${applicationService.productAlias}`
            + `/environment/${applicationService.environment}`
            + `/form-type/${applicationService.formType}`
            + `/release/${applicationService.productReleaseId}`
            + `/asset`;
        expect(result).toBe(url);
    });

    it('min calculates the minimum of two values', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        let result: number = (<any>service).min(5, 6);
        expect(result).toBe(5);
    });

    it('min calculates the minimum an array of values', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        let numbers: Array<number> = [5, 6, 7, 8];
        let result: number = (<any>service).min(numbers);
        expect(result).toBe(5);
    });

    it('max calculates the maximum of two values', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        let result: number = (<any>service).max(5, 6);
        expect(result).toBe(6);
    });

    it('max calculates the maximum an array of values', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        let numbers: Array<number> = [5, 6, 7, 8];
        let result: number = (<any>service).max(numbers);
        expect(result).toBe(8);
    });

    it('getFieldData should return the data that was set when it\'s an array', () => {
        formService.setFieldData('myField', ['a', 'b', 'c']);
        service = new ExpressionMethodService(null, formService, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getFieldData('myField')).toEqual(['a', 'b', 'c']);
    });

    it('getDefaultLandingPageUrl returns the correct url when there is an organisation', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        let result: string = service.getDefaultLandingPageUrl();
        const url: string = `${document.location.origin}/assets/landing-page.html`
            + `?tenant=${applicationService.tenantAlias}`
            + `&organisation=${applicationService.organisationAlias}`
            + `&productId=${applicationService.productAlias}&environment=${applicationService.environment}`;
        expect(result).toBe(url);
    });

    it('getDefaultLandingPageUrl returns the correct url when there is no organization', () => {
        applicationService = new ApplicationService();
        (<any>applicationService)._formType = FormType.Quote;
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'test-tenant',
            'test-tenant',
            null,
            null,
            null,
            'test-productId',
            'test-product',
            'production',
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null, null);
        resumeApplicationService = new ResumeApplicationService(
            new ResilientStorage(),
            new SessionDataManager(),
            applicationService);

        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        let result: string = service.getDefaultLandingPageUrl();
        const url: string = `${document.location.origin}/assets/landing-page.html`
            + `?tenant=${applicationService.tenantAlias}`
            + `&productId=${applicationService.productAlias}&environment=${applicationService.environment}`;
        expect(result).toBe(url);
    });

    it('getQuoteCalculationState should return premiumComplete', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        applicationService.latestQuoteResult = <any>{
            quoteState: QuoteState.Complete,
            calculationState: CalculationState.PremiumComplete,
            ratingSummaryItems: new Array<SourceRatingSummaryItem>(),
        };
        expect(service.getQuoteCalculationState()).toEqual('premiumComplete');
    });

    it('getQuoteCalculationState should return incomplete', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.getQuoteCalculationState()).toEqual('incomplete');
    });

    it('getQuoteCalculationState should return not null value', () => {
        service = new ExpressionMethodService(applicationService, null, null, null, null,
            null, resumeApplicationService, null, workflowStatusService, null, null, null, null, null,
            matchingFieldsSubjectService, null, null, null, null);
        expect(service.getQuoteCalculationState()).not.toBeNull();
    });

    it('getFieldSearchTerm gets the fields search term', () => {
        expressionInputSubjectService.getFieldSearchTermSubject('mySearchSelect', 'toy');
        service = new ExpressionMethodService(null, null, null, null, null,
            null, null, null, null, null, configService, expressionInputSubjectService,
            fieldFormatterResolver, fieldMetadataService, matchingFieldsSubjectService, null, null, null, null);
        expect(service.getFieldSearchTerm('mySearchSelect')).toBe('toy');
    });

    it(`toUpperCase should return the upperCase using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';

        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toUpperCase(defaultFormatStr, TextCase.Default)).toBe('CAN I HAVE A COFFEE PLEASE');
        expect(service.toUpperCase(camelCaseFormatStr, TextCase.Camel)).toBe('CAN I HAVE 12 COFFEES PLEASE');
        expect(service.toUpperCase(pascalFormatStr, TextCase.Pascal)).toBe('CAN I HAVE 12 COFFEES PLEASE');
        expect(service.toUpperCase(kebabFormatStr, TextCase.Kebab)).toBe('CAN I HAVE 12 COFFEES PLEASE');
        expect(service.toUpperCase(snakeFormatStr, TextCase.Snake)).toBe('CAN I HAVE 12 COFFEES PLEASE');
        expect(service.toUpperCase(defaultFormatStr1, TextCase.Default)).toBe('CANIHAVE12COFFEESPLEASE');
    });

    it(`toLowerCase should return the lowerCase using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';

        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toLowerCase(defaultFormatStr, TextCase.Default)).toBe('can i have a coffee please');
        expect(service.toLowerCase(camelCaseFormatStr, TextCase.Camel)).toBe('can i have 12 coffees please');
        expect(service.toLowerCase(pascalFormatStr, TextCase.Pascal)).toBe('can i have 12 coffees please');
        expect(service.toLowerCase(kebabFormatStr, TextCase.Kebab)).toBe('can i have 12 coffees please');
        expect(service.toLowerCase(snakeFormatStr, TextCase.Snake)).toBe('can i have 12 coffees please');
        expect(service.toLowerCase(defaultFormatStr1, TextCase.Default)).toBe('canihave12coffeesplease');
    });

    it(`toTitleCase should return the title case using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toTitleCase(defaultFormatStr, TextCase.Default)).toBe('Can I Have a Coffee Please');
        expect(service.toTitleCase(camelCaseFormatStr, TextCase.Camel)).toBe('Can I Have 12 Coffees Please');
        expect(service.toTitleCase(pascalFormatStr, TextCase.Pascal)).toBe('Can I Have 12 Coffees Please');
        expect(service.toTitleCase(kebabFormatStr, TextCase.Kebab)).toBe('Can I Have 12 Coffees Please');
        expect(service.toTitleCase(snakeFormatStr, TextCase.Snake)).toBe('Can I Have 12 Coffees Please');
        expect(service.toTitleCase(defaultFormatStr1, TextCase.Default)).toBe('Canihave12coffeesplease');
    });

    it(`toCapitalCase should return the capital case using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toCapitalCase(defaultFormatStr, TextCase.Default)).toBe('Can I Have A Coffee Please');
        expect(service.toCapitalCase(camelCaseFormatStr, TextCase.Camel)).toBe('Can I Have 12 Coffees Please');
        expect(service.toCapitalCase(pascalFormatStr, TextCase.Pascal)).toBe('Can I Have 12 Coffees Please');
        expect(service.toCapitalCase(kebabFormatStr, TextCase.Kebab)).toBe('Can I Have 12 Coffees Please');
        expect(service.toCapitalCase(snakeFormatStr, TextCase.Snake)).toBe('Can I Have 12 Coffees Please');
        expect(service.toCapitalCase(defaultFormatStr1, TextCase.Default)).toBe('Canihave12coffeesplease');
    });

    it(`toSentenceCase should return the sentence case using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toSentenceCase(defaultFormatStr, TextCase.Default)).toBe('Can i have a coffee please');
        expect(service.toSentenceCase(camelCaseFormatStr, TextCase.Camel)).toBe('Can i have 12 coffees please');
        expect(service.toSentenceCase(pascalFormatStr, TextCase.Pascal)).toBe('Can i have 12 coffees please');
        expect(service.toSentenceCase(kebabFormatStr, TextCase.Kebab)).toBe('Can i have 12 coffees please');
        expect(service.toSentenceCase(snakeFormatStr, TextCase.Snake)).toBe('Can i have 12 coffees please');
        expect(service.toSentenceCase(defaultFormatStr1, TextCase.Default)).toBe('Canihave12coffeesplease');
    });

    it(`toCamelCase should return the camel case using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toCamelCase(defaultFormatStr, TextCase.Default)).toBe('canIHaveACoffeePlease');
        expect(service.toCamelCase(camelCaseFormatStr, TextCase.Camel)).toBe('canIHave12CoffeesPlease');
        expect(service.toCamelCase(pascalFormatStr, TextCase.Pascal)).toBe('canIHave12CoffeesPlease');
        expect(service.toCamelCase(kebabFormatStr, TextCase.Kebab)).toBe('canIHave12CoffeesPlease');
        expect(service.toCamelCase(snakeFormatStr, TextCase.Snake)).toBe('canIHave12CoffeesPlease');
        expect(service.toCamelCase(defaultFormatStr1, TextCase.Default)).toBe('canihave12coffeesplease');
    });

    it(`toPascalCase should return the pascal case using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toPascalCase(defaultFormatStr, TextCase.Default)).toBe('CanIHaveACoffeePlease');
        expect(service.toPascalCase(camelCaseFormatStr, TextCase.Camel)).toBe('CanIHave12CoffeesPlease');
        expect(service.toPascalCase(pascalFormatStr, TextCase.Pascal)).toBe('CanIHave12CoffeesPlease');
        expect(service.toPascalCase(kebabFormatStr, TextCase.Kebab)).toBe('CanIHave12CoffeesPlease');
        expect(service.toPascalCase(snakeFormatStr, TextCase.Snake)).toBe('CanIHave12CoffeesPlease');
        expect(service.toPascalCase(defaultFormatStr1, TextCase.Default)).toBe('Canihave12coffeesplease');
    });

    it(`toKebabCase should return the kebab case using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toKebabCase(defaultFormatStr, TextCase.Default)).toBe('can-i-have-a-coffee-please');
        expect(service.toKebabCase(camelCaseFormatStr, TextCase.Camel)).toBe('can-i-have-12-coffees-please');
        expect(service.toKebabCase(pascalFormatStr, TextCase.Pascal)).toBe('can-i-have-12-coffees-please');
        expect(service.toKebabCase(kebabFormatStr, TextCase.Kebab)).toBe('can-i-have-12-coffees-please');
        expect(service.toKebabCase(snakeFormatStr, TextCase.Snake)).toBe('can-i-have-12-coffees-please');
        expect(service.toKebabCase(defaultFormatStr1, TextCase.Default)).toBe('canihave12coffeesplease');
    });

    it(`toSnakeCase should return the snake case using `
        + `inputFormat [default,camel,pascal,kebab,snake] `
        + `given the input string`, () => {
        const defaultFormatStr: string = 'Can I have a coffee please';
        const defaultFormatStr1: string = 'canIHave12CoffeesPlease';
        const camelCaseFormatStr: string = 'canIHave12CoffeesPlease';
        const pascalFormatStr: string = 'CanIHave12CoffeesPlease';
        const kebabFormatStr: string = 'can-i-have-12-coffees-please';
        const snakeFormatStr: string = 'can_i_have_12_coffees_please';
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.toSnakeCase(defaultFormatStr, TextCase.Default)).toBe('can_i_have_a_coffee_please');
        expect(service.toSnakeCase(camelCaseFormatStr, TextCase.Camel)).toBe('can_i_have_12_coffees_please');
        expect(service.toSnakeCase(pascalFormatStr, TextCase.Pascal)).toBe('can_i_have_12_coffees_please');
        expect(service.toSnakeCase(kebabFormatStr, TextCase.Kebab)).toBe('can_i_have_12_coffees_please');
        expect(service.toSnakeCase(snakeFormatStr, TextCase.Snake)).toBe('can_i_have_12_coffees_please');
        expect(service.toSnakeCase(defaultFormatStr1, TextCase.Default)).toBe('canihave12coffeesplease');
    });

    it(`join should return all the elements of an array into a string`, () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.join(['Melbourne', 'VIC'])).toBe('MelbourneVIC');
        expect(service.join(['John', 'Doe'])).toBe('JohnDoe');
        expect(service.join(['John', 'Doe', 'Jr'], '_')).toBe('John_Doe_Jr');

        const arr: Array<string> = new Array('First', 'Second', 'Third');
        expect(service.join(arr, ',')).toBe('First,Second,Third');
        expect(service.join(arr, ', ')).toBe('First, Second, Third');
        expect(service.join(arr, ' + ')).toBe('First + Second + Third');
    });

    it(`getContextEntities should return object representation of the context entities`, () => {
        service = new ExpressionMethodService(
            null, null, null, null, null, null, resumeApplicationService, null, workflowStatusService,
            null, null, null, null, null, matchingFieldsSubjectService, null, contextEntityService, null, null);
        let contextEntities: any = service.getContextEntities();
        expect(contextEntities['organisation']['alias'].toString()).toBe('carl');
        expect(contextEntities['tenant']['alias'].toString()).toBe('carl');
        expect(contextEntities['product']['alias'].toString()).toBe('dev6841');
    });

    it(`fileProperties should return the correct file properties when it\'s not available in AttachmentService`, () => {
        const testAttachmentFieldValue: string
            = 'images.jpg:image/jpeg:03fd3f14-985f-4f38-a762-3096f491e0ad:275:183:5754';
        service = new ExpressionMethodService(
            null, null, new AttachmentService(), null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null,
            contextEntityService, null, null);
        const fileProperties: AttachmentFileProperties = service.fileProperties(testAttachmentFieldValue);
        expect(fileProperties.fileName).toBe('images.jpg');
    });

    it(`operationInProgress should return true when an operation is in progress`, fakeAsync(() => {
        operationStatusService = new OperationStatusService();
        let loggerService: LoggerService = new LoggerService(null, applicationService);
        operationInstructionService = new OperationInstructionService(
            new FakeOperationFactory() as any as OperationFactory,
            configService, applicationService, contextEntityService, null, null, null, operationStatusService,
            loggerService, null);
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService,
            null, workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null,
            operationStatusService, null);
        operationInstructionService.execute(new OperationInstruction('calculation'));
        tick();
        expect(service.operationInProgress('calculation')).toBeTrue();
        expect(service.operationInProgress()).toBeTrue();
        expect(service.operationInProgress('formUpdate')).toBeFalse();
    }));

    describe('isDate', () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null);

        it('should return true for valid dates', () => {
            expect(service.isDate('31/12/2023')).toBe(true);
            expect(service.isDate('01/01/2020')).toBe(true);
        });

        it('should return false for invalid dates', () => {
            expect(service.isDate('32/12/2023')).toBe(false);
            expect(service.isDate('31-12-2023')).toBe(false);
        });
    });

    describe('isWeekday', () => {
        it('should return true for weekdays', () => {
            expect(service.isWeekday('20/07/2023')).toBe(true);
            expect(service.isWeekday(service.date('17/07/2023'))).toBe(true);
        });

        it('should return false for weekends', () => {
            expect(service.isWeekday('22/07/2023')).toBe(false);
            expect(service.isWeekday(service.date('23/07/2023'))).toBe(false);
        });
    });

    describe('isWeekend', () => {
        it('should return true for weekends', () => {
            expect(service.isWeekend('22/07/2023')).toBe(true);
            expect(service.isWeekend(service.date('23/07/2023'))).toBe(true);
        });

        it('should return false for weekdays', () => {
            expect(service.isWeekend('20/07/2023')).toBe(false);
            expect(service.isWeekend(service.date('19/07/2023'))).toBe(false);
        });
    });

    describe('dateAsString', () => {
        it('should format date correctly', () => {
            expect(service.dateAsString(1679809200000)).toBe('26/03/2023');
            expect(service.dateAsString(1619218800000)).toBe('24/04/2021');
        });
    });

    describe('date', () => {
        it('should convert valid date strings to timestamps', () => {
            const dateStr: string = '01/08/2023';
            const expectedTimestamp: number = new Date(2023, 7, 1).getTime();
            expect(service.date(dateStr)).toEqual(expectedTimestamp);
        });

        it('should throw an error for invalid date strings', () => {
            const invalidDateStr: string = 'invalid-date-string';
            expect(() => service.date(invalidDateStr)).toThrowError(/Error trying to parse this string as a date/);
        });

        it('should throw an error for null input', () => {
            const nullDateStr: null = null;
            expect(() => service.date(nullDateStr)).toThrowError(/Error trying to parse this string as a date/);
        });

        it('should throw an error for undefined input', () => {
            const undefinedDateStr: undefined = undefined;
            expect(() => service.date(undefinedDateStr)).toThrowError(/Error trying to parse this string as a date/);
        });
    });

    it('should return correct age', () => {
        // jan 1, 1995
        const today: Date = new Date();
        let expectedAge: number = today.getFullYear() - 1995;
        expect(service.getAgeFromDateOfBirth('01/01/1995')).toBe(expectedAge);
        // jan 1, 2001
        expectedAge = today.getFullYear() - 2001;
        expect(service.getAgeFromDateOfBirth(978307200000)).toBe(expectedAge);
    });

    describe('now', () => {
        it('should return the current Unix timestamp', () => {
            const now: number = new Date().getTime();
            expect(service.now()).toBeLessThanOrEqual(now + 2);
        });
    });

    describe('today', () => {
        it('should return the Unix timestamp for the start of the current day', () => {
            const now: Date = new Date();
            const startOfDay: number = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
            expect(service.today()).toBe(startOfDay);
        });
    });

    describe('thisMonth', () => {
        it('should return the Unix timestamp for the start of the current month', () => {
            const now: Date = new Date();
            const startOfMonth: number = new Date(now.getFullYear(), now.getMonth(), 1).getTime();
            expect(service.thisMonth()).toBe(startOfMonth);
        });
    });

    describe('thisYear', () => {
        it('should return the Unix timestamp for the start of the current year', () => {
            const now: Date = new Date();
            const startOfYear: number = new Date(now.getFullYear(), 1, 1).getTime();
            expect(service.thisYear()).toBe(startOfYear);
        });
    });

    describe('getYear', () => {
        it('should return the year from the provided date string', () => {
            const dateStr: string = '22/07/2023';
            const millis: number = service.date('31/12/2022');
            expect(service.getYear(dateStr)).toBe(2023);
            expect(service.getYear(millis)).toBe(2022);
        });
    });

    describe('getMonth', () => {
        it('should return the month from the provided date string', () => {
            const dateStr1: string = '22/07/2023';
            const millis: number = service.date('31/12/2022');
            expect(service.getMonth(dateStr1)).toBe(7);
            expect(service.getMonth(millis)).toBe(12);
        });
    });

    describe('getDate', () => {
        it('should return the day from the provided date string', () => {
            const dateStr: string = '22/07/2023';
            const millis: number = service.date('31/12/2022');
            expect(service.getDate(dateStr)).toBe(22);
            expect(service.getDate(millis)).toBe(31);
        });
    });

    describe('inThePast', () => {
        it('should return true if the provided date is in the past', () => {
            const pastDateStr: string = '01/01/2000';
            expect(service.inThePast(pastDateStr)).toBe(true);
            expect(service.inThePast(service.date(pastDateStr))).toBe(true);
        });

        it('should return false if the provided date is in the future', () => {
            const now: Date = new Date();
            const currentDate: Date = new Date(
                now.getFullYear() + 10, now.getMonth(), now.getDate() + 8);
            expect(service.inThePast(service.dateAsString(currentDate.getTime()))).toBe(false);
            expect(service.inThePast(currentDate.getTime())).toBe(false);
        });
    });

    describe('inTheFuture', () => {
        it('should return true if date is in the future', () => {
            const currentDate: Date = new Date();
            const futureDate: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 2)
                .getTime();
            expect(service.inTheFuture(futureDate)).toBe(true);
            expect(service.inTheFuture(service.dateAsString(futureDate))).toBe(true);
        });

        it('should return false if date is in the past', () => {
            const currentDate: Date = new Date();
            const yesterday: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() - 1)
                .getTime();
            expect(service.inTheFuture(yesterday)).toBe(false);
            expect(service.inTheFuture(service.dateAsString(yesterday))).toBe(false);
        });

        it('should return false if date is today', () => {
            const currentDate: Date = new Date();
            const today: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.inTheFuture(service.dateAsString(today))).toBe(false);
            expect(service.inTheFuture(today)).toBe(false);
        });
    });

    describe('inTheNextYears', () => {
        it('should return true if date is within the next x years', () => {
            const currentDate: Date = new Date();
            const twoYearsFromNow: number = new Date(
                currentDate.getFullYear() + 2, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.inTheNextYears(twoYearsFromNow, 5)).toBe(true);
            expect(service.inTheNextYears(service.dateAsString(twoYearsFromNow), 5)).toBe(true);
        });

        it('should return false if date is not within the next x years', () => {
            const currentDate: Date = new Date();
            const fourYearsFromNow: number = new Date(
                currentDate.getFullYear() + 4, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.inTheNextYears(fourYearsFromNow, 3)).toBe(false);
            expect(service.inTheNextYears(service.dateAsString(fourYearsFromNow), 3)).toBe(false);
        });

        it('should return false if date is a year ago and x is 1', () => {
            const currentDate: Date = new Date();
            const oneYearAgo: number = new Date(
                currentDate.getFullYear() - 1, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.inTheNextYears(oneYearAgo, 1)).toBe(false);
            expect(service.inTheNextYears(service.dateAsString(oneYearAgo), 1)).toBe(false);
        });
    });

    describe('inTheNextMonths', () => {
        it('should return true if the provided date is within the next specified months', () => {
            const currentDate: Date = new Date();
            const currentYear: number = currentDate.getFullYear();
            const currentMonth: number = currentDate.getMonth();
            const nextMonthDate: number = new Date(
                currentYear, currentMonth + 1, currentDate.getDate())
                .getTime();
            expect(service.inTheNextMonths(nextMonthDate, 1)).toBe(true);

            const nextTwoMonthsDateAsString: string = service.dateAsString(new Date(
                currentYear, currentMonth + 2, currentDate.getDate()).getTime());
            expect(service.inTheNextMonths(nextTwoMonthsDateAsString, 2)).toBe(true);
        });
    });

    describe('inTheNextDays', () => {
        it('should return true if date is within the next x days', () => {
            const currentDate: Date = new Date();
            const threeDaysFromNow: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 3)
                .getTime();
            expect(service.inTheNextDays(threeDaysFromNow, 4)).toBe(true);
            expect(service.inTheNextDays(service.dateAsString(threeDaysFromNow), 4)).toBe(true);
        });

        it('should return false if date is not within the next x days', () => {
            const currentDate: Date = new Date();
            const fiveDaysFromNow: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 5)
                .getTime();
            expect(service.inTheNextDays(fiveDaysFromNow, 4)).toBe(false);
            expect(service.inTheNextDays(service.dateAsString(fiveDaysFromNow), 4)).toBe(false);
        });
    });

    describe('inTheLastYears', () => {
        it('should return true if the provided date is within the last specified years', () => {
            const now: Date = new Date();
            const currentDate: number = new Date(
                now.getFullYear(), now.getMonth(), now.getDate())
                .getTime();
            expect(service.inTheLastYears(
                currentDate, 5)).toBe(true);
            expect(service.inTheLastYears(
                service.dateAsString(currentDate), 5)).toBe(true);
        });

        it('should return false if the provided date is within the future specified years', () => {
            const now: Date = new Date();
            const futureDate: number = new Date(
                now.getFullYear() + 4, now.getMonth(), now.getDate())
                .getTime();
            expect(service.inTheLastYears(
                futureDate, 0)).toBe(false);
            expect(service.inTheLastYears(
                service.dateAsString(futureDate), 5)).toBe(false);
        });
    });

    describe('inTheLastMonths', () => {
        it('should return true if date is within the last x months', () => {
            const currentDate: Date = new Date();
            const oneMonthAgo: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth() - 1, currentDate.getDate())
                .getTime();
            expect(service.inTheLastMonths(oneMonthAgo, 2)).toBe(true);
            expect(service.inTheLastMonths(service.dateAsString(oneMonthAgo), 2)).toBe(true);
        });

        it('should return false if date is not within the last x months', () => {
            const currentDate: Date = new Date();
            const threeMonthsAgo: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth() - 3, currentDate.getDate())
                .getTime();
            expect(service.inTheLastMonths(threeMonthsAgo, 2)).toBe(false);
            expect(service.inTheLastMonths(service.dateAsString(threeMonthsAgo), 2)).toBe(false);
        });
    });

    describe('inTheLastDays', () => {
        it('should return true for dates within the last 7 days', () => {
            const date: Date = new Date();
            const sixDaysAgo: Date = new Date(date);
            sixDaysAgo.setDate(date.getDate() - 6);
            expect(service.inTheLastDays(sixDaysAgo.getTime(), 7)).toBe(true);
            expect(service.inTheLastDays(service.dateAsString(sixDaysAgo.getTime()), 7)).toBe(true);
        });

        it('should return false for dates outside the last 7 days', () => {
            const date: Date = new Date();
            const pastDate: Date = new Date(date);
            pastDate.setDate(date.getDate() - 9);

            expect(service.inTheLastDays(pastDate.getTime(), 7)).toBe(false);
            expect(service.inTheLastDays(service.dateAsString(pastDate.getTime()), 7)).toBe(false);
        });

        it('should throw an error for invalid date strings', () => {
            const invalidDateStr: string = 'invalid-date-string'; // Invalid format
            expect(() => service.inTheLastDays(invalidDateStr, 7))
                .toThrowError(/Error trying to parse this string as a date/);
        });
    });

    describe('atLeastYearsInTheFuture', () => {
        it('should return true if the provided date is at least the specified number of years in the future', () => {
            const now: Date = new Date();
            const futureDate: number = new Date(
                now.getFullYear() + 3, now.getMonth(), now.getDate()).getTime();
            expect(service.atLeastYearsInTheFuture(futureDate, 3)).toBe(true);
            expect(service.atLeastYearsInTheFuture(service.dateAsString(futureDate), 3)).toBe(true);
        });

        it('should return false if the provided date is at least the specified number of years in the past', () => {
            const now: Date = new Date();
            const previousDate: number = new Date(
                now.getFullYear() - 3, now.getMonth(), now.getDate()).getTime();
            expect(service.atLeastYearsInTheFuture(previousDate, 2)).toBe(false);
            expect(service.atLeastYearsInTheFuture(service.dateAsString(previousDate), 2)).toBe(false);
        });
    });

    describe('atLeastMonthsInTheFuture', () => {
        it('should return true if the provided date is at least the specified number of months in the future', () => {
            const now: Date = new Date();
            const currentDate: number = new Date(
                now.getFullYear(), now.getMonth() + 3, now.getDate())
                .getTime();
            expect(service.atLeastMonthsInTheFuture(currentDate, 1)).toBe(true);
            expect(service.atLeastMonthsInTheFuture(service.dateAsString(currentDate), 1)).toBe(true);
        });

        it('should return false if the provided date is at least the specified number of months in the past', () => {
            const now: Date = new Date();
            const previousDate: number = new Date(
                now.getFullYear(), now.getMonth() - 3, now.getDate())
                .getTime();
            expect(service.atLeastMonthsInTheFuture(previousDate, 4)).toBe(false);
            expect(service.atLeastMonthsInTheFuture(service.dateAsString(previousDate), 4)).toBe(false);
        });
    });

    describe('atLeastDaysInTheFuture', () => {
        it('should return true if the provided date is at least the specified number of days in the future', () => {
            const now: Date = new Date();
            const futureDate: number = new Date(
                now.getFullYear(), now.getMonth(), now.getDate() + 3)
                .getTime();
            expect(service.atLeastDaysInTheFuture(futureDate, 2)).toBe(true);
            expect(service.atLeastDaysInTheFuture(service.dateAsString(futureDate), 2)).toBe(true);
        });

        it('should return false if the provided date is at least the specified number of days in the past', () => {
            const now: Date = new Date();
            const futureDate: number = new Date(
                now.getFullYear(), now.getMonth(), now.getDate() - 3)
                .getTime();
            expect(service.atLeastDaysInTheFuture(futureDate, 5)).toBe(false);
            expect(service.atLeastDaysInTheFuture(service.dateAsString(futureDate), 5)).toBe(false);
        });
    });

    describe('atMostYearsInTheFuture', () => {
        it('should return true if the provided date is at most the specified number of years in the future', () => {
            const now: Date = new Date();
            const futureDates: number = new Date(
                now.getFullYear() + 4, now.getMonth(), now.getDate())
                .getTime();
            expect(service.atMostYearsInTheFuture(futureDates, 4)).toBe(true);
            expect(service.atMostYearsInTheFuture(service.dateAsString(futureDates), 4)).toBe(true);
        });

        it('should return false if the provided date is at most the specified number of years in the past', () => {
            const now: Date = new Date();
            const previousDate: number = new Date(
                now.getFullYear() - 2, now.getMonth(), now.getDate())
                .getTime();
            expect(service.atMostYearsInTheFuture(previousDate, - 3)).toBe(false);
            expect(service.atMostYearsInTheFuture(service.dateAsString(previousDate), - 3)).toBe(false);
        });
    });

    describe('atMostMonthsInTheFuture', () => {
        it('should return true if the provided date is at most the specified number of months in the future', () => {
            const now: Date = new Date();
            const futureDates: number = new Date(
                now.getFullYear(), now.getMonth() + 6, now.getDate())
                .getTime();
            expect(service.atMostMonthsInTheFuture(futureDates, 6)).toBe(true);
            expect(service.atMostMonthsInTheFuture(service.dateAsString(futureDates), 6)).toBe(true);
        });

        it('should return false if the provided date is at most the specified number of months in the past', () => {
            const now: Date = new Date();
            const previousDate: number = new Date(
                now.getFullYear(), now.getMonth() + 6, now.getDate())
                .getTime();
            expect(service.atMostMonthsInTheFuture(previousDate, 4)).toBe(false);
            expect(service.atMostMonthsInTheFuture(service.dateAsString(previousDate), 4)).toBe(false);
        });
    });

    describe('atMostDaysInTheFuture', () => {
        it('should return true if date is exactly x days in the future', () => {
            const currentDate: Date = new Date();
            const threeDaysAhead: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 3)
                .getTime();
            expect(service.atMostDaysInTheFuture(threeDaysAhead, 3)).toBe(true);
            expect(service.atMostDaysInTheFuture(service.dateAsString(threeDaysAhead), 3)).toBe(true);
        });

        it('should return true if date is less than x days in the future', () => {
            const currentDate: Date = new Date();
            const twoDaysAhead: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 2)
                .getTime();
            expect(service.atMostDaysInTheFuture(twoDaysAhead, 3)).toBe(true);
            expect(service.atMostDaysInTheFuture(service.dateAsString(twoDaysAhead), 3)).toBe(true);
        });

        it('should return false if date is more than x days in the future', () => {
            const currentDate: Date = new Date();
            const fiveDaysAhead: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 5)
                .getTime();
            expect(service.atMostDaysInTheFuture(service.dateAsString(fiveDaysAhead), 3)).toBe(false);
            expect(service.atMostDaysInTheFuture(fiveDaysAhead, 3)).toBe(false);
        });

        it('should return false if date is in the past', () => {
            const currentDate: Date = new Date();
            const previousDate: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 6)
                .getTime();
            expect(service.atMostDaysInTheFuture(previousDate, 3)).toBe(false);
            expect(service.atMostDaysInTheFuture(service.dateAsString(previousDate), 3)).toBe(false);
        });
    });

    describe('atLeastYearsInThePast', () => {
        it('should return true if date is exactly x years in the past', () => {
            const currentDate: Date = new Date();
            const threeYearsAgo: number = new Date(
                currentDate.getFullYear() - 3, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atLeastYearsInThePast(threeYearsAgo, 3)).toBe(true);
            expect(service.atLeastYearsInThePast(service.dateAsString(threeYearsAgo), 3)).toBe(true);
        });

        it('should return true if date is more than x years in the past', () => {
            const currentDate: Date = new Date();
            const fiveYearsAgo: number = new Date(
                currentDate.getFullYear() - 5, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atLeastYearsInThePast(fiveYearsAgo, 3)).toBe(true);
            expect(service.atLeastYearsInThePast(service.dateAsString(fiveYearsAgo), 3)).toBe(true);
        });

        it('should return false if date is less than x years in the past', () => {
            const currentDate: Date = new Date();
            const twoYearsAgo: number = new Date(
                currentDate.getFullYear() - 2, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atLeastYearsInThePast(twoYearsAgo, 3)).toBe(false);
            expect(service.atLeastYearsInThePast(service.dateAsString(twoYearsAgo), 3)).toBe(false);
        });

        it('should return false if date is in the future', () => {
            const currentDate: Date = new Date();
            const nextYear: number = new Date(
                currentDate.getFullYear() + 1, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atLeastYearsInThePast(nextYear, 1)).toBe(false);
            expect(service.atLeastYearsInThePast(service.dateAsString(nextYear), 1)).toBe(false);
        });

        it('should return false if date is today and x is any positive number', () => {
            const currentDate: Date = new Date();
            const today: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atLeastYearsInThePast(today, 1)).toBe(false);
            expect(service.atLeastYearsInThePast(service.dateAsString(today), 1)).toBe(false);
        });
    });

    describe('atLeastMonthsInThePast', () => {
        it('should return true if date is exactly x months in the past', () => {
            const currentDate: Date = new Date();
            const threeMonthsBack: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth() - 3, currentDate.getDate())
                .getTime();
            expect(service.atLeastMonthsInThePast(service.dateAsString(threeMonthsBack), 3)).toBe(true);
            expect(service.atLeastMonthsInThePast(threeMonthsBack, 3)).toBe(true);
        });

        it('should return true if date is more than x months in the past', () => {
            const currentDate: Date = new Date();
            const fiveMonthsBack: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth() - 5, currentDate.getDate())
                .getTime();
            expect(service.atLeastMonthsInThePast(fiveMonthsBack, 3)).toBe(true);
            expect(service.atLeastMonthsInThePast(service.dateAsString(fiveMonthsBack), 3)).toBe(true);
        });

        it('should return false if date is less than x months in the past', () => {
            const currentDate: Date = new Date();
            const oneMonthBack: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth() - 1, currentDate.getDate())
                .getTime();
            expect(service.atLeastMonthsInThePast(oneMonthBack, 3)).toBe(false);
            expect(service.atLeastMonthsInThePast(service.dateAsString(oneMonthBack), 3)).toBe(false);
        });

        it('should return false if date is in the future', () => {
            const currentDate: Date = new Date();
            const oneMonthAhead: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth() + 1, currentDate.getDate())
                .getTime();
            expect(service.atLeastMonthsInThePast(oneMonthAhead, 3)).toBe(false);
            expect(service.atLeastMonthsInThePast(service.dateAsString(oneMonthAhead), 3)).toBe(false);
        });

        it('should return false if date is today and x is any positive number', () => {
            const currentDate: Date = new Date();
            const today: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atLeastMonthsInThePast(today, 1)).toBe(false);
            expect(service.atLeastMonthsInThePast(service.dateAsString(today), 1)).toBe(false);
        });
    });

    describe('atLeastDaysInThePast', () => {
        it('should return true if the provided date is at least the specified number of days in the past', () => {
            const now: Date = new Date();
            const previousDate: number = new Date(
                now.getFullYear(), now.getMonth(), now.getDate() - 3)
                .getTime();
            expect(service.atLeastDaysInThePast(previousDate, 1)).toBe(true);
            expect(service.atLeastDaysInThePast(service.dateAsString(previousDate), 1)).toBe(true);
        });
    });

    describe('atMostYearsInThePast', () => {
        it('should return true if date is within the past x years', () => {
            const currentDate: Date = new Date();
            const twoYearsAgo: number = new Date(
                currentDate.getFullYear() - 2, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atMostYearsInThePast(twoYearsAgo, 3)).toBe(true);
            expect(service.atMostYearsInThePast(service.dateAsString(twoYearsAgo), 3)).toBe(true);
        });
        it('should return false if date is beyond the past x years', () => {
            const currentDate: Date = new Date();
            const fourYearsAgo: number = new Date(
                currentDate.getFullYear() - 4, currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atMostYearsInThePast(fourYearsAgo, 3)).toBe(false);
            expect(service.atMostYearsInThePast(service.dateAsString(fourYearsAgo), 3)).toBe(false);
        });

        it('should return true if date is today and x is any positive number', () => {
            const currentDate: Date = new Date();
            const today: number = new Date(
                currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate())
                .getTime();
            expect(service.atMostYearsInThePast(today, 1)).toBe(true);
            expect(service.atMostYearsInThePast(service.dateAsString(today), 1)).toBe(true);
        });
    });

    describe('atMostMonthsInThePast', () => {
        it('should return true if the provided date is at most the specified number of months in the past', () => {
            const now: Date = new Date();
            const previousDate: number = new Date(
                now.getFullYear(), now.getMonth() + 3, now.getDate())
                .getTime();
            expect(service.atMostMonthsInThePast(previousDate, 7)).toBe(true);
            expect(service.atMostMonthsInThePast(service.dateAsString(previousDate), 7)).toBe(true);
        });

        it('should return false if the provided date is at most the specified number of months in the future', () => {
            const now: Date = new Date();
            const futureDate: number = new Date(
                now.getFullYear(), now.getMonth() - 5, now.getDate())
                .getTime();
            expect(service.atMostMonthsInThePast(futureDate, 4)).toBe(false);
            expect(service.atMostMonthsInThePast(service.dateAsString(futureDate), 4)).toBe(false);
        });
    });

    describe('atMostDaysInThePast', () => {
        it('should return true if the provided date is at most the specified number of days in the past', () => {
            const now: Date = new Date();
            const currentDate: number = new Date(
                now.getFullYear(), now.getMonth(), now.getDate() - 3)
                .getTime();
            expect(service.atMostDaysInThePast(currentDate, 4)).toBe(true);
            expect(service.atMostDaysInThePast(service.dateAsString(currentDate), 4)).toBe(true);
        });

        it('should return false if the provided date is at most the specified number of days in the future', () => {
            const now: Date = new Date();
            const futureDate: number = new Date(
                now.getFullYear(), now.getMonth(), now.getDate() -5)
                .getTime();
            expect(service.atMostDaysInThePast(futureDate, 4)).toBe(false);
            expect(service.atMostDaysInThePast(service.dateAsString(futureDate), 4)).toBe(false);
        });
    });

    describe('addYears', () => {
        it('should add the specified number of years to the input date', () => {
            const millis: number = service.date('21/08/2021');
            const inputDate: string = '21/08/2021';
            const expectedDate: number = service.date('21/08/2025');
            const resultMillis: number = service.addYears(millis, 4);
            const resultDate: number = service.addYears(inputDate, 4);
            expect(resultMillis).toBe(expectedDate);
            expect(resultDate).toBe(expectedDate);
        });
    });

    describe('addMonths', () => {
        it('should add the specified number of months to the input date', () => {
            const inputDate: string = '21/08/2021';
            const millis: number = service.date('21/08/2021');
            const expectedDate: number = service.date('21/11/2021');
            const resultDate: number = service.addMonths(millis, 3);
            const resultMillis: number = service.addMonths(inputDate, 3);
            expect(resultDate).toBe(expectedDate);
            expect(resultMillis).toBe(expectedDate);
        });
    });

    describe('addDays', () => {
        it('should add the specified number of days to the input date', () => {
            const millis: number = service.date('21/08/2021');
            const inputDate: string = '21/08/2021';
            const expectedDate: number = service.date('24/08/2021');
            const resultDate: number = service.addDays(inputDate, 3);
            const resultMillis: number = service.addDays(millis, 3);
            expect(resultDate).toBe(expectedDate);
            expect(resultMillis).toBe(expectedDate);
        });
    });

    it(`stringReplace should return One Four Three given stringReplace('One Two Three', /Two/, 'Four')`, () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.stringReplace('One Two Three', /Two/, 'Four')).toBe('One Four Three');
    });

    it(`stringReplace should return A*A*A*A* given stringReplace('A1A2A3A4', /[0-9]/g, '*')`, () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.stringReplace('A1A2A3A4', /[0-9]/g, '*')).toBe('A*A*A*A*');
    });

    it(`stringContains should return Yes given stringContains('A1A2A3A4', /[0-9]/) ? 'Yes' : 'No'`, () => {
        service = new ExpressionMethodService(null, null, null, null, null, null, resumeApplicationService, null,
            workflowStatusService, null, null, null, null, null, matchingFieldsSubjectService, null, null, null, null);
        expect(service.stringContains('A1A2A3A4', /[0-9]/) ? 'Yes' : 'No').toBe('Yes');
    });

    describe('stringContains', () => {
        it('should return true when the haystack contains the string needle', () => {
            const haystack: string = 'This is a test string';
            const needle: string = 'test';
            expect(service.stringContains(haystack, needle)).toBe(true);
        });

        it('should return true when the haystack URL contains a query parameter delimiter', () => {
            const haystack: string = 'https://localhost:44366/portal/leon/path/product/dev?environment=Development';
            const needle: string = '?';
            expect(service.stringContains(haystack, needle)).toBe(true);
        });

        it('should return true when the haystack URL contains a specified path segment', () => {
            const haystack: string = 'https://localhost:44366/portal/leon/path/product/dev?environment=Development';
            const needle: string = '/leon';
            expect(service.stringContains(haystack, needle)).toBe(true);
        });

        it('should return false when the haystack does not contain the string needle', () => {
            const haystack: string = 'This is a sample string';
            const needle: string = 'nonexistent';
            expect(service.stringContains(haystack, needle)).toBe(false);
        });

        it('should return false for an empty haystack', () => {
            const haystack: string = '';
            const needle: string = 'test';
            expect(service.stringContains(haystack, needle)).toBe(false);
        });

        it('should return true for an empty needle', () => {
            const haystack: string = 'This is a test string';
            const needle: string = '';
            expect(service.stringContains(haystack, needle)).toBe(true);
        });

        it('should return true when the haystack contains a regular expression match', () => {
            const haystack: string = 'Sample text with digits: 12345';
            const needle: RegExp = /\d+/; // Matches one or more digits
            expect(service.stringContains(haystack, needle)).toBe(true);
        });

        it('should return false when the haystack does not match the regular expression', () => {
            const haystack: string = 'No digits here';
            const needle: RegExp = /\d+/; // Matches one or more digits
            expect(service.stringContains(haystack, needle)).toBe(false);
        });
    });
});
