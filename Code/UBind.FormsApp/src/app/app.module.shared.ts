// export declare var app_base_url: "/";

import { NgModule, ErrorHandler } from '@angular/core';
import { APP_BASE_HREF, DatePipe } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormlyModule } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { JsonResponseValidationInterceptor } from './interceptors/JsonResponseValidationInterceptor';

import { CurrencyPipe } from './pipes/currency.pipe';
import { TimePipe } from './pipes/time.pipe';
import { PhoneNumberPipe } from './pipes/phone-number.pipe';
import { NumberPlatePipe } from './pipes/number-plate.pipe';
import { CreditCardNumberPipe } from './pipes/credit-card-number.pipe';
import { AbnPipe } from './pipes/abn.pipe';
import { BsbPipe } from './pipes/bsb.pipe';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';

import { FormatTextInputDirective } from './directives/format-text-input.directive';

import { ContainerWrapper } from './components/wrappers/container/container.wrapper';
import { TooltipWrapper } from './components/wrappers/tooltip/tooltip.wrapper';
import { ContentWrapper } from './components/wrappers/content/content.wrapper';
import { QuestionWrapper } from './components/wrappers/question/question.wrapper';
import { HideWrapper } from './components/wrappers/hide/hide.wrapper';
import { LabelWrapper } from './components/wrappers/label/label.wrapper';
import { AnchorWrapper } from './components/wrappers/anchor/anchor.wrapper';
import { AddonsWrapper } from './components/wrappers/addons/addons.wrapper';
import { FieldsetWrapper } from './components/wrappers/fieldset/fieldset.wrapper';

import { ContainerTemplate } from './components/wrappers/container/container.template';
import { ContentTemplate } from './components/wrappers/content/content.template';
import { DescriptionTemplate } from './components/wrappers/description/description.template';
import { LabelTemplate } from './components/wrappers/label/label.template';
import { LabelTooltipTemplate } from './components/wrappers/tooltip/label-tooltip.template';
import { QuestionTemplate } from './components/wrappers/question/question.template';

import { WebFormComponent } from './components/web-form/web-form';

import { SectionWidget } from './components/widgets/section/section.widget';
import { ArticleWidget } from './components/widgets/article/article.widget';
import { QuestionsWidget } from './components/widgets/questions/questions.widget';
import { SidebarWidget } from './components/widgets/sidebar/sidebar.widget';
import { CalculationWidget } from './components/widgets/calculation/calculation.widget';
import { ProgressWidget } from './components/widgets/progress/progress.widget';
import { HeadingWidget } from './components/widgets/heading/heading.widget';
import { ActionsWidget } from './components/widgets/actions/actions.widget';
import { ContentWidget } from './components/widgets/content/content.widget';
import { AsideWidget } from './components/widgets/aside/aside.widget';
import { AlertWidget } from './components/widgets/alert/alert.widget';

import { RepeatingField } from './components/fields/repeating/repeating.field';
import { ButtonsField } from './components/fields/buttons/buttons.field';
import { ToggleField } from './components/fields/toggle/toggle.field';
import { DatepickerField } from './components/fields/datepicker/datepicker.field';
import { SelectField } from './components/fields/select/select.field';
import { RadioField } from './components/fields/radio/radio.field';
import { ContentField } from './components/fields/content/content.field';
import { TextareaField } from './components/fields/textarea/textarea.field';
import { CheckboxField } from './components/fields/checkbox/checkbox.field';
import { PasswordField } from './components/fields/password/password.field';
import { HiddenField } from './components/fields/hidden/hidden.field';
import { IframeField } from './components/fields/iframe/iframe.field';
import { WebhookField } from './components/fields/webhook/webhook.field';
import { AttachmentField } from './components/fields/attachment/attachment.field';
import { CurrencyField } from './components/fields/currency/currency.field';
import { SearchSelectField } from './components/fields/search-select/search-select.field';
import { SingleLineTextField } from './components/fields/single-line-text/single-line-text.field';

import { IqumulateContentComponent } from './components/fields/iframe/iframes/iqumulate/iqumulate-content.component';
import { GenericIframeComponent } from './components/fields/iframe/generic-iframe.component';
import { EfundComponent } from './components/fields/iframe/iframes/efund/efund.component';
import { SvgLoaderComponent } from './components/svg-loader/svg-loader.component';
import { FieldDebugComponent } from './components/fields/debug/field-debug';
import { OptionsFieldDebugComponent } from './components/fields/debug/options/options-field-debug';
import { ExpressionRegistryComponent } from './components/debug/expression-registry/expression-registry.component';
import { IQumulateMpfComponent } from './components/fields/iframe/iframes/iqumulate/iqumulate-mpf.component';
import { ExpressionWatchComponent } from './components/debug/expression-watch/expression-watch.component';
import { ApngLoaderComponent } from './components/apng-loader/apng-loader.component';
import { WorkbookToolsComponent } from './components/debug/workbook-tools/workbook-tools.component';
import { WorkflowToolsComponent } from './components/debug/workflow-tools/workflow-tools.component';

