import { Component, OnInit, SecurityContext, OnDestroy } from '@angular/core';
import { Subject, SubscriptionLike } from 'rxjs';
import { Field } from '../field';
import { FormService } from '@app/services/form.service';
import { WebhookService } from '@app/services/webhook.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ApplicationService } from '@app/services/application.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { debounceTime, finalize, takeUntil } from 'rxjs/operators';
import { DomSanitizer } from '@angular/platform-browser';
import { CalculationService } from '@app/services/calculation.service';
import { StringHelper } from '@app/helpers/string.helper';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { Expression } from '@app/expressions/expression';
import { QueryStringHelper } from '@app/helpers/query-string.helper';
import { WebhookFieldConfiguration } from '@app/resource-models/configuration/fields/webhook-field.configuration';
import { EventService } from '@app/services/event.service';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { LocaleService } from '@app/services/locale.service';

/**
 * Export webhook field component class.
 * TODO: Write a better class header: webhook field functions.
 */
@Component({
    selector: '' + FieldSelector.Webhook,
    templateUrl: './webhook.field.html',
})

export class WebhookField extends Field implements OnInit, OnDestroy {

    protected url: string;
    protected triggerExpressionValue: string;
    public loading: boolean = false;
    private executeWebhookSubject: Subject<void> = new Subject<void>();
    private requestPayload: string;
    private reloadingWebhookRequiresTrigger: boolean = true;
    private webhookRequestSubscription: SubscriptionLike;
    private webhookFieldConfiguration: WebhookFieldConfiguration;
    private urlExpression: Expression | TextWithExpressions;
    private conditionExpression: Expression;
    private triggerExpression: Expression;
    private payloadExpression: Expression;
    private requestsEnabled: boolean = false;
    protected allowCachingWithMaxAgeSeconds?: number;

    public constructor(
        protected application: ApplicationService,
        protected formService: FormService,
        protected webhookService: WebhookService,
        protected workflowService: WorkflowService,
        protected expressionDependencies: ExpressionDependencies,
        protected sanitizer: DomSanitizer,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        fieldEventLogRegistry: FieldEventLogRegistry,
        localeService: LocaleService,
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

        this.fieldType = FieldType.Webhook;
    }

    public ngOnInit(): void {
        this.webhookFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.allowCachingWithMaxAgeSeconds = this.webhookFieldConfiguration.allowCachingWithMaxAgeSeconds;
        super.ngOnInit();
    }

    protected initialiseField(): void {
        this.debounceWebhookRequests();
        super.initialiseField();
    }

