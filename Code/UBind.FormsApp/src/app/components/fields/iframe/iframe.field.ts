import {
    Component, OnInit, ViewChild,
    ComponentFactoryResolver, ViewContainerRef,
    SecurityContext,
    ChangeDetectorRef,
} from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Field } from '../field';
import { FormService } from '@app/services/form.service';
import { MessageService } from '@app/services/message.service';
import { ConfigService } from '@app/services/config.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ApplicationService } from '@app/services/application.service';
import { IqumulateContentComponent } from './iframes/iqumulate/iqumulate-content.component';
import { IqumulatePremiumFundingApiService } from '@app/services/api/iqumulate-premium-funding-api.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { CalculationService } from '@app/services/calculation.service';
import { EfundComponent } from './iframes/efund/efund.component';
import { GenericIframeComponent } from './generic-iframe.component';
import { Iframeable } from './iframeable';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { AbstractControl } from '@angular/forms';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { IframeFieldConfiguration } from '@app/resource-models/configuration/fields/iframe-field.configuration';
import { EventService } from '@app/services/event.service';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { takeUntil } from 'rxjs/operators';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * Export Iframe field component class
 * This class manage Iframe field functions.
 */
@Component({
    selector: '' + FieldSelector.Iframe,
    templateUrl: './iframe.field.html',
})

export class IframeField extends Field implements OnInit {
    @ViewChild('dynamicIframeContent', { read: ViewContainerRef, static: true })
    public dynamicIframeContent: ViewContainerRef;

    protected checkReadyIntervalMs: number = 100;
    protected url: SafeResourceUrl;
    protected iframeId: string;
    protected iframes: any;
    private baseUrl: string;
    public isCustom: boolean = false;

    // Iframeresizer
    protected iFrameResizerOptions: any = {
        checkOrigin: false,
        heightCalculationMethod: 'taggedElement',
        log: false, // Enable console logging,
        scrolling: false,
    };

    protected messageSubscription: any;
    private iframeFieldFieldConfiguration: IframeFieldConfiguration;
    private dynamicComponent: Iframeable;

    public constructor(
        protected sanitizer: DomSanitizer,
        protected messageService: MessageService,
        protected application: ApplicationService,
        protected config: ConfigService,
        protected formService: FormService,
        protected workflowService: WorkflowService,
        protected iqumulateFunding: IqumulatePremiumFundingApiService,
        private componentFactoryResolver: ComponentFactoryResolver,
        protected expressionDependencies: ExpressionDependencies,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        private changeDetectorRef: ChangeDetectorRef,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super(
            formService,
            workflowService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry);
        this.fieldType = FieldType.Iframe;
    }

    public ngOnInit(): void {
        this.iframeFieldFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
    }

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(<string>this.field.key).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                let oldConfig: IframeFieldConfiguration = <IframeFieldConfiguration>configs.old;
                this.iframeFieldFieldConfiguration = <IframeFieldConfiguration>configs.new;
                if (oldConfig.urlExpression != this.iframeFieldFieldConfiguration.urlExpression) {
                    this.generateUrl();
                    this.dynamicComponent.url = this.url;
                    this.changeDetectorRef.markForCheck();
                }
            });
    }

    public setHidden(hidden: boolean): void {
        if (this.hidden && !hidden) {
            this.onShowIframe();
        } else if (!this.hidden && hidden) {
            this.onHideIframe();
        }
        super.setHidden(hidden);
    }

    protected initialiseField(): void {
        super.initialiseField();

        this.setFieldValue(WorkflowRole.PremiumFinanceAcceptanceConfirmation, '', 'acceptanceConfirmation');
        this.setFieldValue('fundingPage', '');
        if (!this.hidden) {
            this.onShowIframe();
        }
    }

    protected onShowIframe(): void {
        this.generateUrl();
        if (this.isBaseUrlForIqumulate) {
            this.initializeIframeType(IqumulateContentComponent);
        } else if (this.isBaseUrlForEfundExpress) {
            this.initializeIframeType(EfundComponent);
        } else {
            this.initializeIframeType(GenericIframeComponent);
        }
    }

    private generateUrl(): void {
        let fieldValueURLAppendix: string = '';
        if (this.formControl.value != null && this.formControl.value != '') {
            fieldValueURLAppendix = this.formControl.value;
        } else {
            fieldValueURLAppendix = '{}';
        }
        let unsafeUrl: string = new TextWithExpressions(
            this.iframeFieldFieldConfiguration.urlExpression,
            this.expressionDependencies,
            this.fieldKey + ' iframe placeholder url',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope).evaluate();
        this.baseUrl = this.sanitizer.sanitize(SecurityContext.URL, unsafeUrl);
        this.url = this.sanitizer.bypassSecurityTrustResourceUrl(this.baseUrl + '?fieldKey='
            + this.fieldKey + '&fieldValue=' + fieldValueURLAppendix);
    }

    protected onHideIframe(): void {
        // this can be overridden by subclasses to respond when the iframe has been hidden
    }

    private get isBaseUrlForIqumulate(): boolean {
        return this.baseUrl == 'IQumulateMPFIframe';
    }

    private get isBaseUrlForEfundExpress(): boolean {
        return (/redplanetsoftware/i).test(this.baseUrl) || (/brokers.principal/i).test(this.baseUrl);
    }

    private initializeIframeType(component: any): any {
        const componentFactory: any = this.componentFactoryResolver.resolveComponentFactory(component);
        this.dynamicIframeContent.clear();
        this.dynamicComponent = <Iframeable>this.dynamicIframeContent.createComponent(
            componentFactory).instance;
        this.dynamicComponent.facade = this;
        this.dynamicComponent.url = this.url;
        if (component === IqumulateContentComponent) {
            (this.dynamicComponent as IqumulateContentComponent)
                .fundingAccepted.subscribe(
                    (premiumFundingResponseData: any) => this.handleDataReceived(premiumFundingResponseData));
        }
        return component;
    }

    private setFieldValue(workflowRole: string, value: any, fallbackWorkflowRole?: string): void {
        const fieldName: any = this.config.workflowRoles[workflowRole] || fallbackWorkflowRole || workflowRole;
        const field: AbstractControl = this.form.controls[fieldName];
        if (field) {
            field.setValue(value);
        }
    }

    private handleDataReceived(premiumFundingResponseData: any): void {
        this.data = premiumFundingResponseData;
    }
}