import { ApplicationService } from './services/application.service';
import { MessageService } from './services/message.service';
import { ApiService } from './services/api.service';
import { ConfigService } from './services/config.service';
import { ConfigProcessorService } from './services/config-processor.service';
import { WorkflowService } from './services/workflow.service';
import { FormService } from './services/form.service';
import { WindowScrollService } from './services/window-scroll.service';
import { ValidationService } from './services/validation.service';
import { validationMessages } from './services/validation-messages';
import { OperationInstructionService } from './services/operation-instruction.service';
import { CalculationService } from './services/calculation.service';
import { ExpressionMethodService } from './expressions/expression-method.service';
import { EvaluateService } from './services/evaluate.service';
import { AppEventService } from './services/app-event.service';
import { LoggerService } from './services/logger.service';
import { AttachmentService } from './services/attachment.service';
import { WebhookService } from './services/webhook.service';
import { CssProcessorService } from './services/css-processor.service';
import { StripeTokenService } from './services/stripe-token/stripe-token.service';
import { ApplicationStartupService } from './services/application-startup.service';
import { BrowserDetectionService } from './services/browser-detection.service';
import { QuoteApiService } from './services/api/quote-api.service';
import { ToolTipService } from './services/tooltip.service';
import { ResumeApplicationService } from './services/resume-application.service';
import { QuoteResultProcessor } from './services/quote-result-processor';
import { ClaimResultProcessor } from './services/claim-result-processor';
import { FormEventCallbackService } from './services/form-event-callback.service';

// operations
import { ConfigurationOperation } from './operations/configuration.operation';
import { FormUpdateOperation } from './operations/form-update.operation';
import { CalculationOperation } from './operations/calculation.operation';
import { LoadOperation } from './operations/load.operation';
import { SubmissionOperation } from './operations/submission.operation';
import { EnquiryOperation } from './operations/enquiry.operation';
import { BindOperation } from './operations/bind.operation';
import { CreditCardPaymentOperation } from './operations/credit-card-payment.operation';
import { PremiumFundingProposalAndAcceptanceOperation } from './operations/premium-funding.operation';
import { StripePaymentOperation } from './operations/stripe-payment.operation';
import { SaveOperation } from './operations/save.operation';
import { CustomerOperation } from './operations/customer.operation';
import { QuoteVersionOperation } from './operations/quote-version.operation';
import { PolicyOperation } from './operations/policy.operation';
import { InvoiceOperation } from './operations/invoice.operation';
import { AttachmentOperation } from './operations/attachment.operation';
import { AutoApprovalOperation } from './operations/auto-approve.operation';
import { DeclineOperation } from './operations/decline.operation';
import { EndorsementApprovalOperation } from './operations/endorsement-approval.operation';
import { EndorsementReferralOperation } from './operations/endorsement-referral.operation';
import { ReturnOperation } from './operations/return.operation';
import { ReviewApprovalOperation } from './operations/review-approval.operation';
import { ReviewReferralOperation } from './operations/review-referral.operation';
import { AcknowledgeOperation } from './operations/acknowledge.operation';
import { ActualiseOperation } from './operations/actualise.operation';
import { AssessmentApprovalOperation } from './operations/assessment-approval.operation';
import { AssessmentReferralOperation } from './operations/assessment-referral.operation';
import { NotifyOperation } from './operations/notify.operation';
import { SettleOperation } from './operations/settle.operation';
import { WithdrawOperation } from './operations/withdraw.operation';
import { ClaimVersionOperation } from './operations/claim-version.operation';
import { CopyExpiredQuoteOperation } from './operations/copy-expired-quote.operation';
import { WorkflowStepOperation } from './operations/workflow-step.operation';
import { CustomerUserOperation } from './operations/customer-user.operation';
import { GetCustomerOperation } from './operations/get-customer.operation';

import { OperationFactory } from './operations/operation.factory';
import { IqumulatePremiumFundingApiService } from './services/api/iqumulate-premium-funding-api.service';

import { AppComponent } from './components/app/app.component';
import { BroadcastService } from './services/broadcast.service';
import { UserService } from './services/user.service';
import { AlertService } from './services/alert.service';
import { AppContextApiService } from './services/api/app-context-api.service';

