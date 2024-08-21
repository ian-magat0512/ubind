/* eslint-disable no-unused-vars */
import { EventEmitter, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ConfigurationOperation } from '@app/operations/configuration.operation';
import { ConfigProcessorService } from '../services/config-processor.service';
import { EvaluateService } from '../services/evaluate.service';
import { EventService } from '../services/event.service';
import { MessageService } from '../services/message.service';
import { sharedConfig } from '../app.module.shared';
import { CalculationService } from '@app/services/calculation.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { FormService } from '@app/services/form.service';
import { AttachmentService } from '@app/services/attachment.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { ApplicationService } from '@app/services/application.service';
import { AlertService } from '@app/services/alert.service';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { BroadcastService } from '@app/services/broadcast.service';
import { CssProcessorService } from '@app/services/css-processor.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { BehaviorSubject, Subject, Subscription } from 'rxjs';
import { ValidationService } from '@app/services/validation.service';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
import { OperationFactory } from '@app/operations/operation.factory';
import { WebhookService } from '@app/services/webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { PolicyOperation } from '@app/operations/policy.operation';
import { UserService } from '@app/services/user.service';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { TimePipe } from '@app/pipes/time.pipe';
import { AbnPipe } from '@app/pipes/abn.pipe';
import { BsbPipe } from '@app/pipes/bsb.pipe';
import { CreditCardNumberPipe } from '@app/pipes/credit-card-number.pipe';
import { PhoneNumberPipe } from '@app/pipes/phone-number.pipe';
import { NumberPlatePipe } from '@app/pipes/number-plate.pipe';
import { ExpressionInputSubjectService } from './expression-input-subject.service';
import { Expression } from './expression';
import { QuoteType } from '@app/models/quote-type.enum';
import { ExpressionDependencies } from './expression-dependencies';
import { NgSelectModule } from '@ng-select/ng-select';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { QuoteResult } from '@app/models/quote-result';
import { MatchingFieldsSubjectService } from './matching-fields-subject.service';
import { FakeConfigServiceForMetadata } from '@app/services/test/fake-config-service-for-metadata';
import { TaggedFieldsSubjectService } from './tagged-fields-subject.service';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { Alert } from '@app/models/alert';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { ExpressionMethodDependencyService } from './expression-method-dependency.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { MaskPipe } from 'ngx-mask';

/* global spyOn */