    /**
     * We override this because we don't want any of the normal Field expressions.
     */
    protected setupExpressions(force: boolean = false): void {
        this.reloadingWebhookRequiresTrigger = this.webhookFieldConfiguration.triggerExpression != null;
        this.setupUrlExpression(force);
        this.setupPayloadExpression(force);
        this.setupConditionExpression(force);
        this.setupTriggerExpression(force);
    }

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(<string>this.field.key).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration }) => {
                this.webhookFieldConfiguration = <WebhookFieldConfiguration>configs.new;
                this.setupExpressions();
            });
    }

    public ngOnDestroy(): void {
        this.webhookRequestSubscription?.unsubscribe();
        this.executeWebhookSubject?.complete();
        super.ngOnDestroy();
    }

    protected destroyExpressions(): void {
        this.urlExpression?.dispose();
        this.urlExpression = null;
        this.conditionExpression?.dispose();
        this.conditionExpression = null;
        this.triggerExpression?.dispose();
        this.triggerExpression = null;
        this.payloadExpression?.dispose();
        this.payloadExpression = null;
        super.destroyExpressions();
    }

    private setupConditionExpression(force: boolean = false): void {
        if (this.conditionExpression) {
            if (!force && this.conditionExpression.source
                == this.webhookFieldConfiguration.conditionExpression
            ) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }
            this.conditionExpression.dispose();
            this.conditionExpression = null;
        }
        if (this.webhookFieldConfiguration.conditionExpression) {
            this.conditionExpression = new Expression(
                this.webhookFieldConfiguration.conditionExpression,
                this.expressionDependencies,
                this.fieldPath + ' webhook condition',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.conditionExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: boolean) => {
                    this.requestsEnabled = result;
                });
            this.conditionExpression.triggerEvaluation();
        } else {
            this.requestsEnabled = true;
        }
    }

    private setupTriggerExpression(force: boolean = false): void {
        if (this.triggerExpression) {
            if (!force && this.triggerExpression.source
                == this.webhookFieldConfiguration.triggerExpression
            ) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }
            this.triggerExpression.dispose();
            this.triggerExpression = null;
        }
        if (this.webhookFieldConfiguration.triggerExpression) {
            this.triggerExpression = new Expression(
                this.webhookFieldConfiguration.triggerExpression,
                this.expressionDependencies,
                this.fieldPath + ' webhook trigger',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.triggerExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: any) => {
                    if (this.requestsEnabled) {
                        this.executeWebhookSubject.next();
                    }
                });

            if (this.webhookFieldConfiguration.autoTrigger && !this.data) {
                this.triggerExpression.triggerEvaluation();
            }
        } else if (this.webhookFieldConfiguration.autoTrigger) {
            this.executeWebhookSubject.next();
        }
    }

    private setupUrlExpression(force: boolean = false): void {
        const urlExpressionSource: string = this.webhookFieldConfiguration.urlExpression;
        if (this.urlExpression) {
            if (!force && urlExpressionSource == this.urlExpression.source) {
                // don't bother recreating it if the url hasn't changed.
                return;
            }
            this.urlExpression.dispose();
            this.urlExpression = null;
        }
        const isText: boolean = urlExpressionSource.startsWith('http://')
            || urlExpressionSource.startsWith('https://')
            || (urlExpressionSource.indexOf('%{') != -1 && urlExpressionSource.indexOf('}%') != -1);
        if (isText) {
            this.urlExpression = new TextWithExpressions(
                urlExpressionSource,
                this.expressionDependencies,
                this.fieldKey + ' webhook url',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.urlExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => {
                    this.url = this.sanitizer.sanitize(SecurityContext.URL, result);
                    if (this.calculatedValueExpression) {
                        this.calculatedValueExpression.triggerEvaluation();
                    } else {
                        if (this.requestsEnabled && !this.reloadingWebhookRequiresTrigger) {
                            this.executeWebhookSubject.next();
                        }
                    }
                });
            this.urlExpression.triggerEvaluation();
        } else {
            this.urlExpression = new Expression(
                this.webhookFieldConfiguration.urlExpression,
                this.expressionDependencies,
                this.fieldKey + ' webhook url',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.urlExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => {
                    this.url = this.sanitizer.sanitize(SecurityContext.URL, result);
                    if (this.calculatedValueExpression) {
                        this.calculatedValueExpression.triggerEvaluation();
                    } else {
                        if (this.requestsEnabled && !this.reloadingWebhookRequiresTrigger) {
                            this.executeWebhookSubject.next();
                        }
                    }
                });
            this.urlExpression.triggerEvaluation();
        }
    }

    private setupPayloadExpression(force: boolean = false): void {
        if (this.payloadExpression) {
            if (!force && this.webhookFieldConfiguration.payloadExpression == this.payloadExpression.source
                && this.webhookFieldConfiguration.httpVerb != 'GET'
            ) {
                return;
            }
            this.payloadExpression.dispose();
            this.payloadExpression = null;
        }
        if (this.webhookFieldConfiguration.payloadExpression && this.webhookFieldConfiguration.httpVerb != 'GET') {
            this.payloadExpression = new Expression(
                this.webhookFieldConfiguration.payloadExpression,
                this.expressionDependencies,
                `${this.fieldKey} ${this.fieldType} webhook request payload`,
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.payloadExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => {
                    this.requestPayload = result;
                    if (this.requestsEnabled && !this.reloadingWebhookRequiresTrigger) {
                        this.executeWebhookSubject.next();
                    }
                });
            this.payloadExpression.triggerEvaluation();
        }
    }

    private debounceWebhookRequests(): void {
        this.executeWebhookSubject.pipe(
            debounceTime(this.webhookFieldConfiguration.debounceTimeMilliseconds),
            takeUntil(this.destroyed),
        )
            .subscribe(() =>{
                this.executeWebhookRequest();
            });
    }

    private executeWebhookRequest(): void {
        // Double check that we should still continue with this request, because there has been a debounce delay
        // and the calculated condition may no longer be true
        if (!this.requestsEnabled) {
            // the calculated condition is no longer true.
            return;
        }
        this.loading = true;
        if (this.webhookRequestSubscription) {
            this.webhookRequestSubscription.unsubscribe();
        }
        let body: any = null;
        let url: string = this.url;
        let httpVerb: string = this.webhookFieldConfiguration.httpVerb
            ? this.webhookFieldConfiguration.httpVerb
            : 'GET';
        if (StringHelper.equalsIgnoreCase(httpVerb, 'GET')) {
            if (!StringHelper.isNullOrEmpty(this.requestPayload)) {
                url += '?' + this.requestPayload;
            }
        } else {
            let payload: any = this.webhookFieldConfiguration.payloadExpression
                ? this.requestPayload
                : this.getDefaultPayload();
            body = payload;
            if (QueryStringHelper.isQueryString(body)) {
                body = QueryStringHelper.queryStringToJson(body);
            }
        }
        this.webhookRequestSubscription
            = this.webhookService
                .sendRequest(this.fieldPath, httpVerb, url, body, this.allowCachingWithMaxAgeSeconds)
                .pipe(
                    finalize(() => {
                        this.loading = false;
                    }),
                )
                .subscribe((data: any) => {
                    this.data = data;
                    if (this.webhookFieldConfiguration.autoPopulateFormModel) {
                        this.onModelUpdate(data);
                    }
                });
    }

    /**
     * The default payload is a the form model.
     * @returns 
     */
    protected getDefaultPayload(): object {
        return this.formService.getValues(true, true, false, false, true);
    }

    protected onModelUpdate(data: any): void {
        if (data) {
            let newValue: any = data.values || data;
            if (typeof (newValue) == 'object') {
                for (let key in newValue) {
                    if (this.form.controls[key]) {
                        const field: Field = this.form.controls[key]['field'];
                        if (field) {
                            field.setValue(newValue[key]);
                        }
                    }
                }
            }

            if (newValue != this.field.formControl.value) {
                this.setValue(newValue);
            }
        }
    }
}