import { ValidationWrapper } from './components/wrappers/validation/validation.wrapper';
import { ResilientStorage } from './storage/resilient-storage';
import { GlobalErrorHandler } from './providers/global-error-handler';
import { NgxFilesizeModule } from 'ngx-filesize';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import {
    FormTypeApplicationPropertiesResolver,
} from './operations/operation-form-type-application-properties-resolver';
import { FieldSelector } from './models/field-selectors.enum';
import { AuthenticationService } from './services/authentication.service';
import { ClaimApiService } from './services/api/claim-api-service';
import { NotificationWidget } from './components/widgets/notification/notification.widget';
import { NotificationService } from './services/notification.service';
import { DescriptionWrapper } from './components/wrappers/description/description.wrapper';
import { ValidationTemplate } from './components/wrappers/validation/validation.template';
import { ConfigurationV2Processor } from './services/configuration-v2-processor';
import { EncryptionService } from "@app/services/encryption.service";
import { CssIdentifierPipe } from './pipes/css-identifier.pipe';
import { RevealGroupTrackingService } from './services/reveal-group-tracking.service';
import { QuestionTooltipTemplate } from './components/wrappers/tooltip/question-tooltip.template';
import { NgxSliderModule } from '@angular-slider/ngx-slider';
import { SliderField } from './components/fields/slider/slider.field';
import { FieldEventLogComponent } from './components/debug/field-event-log/field-event-log.component';
import { ActionWidget } from './components/widgets/action/action.widget';
import { HeaderWidget } from './components/widgets/header/header.widget';
import { FooterWidget } from './components/widgets/footer/footer.widget';
import { PriceWidget } from './components/widgets/price/price.widget';
import { TooltipWidget } from './components/widgets/tooltip/tooltip.widget';
import { NgxMaskModule } from 'ngx-mask';
import { MaskPipe } from 'ngx-mask';