describe('Expression', () => {
    let fakeConfigService: FakeConfigServiceForMetadata;

    let workflowServiceStub: any = {
        navigate: new EventEmitter<any>(),
        currentDestination: { stepName: "purchaseQuote" },
        initialised: new BehaviorSubject<boolean>(true),
        actionAborted: new EventEmitter<any>(),
        actionCompleted: new EventEmitter<any>(),
        completedNavigationIn: (): void => { },
        quoteLoadedSubject: new Subject<boolean>(),
        loadedCustomerHasUserSubject: new Subject<boolean>(),
    };

    let alertServiceStub: any = {
        updateSubject: new Subject<Alert>(),
        visibleSubject: new Subject<boolean>(),
        alert: (alert: Alert): void => {},
    };

    let broadcastServiceStub: any = {
        on: (key: any): Subject<any> => new Subject<any>(),
    };

    let operationFactoryStub: any = {
        getStatus: (operation: any): string => 'success',
    };

    let webhookServiceStub: any = {
        getActiveWebhookCount: (): number => 0,
        inProgressSubject: new Subject<boolean>(),
        webhookFieldInProgressSubject: new Subject<boolean>(),
    };

    let attachmentOperationStub: any = {
        operationInProgress: false,
        inProgressSubject: new Subject<boolean>(),
    };

    let policyOperationStub: any = {
        getQuoteType: (): string => 'renewal',
    };

    let encryptionServiceStub: any = {
        loadPublicKey: (): void => {},
    };

    beforeEach(async () => {
        return TestBed.configureTestingModule({
            declarations: [
                ...sharedConfig.declarations,
            ],
            providers: [
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                EventService,
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useValue: workflowServiceStub },
                { provide: ConfigService, useClass: FakeConfigServiceForMetadata },
                FormService,
                { provide: AttachmentService, useClass: AttachmentService },
                { provide: CalculationOperation, useValue: {} },
                { provide: ApplicationService, useClass: ApplicationService },
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: {} },
                { provide: BroadcastService, useValue: broadcastServiceStub },
                { provide: CssProcessorService, useValue: {} },
                { provide: ValidationService, useClass: ValidationService },
                { provide: ExpressionMethodService, useClass: ExpressionMethodService },
                { provide: OperationFactory, useValue: operationFactoryStub },
                { provide: WebhookService, useValue: webhookServiceStub },
                { provide: AttachmentOperation, useValue: attachmentOperationStub },
                { provide: PolicyOperation, useValue: policyOperationStub },
                { provide: UserService, useClass: UserService },
                { provide: ResumeApplicationService, useClass: ResumeApplicationService },
                { provide: OperationFactory, useClass: FakeOperationFactory },
                { provide: ToolTipService, useClass: FakeToolTipService },
                { provide: MaskPipe, useClass: MaskPipe },
                AbnPipe,
                BsbPipe,
                CreditCardNumberPipe,
                CurrencyPipe,
                CssIdentifierPipe,
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                ExpressionDependencies,
                WorkflowStatusService,
                MatchingFieldsSubjectService,
                TaggedFieldsSubjectService,
                { provide: EncryptionService, useValue: encryptionServiceStub },
            ],
            imports: [
                NoopAnimationsModule,
                NgSelectModule,
                ...sharedConfig.imports,
            ],
            schemas: [
                CUSTOM_ELEMENTS_SCHEMA,
            ],
        }).compileComponents().then(() => {
            let messageService: MessageService = TestBed.inject<MessageService>(MessageService);
            spyOn(messageService, 'sendMessage'); // make it do nothing.
        });
    });

    it('a field value should match a string value', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService.getFieldValueSubject('myTestField', 'asdf');
        let expression: Expression = new Expression(
            "myTestField == 'asdf'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('asdf');
        expect(result).toBe(true);
    });

    it('a field value should match one string value or another', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService.getFieldValueSubject('myTestField', 'xxxx');
        let expression: Expression = new Expression(
            "myTestField == 'asdf' || myTestField == 'xxxx'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('xxxx');
        expect(result).toBe(true);
    });

    it('a field value should match a substring', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService.getFieldValueSubject('myTestField', 'abcde');
        let expression: Expression = new Expression(
            "substring(myTestField, 1, 3) == 'bc'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('abcde');
        expect(result).toBe(true);
    });

    it('should detect when a field value contains another string', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService.getFieldValueSubject('myTestField', 'abcde');

        let expression: Expression = new Expression(
            "stringContains(myTestField, 'bcd')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('abcde');
        expect(result).toBe(true);
    });

    it('should evaluate a constant expression when asked', () => {
        let expression: Expression = new Expression(
            "stringContains('abcde', 'bcd')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(true);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe(true);
    });

    it('should re-evaluate every second when it depends on the current time', async () => {
        let date: Date = new Date();
        let currentTimeMillis: number = date.getTime();
        let expressionMethodDependencyService: ExpressionMethodDependencyService
            = TestBed.inject<ExpressionMethodDependencyService>(ExpressionMethodDependencyService);

        // change it to 100ms instead of 1000ms so the test is fast
        (<any>expressionMethodDependencyService).timeUpdateIntervalMillis = 100;
        (<any>expressionMethodDependencyService).createTimeDependantExpressionMethodSubjects();

        let expression: Expression = new Expression(
            "now() - 220 > " + currentTimeMillis,
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');

        expect(expression.isConstant()).toBe(false);

        return new Promise((resolve: any): void => {
            let count: number = 0;
            let subscription: Subscription = expression.nextResultObservable.subscribe((result: any) => {
                count++;
                if (count > 1) {
                    expect(result).toBe(true);
                    subscription.unsubscribe();
                    resolve();
                }
            });
        });
    });

    it('should re-evaulate when an operation\'s status changes', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let expression: Expression = new Expression(
            "operationStatus('load')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.operationStatuses.set('load', 'success');
        applicationService.operationResultSubject.next('blah');
        expect(result).toBe('success');
    });

    it('should re-evaulate when there\'s a new calculation result', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let expression: Expression = new Expression(
            "getQuoteResult()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: QuoteResult;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.latestQuoteResult = {
            calculationState: 'premiumComplete',
        };
        applicationService.operationStatuses.set('calculation', 'success');
        applicationService.calculationResultSubject.next('');
        expect(result.calculationState).toBe('premiumComplete');
    });

    it('should re-evaulate when an attachment field\'s value is updated', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let attachmentService: AttachmentService = TestBed.inject<AttachmentService>(AttachmentService);
        let myTestFieldSubject: any =
            expressionInputSubjectService.getFieldValueSubject(
                'myTestField',
                'testFilename:application/pdf:testAttachmentId:0:0');
        attachmentService.setAttachment({
            fileName: 'testFilename',
            mimeType: 'application/pdf',
            fileData: 'asdfasdfasdf',
            fileSizeBytes: 12,
            imageWidth: 0,
            imageHeight: 0,
            attachmentId: 'testAttachmentId',
            quoteId: 'testQuoteId',
            claimId: null,
        });
        let expression: Expression = new Expression(
            "fileProperties(myTestField)",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('testFilename:application/pdf:testAttachmentId:0:0');
        expect(result.fileSizeBytes).toBe(12);
    });

    it('should re-evaulate when the workflow step changes', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let expression: Expression = new Expression(
            "getCurrentWorkflowStep()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.currentWorkflowDestination = { stepName: 'testWorkflowStep' };
        expect(result).toBe('testWorkflowStep');
    });

    it('should re-evaulate when an application property changes', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.userHasAccount = false;
        let expression: Expression = new Expression(
            "getApplicationValues('userHasAccount')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.userHasAccount = true;
        expect(result).toBe(true);
    });

    it('should re-evaulate when the quote type changes', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let expression: Expression = new Expression(
            "getQuoteType()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.quoteType = QuoteType.Renewal;
        expect(result).toBe('renewal');
    });

    it('should re-evaulate when the policyId gets set', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let expression: Expression = new Expression(
            "getPolicyId()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.policyId = 'testPolicyId';
        expect(result).toBe('testPolicyId');
    });

    it('should re-evaulate when a customer is created for a quote', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let expression: Expression = new Expression(
            "quoteHasCustomer()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.customerId = 'testCustomerId';
        expect(result).toBe(true);
    });

    it('should re-evaulate when a user account is known to exist', async () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let workflowStatusService: any = TestBed.inject<WorkflowStatusService>(WorkflowStatusService);

        let expression: Expression = new Expression(
            "userHasAccount()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');

        expect(expression.isConstant()).toBe(false);

        return new Promise((resolve: any): void => {
            let count: number = 0;
            expression.nextResultObservable.subscribe((result: any): void => {
                count++;
                if (count > 1) {
                    expect(result).toBe(true);
                    resolve();
                }
            });
            workflowStatusService.quoteLoadedSubject.next(true);
            applicationService.userHasAccount = true;
        });
    });

    it('should re-evaulate when a customer\'s user account is created', async () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let workflowStatusService: any = TestBed.inject<WorkflowStatusService>(WorkflowStatusService);
        let userService: any = TestBed.inject<UserService>(UserService);

        let expression: Expression = new Expression(
            "customerHasAccount()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');

        expect(expression.isConstant()).toBe(false);

        return new Promise((resolve: any): void => {
            let count: number = 0;
            expression.nextResultObservable.subscribe((result: any): void => {
                count++;
                if (count > 1) {
                    expect(result).toBe(true);
                    resolve();
                }
            });
            applicationService.customerId = 'testCustomerId';

            // Not setting this one since if we do the next one won't have an effect
            // This is due to it not causing the end result to change value.
            // Only changes in the final expression value will be published.
            // applicationService.userHasAccount = true;

            userService.isLoadedCustomerHasUser = true;
            workflowStatusService.loadedCustomerHasUserSubject.next(true);
        });
    });

    it('should re-evaulate when a quote reference number is set', () => {
        let applicationService: any = TestBed.inject<ApplicationService>(ApplicationService);
        let expression: Expression = new Expression(
            "getQuoteReference()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        applicationService.quoteReference = 'ABYJXC';
        expect(result).toBe('ABYJXC');
    });

    it('should re-evaulate when a quoteId is saved for later', () => {
        let resumeApplicationService: any = TestBed.inject<ResumeApplicationService>(ResumeApplicationService);
        let expression: Expression = new Expression(
            "getPreviousQuoteId()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        resumeApplicationService.saveQuoteIdForLater('testQuoteId', 30);
        expect(result).toBe('testQuoteId');
    });

    it('should re-evaulate when a saved claimId is deleted', () => {
        let resumeApplicationService: any = TestBed.inject<ResumeApplicationService>(ResumeApplicationService);
        resumeApplicationService.saveClaimIdForLater('testClaimId', 30);
        let expression: Expression = new Expression(
            "getPreviousClaimId()",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expression.triggerEvaluation();
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        resumeApplicationService.deleteClaimId('testClaimId');
        expect(result).toBe(null);
    });

    it('should concatenate strings', async () => {
        let expression: Expression = new Expression(
            ' \'send\' + \'Enquiry\'',
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(true);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe('sendEnquiry');
    });

    it('should do maths within an expression', () => {
        let expression: Expression = new Expression(
            ' \'step\' + (1+2)',
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(true);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe('step3');
    });

    it('should return the same value (TEXT + \\r\\n + TEXT)', () => {
        let result: any = new Expression(
            "'text1' + '\r\n' + 'text2' + '\r\n' + 'text3'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual('text1\r\ntext2\r\ntext3');
    });

    it('should return the same value delimited chr(13)chr(10) (TEXT\\r\\nTEXT)', () => {
        let result: any = new Expression(
            "'text1\r\ntext2\r\ntext3'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual("text1\r\ntext2\r\ntext3");
    });

    it('should escape the single quote (\'TEXT\'TEXT)', () => {
        let result: any = new Expression(
            "'\\'string0\\'' + 'string1' + 'string2' + 'asdf\\'string3\\'asdf'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual("'string0'string1string2asdf'string3'asdf");
    });

    it('should concat single and double quoted strings', () => {
        let result: any = new Expression(
            "'string0' + \"string1\"",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual("string0string1");
    });

    it('should escape the back space (\\bTEXT)', () => {
        let result: any = new Expression(
            "'\btext1'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual('\btext1');
    });

    it('should escape the Horizontal Tabulator (\\tTEXT)', () => {
        let result: any = new Expression(
            "'\ttext1'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual('\ttext1');
    });

    it('should escape the Vertical Tabulator (\\vTEXT)', () => {
        let result: any = new Expression(
            "'\vtext1'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual('\vtext1');
    });

    it('should convert \\n to new line string value (TEXT\\nTEXT)', () => {
        let result: any = new Expression(
            "'text1\ntext2'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual('text1\ntext2');
    });

    it('should convert \\r to carriage return string value (TEXT\\rTEXT)', () => {
        let result: any = new Expression(
            "'text1\rtext2'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual('text1\rtext2');
    });

    it('should escape backslash (TEXT\\TEXT)', () => {
        let result: any = new Expression(
            "'\\\\text1\\\\text2\\\\text3'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing').evaluate();
        expect(result).toEqual('\\text1\\text2\\text3');
    });

    it('should use the value of fixed arguments', () => {
        let expression: Expression = new Expression(
            "fixedValue + ' fox'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing',
            { fixedValue: 'brown' });
        expect(expression.isConstant()).toBe(true);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toEqual('brown fox');
    });

    it('should re-evaluate when an observable argument changes', () => {
        let observableValue: Subject<string> = new Subject<string>();
        let expression: Expression = new Expression(
            "observableValue + ' ' + fixedValue + ' fox'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing',
            { fixedValue: 'brown' },
            { observableValue: observableValue });
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        observableValue.next('lazy');
        expect(result).toEqual('lazy brown fox');
    });

    it('should re-evaluate when a question set becomes valid and questionSetsAreValid is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        formService.setQuestionSetValidity('ratingPrimary', false);
        let expression: Expression = new Expression(
            "questionSetsAreValid(['ratingPrimary'])",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        formService.setQuestionSetValidity('ratingPrimary', true);
        expressionInputSubjectService.getQuestionSetValidSubject('ratingPrimary').next(true);
        expect(result).toBeTruthy();
    });

    it('should re-evaluate when a question set becomes valid '
        + 'and questionSetsAreValid is used, with spaces between brackets', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
                = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        formService.setQuestionSetValidity('ratingPrimary', false);
        let expression: Expression = new Expression(
            "questionSetsAreValid( ['ratingPrimary'] )",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        formService.setQuestionSetValidity('ratingPrimary', true);
        expressionInputSubjectService.getQuestionSetValidSubject('ratingPrimary').next(true);
        expect(result).toBeTruthy();
    });

    it('should re-evaluate when a question set becomes valid and questionSetsAreValidOrHidden is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        formService.setQuestionSetValidity('ratingPrimary', false);
        let expression: Expression = new Expression(
            "questionSetsAreValidOrHidden(['ratingPrimary'])",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        formService.setQuestionSetValidity('ratingPrimary', true);
        expressionInputSubjectService.getQuestionSetValidSubject('ratingPrimary').next(true);
        expect(result).toBeTruthy();
    });

    it('should re-evaluate when a question set becomes invalid and questionSetsAreInvalid is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        formService.setQuestionSetValidity('ratingPrimary', true);
        let expression: Expression = new Expression(
            "questionSetsAreInvalid(['ratingPrimary'])",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        formService.setQuestionSetValidity('ratingPrimary', false);
        expressionInputSubjectService.getQuestionSetValidSubject('ratingPrimary').next(true);
        expect(result).toBeTruthy();
    });

    it('should re-evaluate when a question set becomes invalid and questionSetsAreInvalidOrHidden is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        formService.setQuestionSetValidity('ratingPrimary', true);
        let expression: Expression = new Expression(
            "questionSetsAreInvalidOrHidden(['ratingPrimary'])",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        formService.setQuestionSetValidity('ratingPrimary', false);
        expressionInputSubjectService.getQuestionSetValidSubject('ratingPrimary').next(true);
        expect(result).toBeTruthy();
    });

    it('should re-evaluate when a question set becomes valid and questionSetIsValid is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        formService.setQuestionSetValidity('ratingPrimary', false);
        let expression: Expression = new Expression(
            "questionSetIsValid('ratingPrimary')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        formService.setQuestionSetValidity('ratingPrimary', true);
        expressionInputSubjectService.getQuestionSetValidSubject('ratingPrimary').next(true);
        expect(result).toBeTruthy();
    });

    it('should re-evaluate when a question set becomes invalid and questionSetsIsInvalid is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        formService.setQuestionSetValidity('ratingPrimary', true);
        let expression: Expression = new Expression(
            "questionSetIsInvalid('ratingPrimary')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        formService.setQuestionSetValidity('ratingPrimary', false);
        expressionInputSubjectService.getQuestionSetValidSubject('ratingPrimary').next(true);
        expect(result).toBeTruthy();
    });

    it('should re-evaluate when a field becomes valid and fieldIsValid is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let fieldValidSubject: any = expressionInputSubjectService.getFieldValidSubject('myField', false);
        let expression: Expression = new Expression(
            "fieldIsValid('myField')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: boolean;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBeFalsy();
        fieldValidSubject.next(true);
        expect(result).toBeTruthy();
    });

    it('replaces sumRepeating(xxx, \'yyy\') with sum('
        + 'getFieldValuesForFieldPathPattern(\'xxx[*].yyy\')) before parsing an expression', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService = TestBed.get(
            ExpressionInputSubjectService);
        let expression: Expression = new Expression(
            "occupation == 'Other' || occupation == 'Miscellaneous' || (occupation != 'Miscellaneous' && "
                + "sumRepeating(acti_vity, 'perc_entage') == 100)",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.source).toBe(
            "occupation == 'Other' || occupation == 'Miscellaneous' || (occupation != 'Miscellaneous' && "
                + "sum(getFieldValuesForFieldPathPattern('acti_vity[*].perc_entage')) == 100)");
    });

    it('should sum up an array of field values', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let matchingFieldsSubjectService: MatchingFieldsSubjectService
            = TestBed.inject<MatchingFieldsSubjectService>(MatchingFieldsSubjectService);
        let eventService: EventService = TestBed.inject<EventService>(EventService);
        eventService.fieldPathAddedSubject.next('claims[0].amount');
        eventService.fieldPathAddedSubject.next('claims[1].amount');
        eventService.fieldPathAddedSubject.next('claims[2].amount');
        expressionInputSubjectService.getFieldValueSubject('claims[0].amount', 10);
        expressionInputSubjectService.getFieldValueSubject('claims[1].amount', 20);
        expressionInputSubjectService.getFieldValueSubject('claims[2].amount', 30);
        let expression: Expression = new Expression(
            "sum(getFieldValuesForFieldPathPattern('claims[*].amount'))",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe(60.00);
    });

    it('replaces countRepeating(xxx) with countRepeating(\'xxx\') before parsing an expression', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let expression: Expression = new Expression(
            "(occupation == 'Miscellaneous') ? (stringContains("
            + "miscellaneousOccupationPlLiability, miscellaneousLookup)? 1:0) "
            + ": (countRepeating(acti_vity)? sum(getFieldValuesForFieldPathPattern("
            + "'acti_vity[*].disablePublicLiability')):0)",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.source).toBe(
            "(occupation == 'Miscellaneous') ? (stringContains("
            + "miscellaneousOccupationPlLiability, miscellaneousLookup)? 1:0) "
            + ": (countRepeating('acti_vity')? sum(getFieldValuesForFieldPathPattern("
            + "'acti_vity[*].disablePublicLiability')):0)");
    });

    it('should count repeating question set quantities', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        expressionInputSubjectService.getFieldRepeatingCountSubject('claims', 5);
        let expression: Expression = new Expression(
            "countRepeating('claims')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        expect(expression.isConstant()).toBe(false);
        let result: number;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe(5);
    });

    it('should allow a field value from a repeating question set be accesible by field path', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService.getFieldValueSubject('claims[9].amount', 28596);
        let expression: Expression = new Expression(
            "claims[9].amount",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next(33123);
        expect(result).toBe(33123);
    });

    it('should allow a relative fieldPath to be resolved and used as a normal field value input', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService.getFieldValueSubject('claims[9].amount', 28596);
        let expression: Expression = new Expression(
            "getRelativeFieldValue(fieldPath, '1/amount')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing',
            { fieldPath: 'claims[9].date' });
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next(33123);
        expect(result).toBe(33123);
    });

    it('should allow a relative fieldPath to be resolved even when the fieldPath passed is a string literal, '
        + 'and then be resolved and used as a normal field value input', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService.getFieldValueSubject('claims[9].amount', 28596);
        let expression: Expression = new Expression(
            "getRelativeFieldValue('claims[9].date', '1/amount')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next(33123);
        expect(result).toBe(33123);
    });

    it('should allow a relative fieldPath to be resolved to another fieldPath', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let expression: Expression = new Expression(
            "getRelativeFieldPath(fieldPath, '1/amount')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing',
            { fieldPath: 'claims[9].date' });
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe('claims[9].amount');
    });

    it('should allow a relative fieldPath to be resolved even when the fieldPath passed is a string literal, '
        + 'and then be resolved as another fieldPath', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let expression: Expression = new Expression(
            "getRelativeFieldPath('claims[9].date', '1/amount')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe('claims[9].amount');
    });

    it('should allow a relative fieldPath to be resolved to another fieldPath and combined with '
        + 'getFieldPropertyValue', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: BehaviorSubject<any> = expressionInputSubjectService.getFieldValueSubject(
            'locations[9].address',
            '330 Collins St, Melbourne, VIC 3000');
        formService.setFieldData(
            'locations[9].addressSearch',
            {
                streetNumber: '330',
                street: 'Collins',
                streetType: 'St',
                city: 'Melbourne',
                state: 'VIC',
                postCode: 3000,
            });
        let expression: Expression = new Expression(
            "getFieldPropertyValue(getRelativeFieldPath(fieldPath, '1/addressSearch'), 'postCode')",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing',
            { fieldPath: 'locations[9].postCode' });
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toBe(3000);
    });

    it('replaces getRepeatingValue(fieldKey, \'xxx\') with getRelativeFieldValue(fieldPath, \'1/xxx\') '
        + 'before parsing an expression', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let expression: Expression = new Expression(
            "(occupation == 'Accountant' || occupation == 'Building Services' || occupation == 'Business' "
            + "|| occupation == 'Finance' || occupation == 'Insurance' || occupation == 'Real Estate' "
            + "|| occupation == 'Technology & Communications') ? occupation + ' - ' + "
            + "getRepeatingValue(fieldKey, 'selectedDiscipline') : ''",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing',
            { fieldPath: 'activities[0].percentage' });
        expect(expression.source).toBe(
            "(occupation == 'Accountant' || occupation == 'Building Services' || occupation == 'Business' "
            + "|| occupation == 'Finance' || occupation == 'Insurance' || occupation == 'Real Estate' "
            + "|| occupation == 'Technology & Communications') ? occupation + ' - ' + "
            + "getRelativeFieldValue(fieldPath, '1/selectedDiscipline') : ''");
    });

    it('it re-evaluates when a field value changes which is referenced by getFieldValuesWithTag', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let taggedFieldsSubjectService: TaggedFieldsSubjectService =
            TestBed.inject<TaggedFieldsSubjectService>(TaggedFieldsSubjectService);
        let configService: ConfigService = TestBed.inject<ConfigService>(ConfigService);
        let eventService: EventService = TestBed.inject<EventService>(EventService);
        let fakeConfigService: FakeConfigServiceForMetadata = <FakeConfigServiceForMetadata><any>configService;
        fakeConfigService.addTaggedMetadataResult(['test'], 20);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        eventService.fieldPathAddedSubject.next('fieldA');
        eventService.fieldPathAddedSubject.next('fieldB');
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldA', 'fieldAInitialValue');
        let fieldBSubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldB', 'fieldBInitialValue');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        spyOn(expressionDependencies.expressionMethodService, 'getFieldPathsWithTag')
            .and.returnValue(['fieldA', 'fieldB']);
        let expression: Expression = new Expression(
            "getFieldValuesWithTag('test')",
            expressionDependencies,
            'testing');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result[0]).toBe('fieldAInitialValue');
        expect(result[1]).toBe('fieldBInitialValue');
        fieldASubject.next('fieldAChangedValue');
        expect(result[0]).toBe('fieldAChangedValue');
        fieldBSubject.next('fieldBChangedValue');
        expect(result[1]).toBe('fieldBChangedValue');
    });

    it('it re-evaluates when a field value changes which is referenced by generateSummaryTableOfFieldsWithTag', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let taggedFieldsSubjectService: TaggedFieldsSubjectService
            = TestBed.inject<TaggedFieldsSubjectService>(TaggedFieldsSubjectService);
        let configService: ConfigService = TestBed.inject<ConfigService>(ConfigService);
        let eventService: EventService = TestBed.inject<EventService>(EventService);
        let fakeConfigService: FakeConfigServiceForMetadata = <FakeConfigServiceForMetadata><any>configService;
        fakeConfigService.addTaggedMetadataResult(['test'], 20);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        eventService.fieldPathAddedSubject.next('fieldA');
        eventService.fieldPathAddedSubject.next('fieldB');
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldA', 'fieldAInitialValue');
        let fieldBSubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldB', 'fieldBInitialValue');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        spyOn(expressionDependencies.expressionMethodService, 'getFieldPathsWithTag')
            .and.returnValue(['fieldA', 'fieldB']);
        let expression: Expression = new Expression(
            "generateSummaryTableOfFieldsWithTag('test', ['Property', 'Value'], '', true)",
            expressionDependencies,
            'testing');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result).toContain('fieldAInitialValue</td>');
        expect(result).toContain('fieldBInitialValue</td>');
        fieldASubject.next('fieldAChangedValue');
        expect(result).toContain('fieldAChangedValue</td>');
        fieldBSubject.next('fieldBChangedValue');
        expect(result).toContain('fieldBChangedValue</td>');
    });

    it('it re-evaluates when a field value changes which is referenced by generateSummaryTableOfFields', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldA', 'fieldAInitialValue');
        let fieldBSubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldB', 'fieldBInitialValue');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        let expression: Expression = new Expression(
            "generateSummaryTableOfFields(['fieldA', 'fieldB'])",
            expressionDependencies,
            'testing');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result).toContain('fieldAInitialValue</td>');
        expect(result).toContain('fieldBInitialValue</td>');
        fieldASubject.next('fieldAChangedValue');
        expect(result).toContain('fieldAChangedValue</td>');
        fieldBSubject.next('fieldBChangedValue');
        expect(result).toContain('fieldBChangedValue</td>');
    });

    it('it re-evaluates when a field value changes which is referenced by getFieldValues', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldA', 'fieldAInitialValue');
        let fieldBSubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldB', 'fieldBInitialValue');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        let expression: Expression = new Expression(
            "getFieldValues(['fieldA', 'fieldB'])",
            expressionDependencies,
            'testing');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result[0]).toBe('fieldAInitialValue');
        expect(result[1]).toBe('fieldBInitialValue');
        fieldASubject.next('fieldAChangedValue');
        expect(result[0]).toBe('fieldAChangedValue');
        fieldBSubject.next('fieldBChangedValue');
        expect(result[1]).toBe('fieldBChangedValue');
    });

    it('it re-evaluates when a field value changes which is referenced by getFieldValue', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldX', 'fieldXInitialValue');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        let expression: Expression = new Expression(
            "getFieldValue('fieldX')",
            expressionDependencies,
            'testing');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result).toBe('fieldXInitialValue');
        fieldASubject.next('fieldXChangedValue');
        expect(result).toBe('fieldXChangedValue');
    });

    it('it re-evaluates when a repeating question set changes and the expression references a repeating instance',
        () => {
            let formService: FormService = TestBed.inject<FormService>(FormService);
            let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
            let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('drivers', 'something');
            let fieldBSubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('drivers[0]', 'something');
            let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
            let expression: Expression = new Expression(
                "getFieldValue('drivers[0]')",
                expressionDependencies,
                'testing');
            let result: any = null;
            expression.nextResultObservable.subscribe((val: any) => result = val);
            expression.triggerEvaluation();
            expect(result).not.toBeNull();
            fieldBSubject.next('something2');
            result = 'asdf';
            fieldASubject.next('something2');
            expect(result).not.toBe('asdf');
        });

    it('it re-evaluates when a field value changes which is referenced by generateSummaryTable also using '
        + 'getFieldPathsWithTag and getFieldValuesWithTag', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let taggedFieldsSubjectService: TaggedFieldsSubjectService
            = TestBed.inject<TaggedFieldsSubjectService>(TaggedFieldsSubjectService);
        let configService: ConfigService = TestBed.inject<ConfigService>(ConfigService);
        let eventService: EventService = TestBed.inject<EventService>(EventService);
        let fakeConfigService: FakeConfigServiceForMetadata = <FakeConfigServiceForMetadata><any>configService;
        fakeConfigService.addTaggedMetadataResult(['test'], 20);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        eventService.fieldPathAddedSubject.next('fieldA');
        eventService.fieldPathAddedSubject.next('fieldB');
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldA', 'fieldAInitialValue');
        let fieldBSubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('fieldB', 'fieldBInitialValue');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        spyOn(expressionDependencies.expressionMethodService, 'getFieldPathsWithTag')
            .and.returnValue(['fieldA', 'fieldB']);
        let expression: Expression = new Expression(
            "generateSummaryTable([getFieldPathsWithTag('test'), getFieldValuesWithTag('test')], false)",
            expressionDependencies,
            'testing');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result).toContain('fieldAInitialValue</td>');
        expect(result).toContain('fieldBInitialValue</td>');
        fieldASubject.next('fieldAChangedValue');
        expect(result).toContain('fieldAChangedValue</td>');
        fieldBSubject.next('fieldBChangedValue');
        expect(result).toContain('fieldBChangedValue</td>');
    });

    it('it correctly parses an expression with double closing square bracket e.g. policyStartDate]]', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        let expression: Expression = new Expression(
            "generateSummaryTable([[ 'Address', 'myAddress'], [ 'Policy Start Date', policyStartDate]], true)",
            expressionDependencies,
            'testing');
        let act: () => void = () => expression.triggerEvaluation();
        expect(act).not.toThrow();
    });

    it('it correctly parses an expression with string concatenation within an array element', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        let expression: Expression = new Expression(
            "generateSummaryTable([[ 'Address', insuredAddress + '\n' + insuredTown + ' ' + insuredState + ' ' + "
            + "insuredPostcode], [ 'Policy Start Date', policyStartDate]], true, ['Property','Value'])",
            expressionDependencies,
            'testing');
        let act: () => void = () => expression.triggerEvaluation();
        expect(act).not.toThrow();
    });

    it('it correctly parses an expression with multiple regex patterns', () => {
        let expression: Expression = new Expression(
            "'The ' + stringReplace('little', /little/g, 'big') + ' ' + "
            + "stringReplace('banana', /banana/g, 'apple') + '.'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toEqual('The big apple.');
    });

    it('should allow the use of an object as an argument', () => {
        let expression: Expression = new Expression(
            "fixedValue.text + ' fox'",
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing',
            { fixedValue: { text: 'brown' } });
        expect(expression.isConstant()).toBe(true);
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        expression.triggerEvaluation();
        expect(result).toEqual('brown fox');
    });

    it('it re-evaluates when a field search term changes which is referenced by getFieldSearchTerm', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldSearchTermSubject('fieldX', 'toy');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        let expression: Expression = new Expression(
            "getFieldSearchTerm('fieldX')",
            expressionDependencies,
            'testing');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result).toBe('toy');
        fieldASubject.next('ota');
        expect(result).toBe('ota');
    });

    it('it scopes the field value to the repeating instance when the "this." prefix is used', () => {
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let fieldASubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('drivers[0].firstName', 'Danny');
        let fieldBSubject: BehaviorSubject<any>
            = expressionInputSubjectService.getFieldValueSubject('drivers[0].lastName', 'Masters');
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
        let expression: Expression = new Expression(
            "this.firstName + ' ' + this.lastName",
            expressionDependencies,
            'testing',
            null,
            null,
            'drivers[0]');
        let result: any = null;
        expression.nextResultObservable.subscribe((val: any) => result = val);
        expression.triggerEvaluation();
        expect(result).toBe('Danny Masters');
    });

    it('should evaluate an expression with multiple division', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService
            .getFieldValueSubject('startDate', '11/07/2023');
        expressionInputSubjectService
            .getFieldValueSubject('endDate', '11/07/2024');
        let expression: Expression = new Expression(
            'isDate(startDate) && isDate(endDate) && ' +
            '((date(endDate)/86400000) - (date(startDate)/86400000) < 731) && ' +
            '((date(endDate)/86400000) - (date(startDate)/86400000) > 0)',
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('11/07/2023');
        expect(result).toBe(true);
    });

    it('should evaluate an expression with regex pattern with extra spaces', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService
            .getFieldValueSubject('date', '11/07/2023');
        let expression: Expression = new Expression(
            'stringReplace(date + \'A1A2A3A4\',   /\\d/g   , \'*\')',
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('11/07/2024');
        expect(result).toBe('**/**/****A*A*A*A*');
    });

    it('should evaluate an expression with mix of multiple divisions and regex patterns', () => {
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let myTestFieldSubject: any = expressionInputSubjectService
            .getFieldValueSubject('startDate', '11/07/2023');
        expressionInputSubjectService
            .getFieldValueSubject('endDate', '11/07/2024');
        let expression: Expression = new Expression(
            'isDate(startDate) && isDate(endDate) && ' +
            // consecutive division
            '((date(endDate)/86400000) - (date(startDate)/86400000) < 731) && ' +
            // regex pattern
            'stringContains(\'A1A2A3A4\',/[0-9]/) && ' +
            // single division
            'date(startDate)/86400000 > 731 && ' +
            // consecutive regex pattern
            'stringContains(\'A1A2A3A4\', /[0-9]/) && ' +
            'stringContains(\'A1A2A3A4\',     /[0-9]/  ) && ' +
            // consecutive division
            '(date(endDate)/86400000) - (date(startDate)/86400000) > 0 && ' +
            // single regex pattern
            'stringReplace(endDate + \'A1A2A3A4\', /\\d/g, \'*\') == \'**/**/****A*A*A*A*\'',
            TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
            'testing');
        let result: any;
        expression.nextResultObservable.subscribe((r: any) => result = r);
        myTestFieldSubject.next('11/07/2023');
        expect(result).toBe(true);
    });
});
