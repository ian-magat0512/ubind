import { Component, ElementRef, OnInit } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { OptionsField } from '../options/options.field';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { WebhookService } from '@app/services/webhook.service';
import { DomSanitizer } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { EventService } from '@app/services/event.service';
import { ConfigService } from '@app/services/config.service';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';

/**
 * Export select field component class.
 * This class manage selection field compoenent functions.
 */
@Component({
    selector: '' + FieldSelector.DropDownSelect,
    templateUrl: './select.field.html',
    styleUrls: ['./select.field.scss'],
})
export class SelectField extends OptionsField implements OnInit {

    protected selectFieldNativeElement: HTMLElement;

    public constructor(
        public elementRef: ElementRef,
        formService: FormService,
        webhookService: WebhookService,
        workflowService: WorkflowService,
        expressionDependencies: ExpressionDependencies,
        sanitizer: DomSanitizer,
        httpClient: HttpClient,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        configService: ConfigService,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super(
            formService,
            webhookService,
            workflowService,
            expressionDependencies,
            sanitizer,
            httpClient,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            configService,
            fieldEventLogRegistry,
        );
        this.fieldType = FieldType.DropDownSelect;
    }

    public ngOnInit(): void {
        super.ngOnInit();

        // if there is no value, set the value to an empty string so that we can show the placeholder option
        if (this.form.controls[this.fieldKey].value == null) {
            this.form.controls[this.fieldKey].setValue('');
        }

        this.selectFieldNativeElement = this.elementRef.nativeElement;
    }

    protected onClick(e: any, disabled: any): void {
        if (disabled) {
            e.preventDefault();
        }
    }
}