export const sharedConfig: NgModule = {
    bootstrap: [AppComponent],
    declarations: [
        // Pipes
        CurrencyPipe,
        TimePipe,
        PhoneNumberPipe,
        AbnPipe,
        BsbPipe,
        CreditCardNumberPipe,
        NumberPlatePipe,
        SafeHtmlPipe,
        CssIdentifierPipe,
        // Directives
        FormatTextInputDirective,
        // Wrappers
        ContainerWrapper,
        TooltipWrapper,
        ContentWrapper,
        DescriptionWrapper,
        QuestionWrapper,
        HideWrapper,
        LabelWrapper,
        AnchorWrapper,
        FieldsetWrapper,
        AddonsWrapper,
        ValidationWrapper,
        // Components
        AppComponent,
        WebFormComponent,
        SectionWidget,
        ArticleWidget,
        QuestionsWidget,
        SidebarWidget,
        CalculationWidget,
        ProgressWidget,
        HeadingWidget,
        ActionsWidget,
        ActionWidget,
        TooltipWidget,
        ContentWidget,
        AsideWidget,
        HeaderWidget,
        FooterWidget,
        PriceWidget,
        AlertWidget,
        NotificationWidget,
        ApngLoaderComponent,
        SvgLoaderComponent,
        // Field Types
        RepeatingField,
        ButtonsField,
        ToggleField,
        DatepickerField,
        SingleLineTextField,
        CurrencyField,
        SelectField,
        SliderField,
        RadioField,
        ContentField,
        TextareaField,
        CheckboxField,
        PasswordField,
        HiddenField,
        IframeField,
        GenericIframeComponent,
        IqumulateContentComponent,
        EfundComponent,
        WebhookField,
        AttachmentField,
        SearchSelectField,
        IQumulateMpfComponent,
        FieldDebugComponent,
        OptionsFieldDebugComponent,
        ExpressionRegistryComponent,
        ExpressionWatchComponent,
        WorkbookToolsComponent,
        WorkflowToolsComponent,
        FieldEventLogComponent,
    ],
    imports: [
        HttpClientModule,
        FormlyModule.forRoot({
            types: [
                {
                    name: FieldSelector.Repeating,
                    component: RepeatingField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Buttons,
                    component: ButtonsField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Toggle,
                    component: ToggleField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.DatePicker,
                    component: DatepickerField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.SingleLineText,
                    component: SingleLineTextField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Currency,
                    component: CurrencyField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.DropDownSelect,
                    component: SelectField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Radio,
                    component: RadioField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.TextArea,
                    component: TextareaField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Password,
                    component: PasswordField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Checkbox,
                    component: CheckboxField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Content,
                    component: ContentField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Hidden,
                    component: HiddenField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Iframe,
                    component: IframeField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Webhook,
                    component: WebhookField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Attachment,
                    component: AttachmentField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.SearchSelect,
                    component: SearchSelectField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
                {
                    name: FieldSelector.Slider,
                    component: SliderField,
                    wrappers: ['anchor', 'hide', 'fieldset'],
                    defaultOptions: {
                        templateOptions: {
                            descriptionWrapper: true,
                            validationWrapper: true,
                        },
                    },
                },
            ],
            wrappers: [
                { name: 'tooltip', component: TooltipWrapper },
                { name: 'hide', component: HideWrapper },
                { name: 'content', component: ContentWrapper },
                { name: 'label', component: LabelWrapper },
                { name: 'question', component: QuestionWrapper },
                { name: 'anchor', component: AnchorWrapper },
                { name: 'container', component: ContainerWrapper },
                { name: 'addons', component: AddonsWrapper },
                { name: 'fieldset', component: FieldsetWrapper },
                { name: 'description', component: DescriptionWrapper },
                { name: 'validation', component: ValidationWrapper },
            ],
            manipulators: [
                { class: ValidationTemplate, method: 'run' },
                { class: DescriptionTemplate, method: 'run' },
                { class: ContainerTemplate, method: 'run' },
                { class: QuestionTooltipTemplate, method: 'run' },
                { class: QuestionTemplate, method: 'run' },
                { class: LabelTooltipTemplate, method: 'run' },
                { class: LabelTemplate, method: 'run' },
                { class: ContentTemplate, method: 'run' },
            ],
            validationMessages: validationMessages(),
        }),
        FormlyBootstrapModule,
        FormsModule,
        ReactiveFormsModule,
        NgxFilesizeModule,
        MatExpansionModule,
        MatInputModule,
        MatListModule,
        MatIconModule,
        NgxJsonViewerModule,
        NgxSliderModule,
        NgxMaskModule.forRoot(),
    ],
    providers: [
        { provide: APP_BASE_HREF, useValue: '/' },
        // Interceptors
        { provide: HTTP_INTERCEPTORS, useClass: JsonResponseValidationInterceptor, multi: true },
        { provide: ErrorHandler, useClass: GlobalErrorHandler },
        // Pipes
        AbnPipe,
        BsbPipe,
        MaskPipe,
        CreditCardNumberPipe,
        CurrencyPipe,
        TimePipe,
        DatePipe,
        PhoneNumberPipe,
        NumberPlatePipe,
        SafeHtmlPipe,
        CssIdentifierPipe,
        // Services
        ApplicationService,
        MessageService,
        ApiService,
        ConfigService,
        ConfigProcessorService,
        ConfigurationV2Processor,
        QuoteApiService,
        ClaimApiService,
        WorkflowService,
        FormService,
        ValidationService,
        OperationInstructionService,
        CalculationService,
        WindowScrollService,
        EvaluateService,
        ExpressionMethodService,
        AppEventService,
        AttachmentService,
        WebhookService,
        CssProcessorService,
        StripeTokenService,
        ApplicationStartupService,
        BroadcastService,
        BrowserDetectionService,
        RevealGroupTrackingService,
        UserService,
        AlertService,
        NotificationService,
        LoggerService,
        ResumeApplicationService,
        ToolTipService,
        AuthenticationService,
        AppContextApiService,
        QuoteResultProcessor,
        ClaimResultProcessor,
        FormEventCallbackService,

        // Operations
        ConfigurationOperation,
        FormUpdateOperation,
        CopyExpiredQuoteOperation,
        CalculationOperation,
        LoadOperation,
        SubmissionOperation,
        EnquiryOperation,
        BindOperation,
        CreditCardPaymentOperation,
        PremiumFundingProposalAndAcceptanceOperation,
        StripePaymentOperation,
        SaveOperation,
        CustomerOperation,
        PolicyOperation,
        InvoiceOperation,
        QuoteVersionOperation,
        AttachmentOperation,
        WorkflowStepOperation,
        CustomerUserOperation,
        GetCustomerOperation,
        AutoApprovalOperation,
        DeclineOperation,
        EndorsementApprovalOperation,
        EndorsementReferralOperation,
        ReturnOperation,
        ReviewApprovalOperation,
        ReviewReferralOperation,
        AcknowledgeOperation,
        AssessmentApprovalOperation,
        AssessmentReferralOperation,
        ActualiseOperation,
        SettleOperation,
        NotifyOperation,
        WithdrawOperation,
        ClaimVersionOperation,

        // Services
        ResilientStorage,
        IqumulatePremiumFundingApiService,
        OperationFactory,

        // Helpers
        FormTypeApplicationPropertiesResolver,
        EncryptionService,
    ],
};
