import { OnInit, SecurityContext, Directive, OnDestroy } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { Expression, FixedArguments, ObservableArguments } from '@app/expressions/expression';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { CalculationService } from '@app/services/calculation.service';
import { FormService } from '@app/services/form.service';
import { WebhookService } from '@app/services/webhook.service';
import { WorkflowService } from '@app/services/workflow.service';
import { Subject, SubscriptionLike } from 'rxjs';
import { debounceTime, distinctUntilChanged, finalize, takeUntil } from 'rxjs/operators';
import { SelectOption } from './select-option';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Errors } from '@app/models/errors';
import { StringHelper } from '@app/helpers/string.helper';
import * as _ from 'lodash-es';
import { QueryStringHelper } from '@app/helpers/query-string.helper';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { AbstractControl } from '@angular/forms';
import { OptionsFieldConfiguration } from '@app/resource-models/configuration/fields/options-field.configuration';
import { EventService } from '@app/services/event.service';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { ArrayHelper, ArraySyncStats } from '@app/helpers/array.helper';
import { OptionConfiguration } from '@app/resource-models/configuration/option.configuration';
import { ConfigService } from '@app/services/config.service';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { KeyboardInputField } from '../keyboard-Input-field';

/**
 * local-only structure for storing settings for API Requests
 */
interface ApiRequestSettings {
    payload: string;
    url: string;
    httpVerb: string;
}

/**
 * Represents a type of field that has options to choose from.
 *
 * E.g. a select droplist, radio buttons, a set of icon buttons, or a search select field
 */
@Directive()
export abstract class OptionsField extends KeyboardInputField implements OnInit, OnDestroy {

    public doesGetOptionsFromApi: boolean = false;
    public doesGetSelectedOptionFromApi: boolean = false;
    public isLoading: boolean = false;
    public errorMessage: string;
    private canReloadOptions: boolean = true;
    private reloadingOptionsRequiresTrigger: boolean = false;
    private loadOptionsSubject: Subject<void> = new Subject<void>();
    private debounceOptionsRequestSubscription: SubscriptionLike;
    private optionsRequestSubscription: SubscriptionLike;
    protected sourceItems: Array<OptionConfiguration>;
    public hideAllOptions: boolean = false;
    protected readonly DefaultNoOptionsFoundText: string = 'No options found';
    public noOptionsFoundText: string = this.DefaultNoOptionsFoundText;
    public loadingText: string = "Loading...";
    public numberOfSelectOptionsShown: number = 0;
    private loadingSelectedOptionFromApi: boolean = false;
    protected activeApiCalls: number = 0;
    protected noOptionsFoundTextExpression: TextWithExpressions;
    protected hideAllOptionsConditionExpression: Expression;
    protected optionsRequestConditionExpression: Expression;
    protected optionsRequestTriggerExpression: Expression;
    protected selectedOptionRequestUrlExpression: Expression;
    protected selectedOptionRequestPayloadExpression: Expression;
    protected searchTextExpression: Expression;
    public optionsFieldConfiguration: OptionsFieldConfiguration;
    private calculatedValueSelectedOptionSubscription: SubscriptionLike;
    private optionSetChangesSubscription: SubscriptionLike;
    protected optionsRequestDebounceTimeMilliseconds: number;
    protected cacheOptionsRequestWithMaxAgeSeconds?: number;
    protected cacheSelectedOptionRequestWithMaxAgeSeconds?: number;
    private hasLoadedOptionsFromApi: boolean = false;
    private isSelectedOptionCacheEnabled: boolean = false;

    /**
     * A name for the option set, typically used in the html "name" attribute.
     */
    public groupName: string;

    public optionsRequestSettings: ApiRequestSettings = <ApiRequestSettings>{
        payload: null,
        url: null,
        httpVerb: null,
    };

    public selectedOptionRequestSettings: ApiRequestSettings = <ApiRequestSettings>{
        payload: null,
        url: null,
        httpVerb: null,
    };

    public optionsRequestConditionBeingUsed: boolean = false;
    public optionsRequestConditionValue: boolean = null;

    private optionSetKey: string;
    public selectOptions: Array<SelectOption> = new Array<SelectOption>();

    public constructor(
        protected formService: FormService,
        protected webhookService: WebhookService,
        protected workflowService: WorkflowService,
        protected expressionDependencies: ExpressionDependencies,
        protected sanitizer: DomSanitizer,
        protected httpClient: HttpClient,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        protected eventService: EventService,
        protected configService: ConfigService,
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
    }

    public ngOnInit(): void {
        this.optionsFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.groupName = this.fieldPath;
    }

    public ngOnDestroy(): void {
        this.loadOptionsSubject?.complete();
        this.debounceOptionsRequestSubscription?.unsubscribe();
        this.optionsRequestSubscription?.unsubscribe();
        this.calculatedValueSelectedOptionSubscription?.unsubscribe();
        this.optionSetChangesSubscription?.unsubscribe();
        super.ngOnDestroy();
    }

    protected destroyExpressions(): void {
        this.noOptionsFoundTextExpression?.dispose();
        this.noOptionsFoundTextExpression = null;
        this.hideAllOptionsConditionExpression?.dispose();
        this.hideAllOptionsConditionExpression = null;
        if (this.doesGetSelectedOptionFromApi) {
            this.selectedOptionRequestUrlExpression?.dispose();
            this.selectedOptionRequestUrlExpression = null;
            this.selectedOptionRequestPayloadExpression?.dispose();
            this.selectedOptionRequestPayloadExpression = null;
        }
        if (this.doesGetOptionsFromApi) {
            this.optionsRequestConditionExpression?.dispose();
            this.optionsRequestConditionExpression = null;
            this.optionsRequestTriggerExpression?.dispose();
            this.optionsRequestTriggerExpression = null;
        }
        this.searchTextExpression?.dispose();
        this.searchTextExpression = null;
        super.destroyExpressions();
    }

    protected initialiseField(): void {
        this.initialisationStarted = true;
        this.updateFieldConfigurationForBackwardsCompatibility();
        this.applyConfigurationDefaults();
        this.debounceOptionsRequests();
        super.initialiseField();
        this.initialised = false;
        this.initialiseOptions();
    }

    protected initialiseOptions(): void {
        this.isLoading = this.doesGetSelectedOptionFromApi && !StringHelper.isNullOrEmpty(this.initialValue);
        if (!this.doesGetOptionsFromApi) {
            if (!this.optionsFieldConfiguration.optionSetKey) {
                throw Errors.Product.NoOptionsDefined(this.fieldPath, this.fieldType);
            }
            this.sourceItems = this.configService.getOptions(this.optionsFieldConfiguration.optionSetKey);
            this.generateItems();
            this.listenToOptionSetChanges();
        }
        this.hasLoadedOptionsFromApi = false;
        this.loadDataFromApiIfRequired();
    }

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(<string>this.field.key).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration }) => {
                this.optionsFieldConfiguration = <OptionsFieldConfiguration>configs.new;
                this.updateFieldConfigurationForBackwardsCompatibility();
                this.applyConfigurationDefaults();
                this.debounceOptionsRequests();
                this.updateOptions();
            });
    }

    protected updateOptions(): void {
        if (!this.doesGetOptionsFromApi) {
            if (!this.optionsFieldConfiguration.optionSetKey) {
                throw Errors.Product.NoOptionsDefined(this.fieldPath, this.fieldType);
            }
            if (this.optionsFieldConfiguration.optionSetKey != this.optionSetKey) {
                this.sourceItems = this.configService.getOptions(this.optionsFieldConfiguration.optionSetKey);
                this.generateItems();
                this.listenToOptionSetChanges();
            }
        }
        this.hasLoadedOptionsFromApi = false;
        this.loadDataFromApiIfRequired();
    }

    private listenToOptionSetChanges(): void {
        if (this.optionSetChangesSubscription) {
            this.optionSetChangesSubscription.unsubscribe();
        }
        this.optionSetChangesSubscription
            = this.eventService.getOptionSetUpdatedObservable(this.optionSetKey).pipe(takeUntil(this.destroyed))
                .subscribe((options: Array<OptionConfiguration>) => {
                    this.sourceItems = options;
                    this.generateItems();
                });
    }

    /**
     * Previously 'optionsRequest' was called 'searchRequest'
     * and 'selectedOptionRequest' was called 'lookupRequest'.
     *
     * Also 'verb' was renamed to 'httpVerb'.
     *
     * filterOptionsByExpression was renamed to searchTextExpression
     *
     * hideOptionsConditionExpression was renamed to hideAllOptionsConditionExpression
     *
     * We will copy all of the values from these old locations to the new locations
     * within customProperties.
     */
    private updateFieldConfigurationForBackwardsCompatibility(): void {
        if (this.optionsFieldConfiguration.filterOptionsByExpression) {
            this.optionsFieldConfiguration.searchTextExpression
                = this.optionsFieldConfiguration.filterOptionsByExpression;
            console.warn(
                `For the options field "${this.fieldPath} you have specified a setting for  `
                + '"filterOptionsByExpression" which is deprecated. Please change the field configuration '
                + 'to use "searchTextExpression" instead.');
        }
        if (this.optionsFieldConfiguration.hideOptionsConditionExpression) {
            this.optionsFieldConfiguration.hideAllOptionsConditionExpression =
                this.optionsFieldConfiguration.hideOptionsConditionExpression;
            console.warn(
                `For the options field "${this.fieldPath} you have specified a setting for  `
                + '"hideOptionsConditionExpression" which is deprecated. Please change the field configuration '
                + 'to use "hideAllOptionsConditionExpression" instead.');
        }
    }

    private applyConfigurationDefaults(): void {
        this.doesGetOptionsFromApi = this.optionsFieldConfiguration.optionsRequest != null;
        this.doesGetSelectedOptionFromApi = this.optionsFieldConfiguration.selectedOptionRequest != null;
        if (this.doesGetSelectedOptionFromApi) {
            this.selectedOptionRequestSettings.httpVerb = this.optionsFieldConfiguration.selectedOptionRequest.httpVerb
                ? this.optionsFieldConfiguration.selectedOptionRequest.httpVerb
                : 'GET';
            this.cacheSelectedOptionRequestWithMaxAgeSeconds =
                this.optionsFieldConfiguration.selectedOptionRequest.allowCachingWithMaxAgeSeconds;
            this.isSelectedOptionCacheEnabled = !!this.cacheSelectedOptionRequestWithMaxAgeSeconds;
        }
        if (this.doesGetOptionsFromApi) {
            this.optionsRequestSettings.httpVerb = this.optionsFieldConfiguration.optionsRequest.httpVerb
                ? this.optionsFieldConfiguration.optionsRequest.httpVerb
                : 'GET';
            this.optionsRequestDebounceTimeMilliseconds
                = this.optionsFieldConfiguration.optionsRequest.debounceTimeMilliseconds
                    ? this.optionsFieldConfiguration.optionsRequest.debounceTimeMilliseconds
                    : 100;
            this.cacheOptionsRequestWithMaxAgeSeconds =
                this.optionsFieldConfiguration.optionsRequest.allowCachingWithMaxAgeSeconds;
        }
    }

    protected applyDefaultPlaceholderText(): void {
        this.placeholderText = 'Please choose one';
    }

    public setHidden(hidden: boolean): void {
        super.setHidden(hidden);
        this.loadOptionsFromApiIfRequired();
    }

    private async loadDataFromApiIfRequired(): Promise<void> {
        if (this.doesGetSelectedOptionFromApi && !StringHelper.isNullOrEmpty(this.initialValue)) {
            await this.getSelectedOptionFromApi();

            // if we get a calculated value we'll need to also get the matching option from the API
            this.listenForCalculatedValueAndGetOptionFromApi();
        }
        this.initialised = true;
        this.loadOptionsFromApiIfRequired();
    }

    private loadOptionsFromApiIfRequired(): void {
        if (this.doesGetOptionsFromApi && this.optionsFieldConfiguration.optionsRequest.autoTrigger) {
            if (!this.hasLoadedOptionsFromApi && this.canReloadOptions && !this.hideAllOptions) {
                this.loadOptionsSubject.next();
            }
        }
    }

    private listenForCalculatedValueAndGetOptionFromApi(): void {
        if (this.calculatedValueSubject) {
            if (this.calculatedValueSelectedOptionSubscription) {
                this.calculatedValueSelectedOptionSubscription.unsubscribe();
            }
            this.calculatedValueSelectedOptionSubscription
                = this.calculatedValueSubject.pipe(takeUntil(this.destroyed))
                    .subscribe((value: string) => {
                        this.getSelectedOptionFromApi();
                    });
        }
    }

    protected setupExpressions(force: boolean = false): void {
        super.setupExpressions(force);
        this.setupNoOptionsFoundTextExpression();
        this.setupHideAllOptionsConditionExpression();
        if (this.doesGetSelectedOptionFromApi) {
            this.setupSelectedOptionRequestUrlExpression();
            this.setupSelectedOptionRequestPayloadExpression();
        }
        if (this.doesGetOptionsFromApi) {
            this.reloadingOptionsRequiresTrigger
                = this.optionsFieldConfiguration.optionsRequest.triggerExpression != null;
            this.setupOptionsRequestConditionExpression();
            this.setupOptionsRequestUrlExpression();
            this.setupOptionsRequestPayloadExpression();
            this.setupOptionsRequestTriggerExpression();
        }
        this.setupSearchTextExpression();
    }

    private setupNoOptionsFoundTextExpression(): void {
        this.noOptionsFoundTextExpression?.dispose();
        this.noOptionsFoundTextExpression = null;
        let expressionSource: string = this.optionsFieldConfiguration.noOptionsFoundText;
        if (expressionSource) {
            this.noOptionsFoundTextExpression = new TextWithExpressions(
                expressionSource,
                this.expressionDependencies,
                this.fieldKey + " no options found text",
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.noOptionsFoundTextExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => {
                    this.noOptionsFoundText = text;
                });
            this.noOptionsFoundTextExpression.triggerEvaluation();
        } else {
            this.noOptionsFoundText = this.DefaultNoOptionsFoundText;
        }
    }

    private setupHideAllOptionsConditionExpression(): void {
        this.hideAllOptionsConditionExpression?.dispose();
        this.hideAllOptionsConditionExpression = null;
        let expressionSource: string = this.optionsFieldConfiguration.hideAllOptionsConditionExpression;
        if (expressionSource) {
            this.hideAllOptionsConditionExpression = new Expression(
                expressionSource,
                this.expressionDependencies,
                this.fieldKey + ' hide all options condition',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.hideAllOptionsConditionExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: boolean) => this.hideAllOptions = result);
            this.hideAllOptionsConditionExpression.triggerEvaluation();
        } else {
            this.hideAllOptions = false;
        }
    }

    private setupOptionsRequestUrlExpression(): void {
        let urlExpressionSource: string = this.optionsFieldConfiguration.optionsRequest.urlExpression;
        let urlExpression: Expression = new Expression(
            urlExpressionSource,
            this.expressionDependencies,
            `${this.fieldKey} ${this.fieldType} options request url`,
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        urlExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe((result: string) => {
                this.optionsRequestSettings.url = this.sanitizer.sanitize(SecurityContext.URL, result);
                if (!this.reloadingOptionsRequiresTrigger &&
                    this.canReloadOptions && !this.loadingSelectedOptionFromApi) {

                    if (this.initialised) {
                        this.loadOptionsSubject.next();
                    }
                }
            });
        urlExpression.triggerEvaluation();
    }

    private setupOptionsRequestPayloadExpression(): void {
        let payloadExpressionSource: string = this.optionsFieldConfiguration.optionsRequest.payloadExpression;
        if (payloadExpressionSource) {
            let payloadExpression: Expression = new Expression(
                payloadExpressionSource,
                this.expressionDependencies,
                `${this.fieldKey} ${this.fieldType} options request payload`,
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            payloadExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => {
                    this.optionsRequestSettings.payload = result;
                    if (!this.reloadingOptionsRequiresTrigger &&
                        this.canReloadOptions && !this.loadingSelectedOptionFromApi) {
                        if (this.initialised) {
                            this.loadOptionsSubject.next();
                        }
                    }
                });
            payloadExpression.triggerEvaluation();
        }
    }

    private setupOptionsRequestConditionExpression(): void {
        let conditionExpressionSource: string = this.optionsFieldConfiguration.optionsRequest.conditionExpression;
        if (conditionExpressionSource) {
            this.optionsRequestConditionBeingUsed = true;
            this.optionsRequestConditionExpression?.dispose();
            this.optionsRequestConditionExpression = null;
            this.optionsRequestConditionExpression = new Expression(
                conditionExpressionSource,
                this.expressionDependencies,
                `${this.fieldKey} ${this.fieldType} options request condition`,
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.optionsRequestConditionExpression.nextResultObservable.pipe(
                takeUntil(this.destroyed), distinctUntilChanged(),
            )
                .subscribe((result: boolean) => {
                    this.optionsRequestConditionValue = result;
                    this.canReloadOptions = result;
                    if (!this.reloadingOptionsRequiresTrigger && this.canReloadOptions) {
                        if (this.initialised) {
                            this.loadOptionsSubject.next();
                        }
                    }
                });
            this.optionsRequestConditionExpression.triggerEvaluation();
        } else {
            this.canReloadOptions = true;
        }
    }

    private setupOptionsRequestTriggerExpression(): void {
        let triggerExpressionSource: string = this.optionsFieldConfiguration.optionsRequest.triggerExpression;
        if (triggerExpressionSource) {
            this.optionsRequestTriggerExpression?.dispose();
            this.optionsRequestTriggerExpression = null;
            this.optionsRequestTriggerExpression = new Expression(
                triggerExpressionSource,
                this.expressionDependencies,
                `${this.fieldKey} ${this.fieldType} options request trigger`,
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.optionsRequestTriggerExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((newValue: any) => {
                    if (this.canReloadOptions) {
                        this.loadOptionsSubject.next();
                    }
                });
        }
    }

    private setupSelectedOptionRequestUrlExpression(): void {
        let urlExpressionSource: string = this.optionsFieldConfiguration.selectedOptionRequest.urlExpression;
        if (urlExpressionSource) {
            this.selectedOptionRequestUrlExpression?.dispose();
            this.selectedOptionRequestUrlExpression = null;
            this.selectedOptionRequestUrlExpression = new Expression(
                urlExpressionSource,
                this.expressionDependencies,
                `${this.fieldKey} ${this.fieldType} selected option request url`,
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.selectedOptionRequestUrlExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => {
                    this.selectedOptionRequestSettings.url = this.sanitizer.sanitize(SecurityContext.URL, result);
                    // Store the data in the cache when selected item changed
                    if (this.isSelectedOptionCacheEnabled && this.initialValue != this.getValueForExpressions()){
                        this.webhookService.apiCache.setCachedApiResponse(
                            this.selectedOptionRequestSettings.url, this.data);
                    }
                });
            this.selectedOptionRequestUrlExpression.triggerEvaluation();
        }
    }

    private setupSelectedOptionRequestPayloadExpression(): void {
        let payloadExpressionSource: string = this.optionsFieldConfiguration.selectedOptionRequest.payloadExpression;
        if (payloadExpressionSource) {
            this.selectedOptionRequestPayloadExpression?.dispose();
            this.selectedOptionRequestPayloadExpression = null;
            this.selectedOptionRequestPayloadExpression = new Expression(
                payloadExpressionSource,
                this.expressionDependencies,
                `${this.fieldKey} ${this.fieldType} selected option request payload`,
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.selectedOptionRequestPayloadExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => {
                    this.selectedOptionRequestSettings.payload = result;
                });
            this.selectedOptionRequestPayloadExpression.triggerEvaluation();
        }
    }

    private setupSearchTextExpression(): void {
        let expressionSource: string = this.optionsFieldConfiguration.searchTextExpression;
        if (expressionSource) {
            this.searchTextExpression?.dispose();
            this.searchTextExpression = null;
            this.searchTextExpression = new Expression(
                expressionSource,
                this.expressionDependencies,
                `${this.fieldKey} ${this.fieldType} filter options by`,
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.searchTextExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((filterString: string) => this.filterOptions(filterString));
        }
    }

    private debounceOptionsRequests(): void {
        if (this.doesGetOptionsFromApi) {
            if (this.debounceOptionsRequestSubscription) {
                this.debounceOptionsRequestSubscription.unsubscribe();
            }
            this.debounceOptionsRequestSubscription = this.loadOptionsSubject
                .pipe(
                    debounceTime(this.optionsRequestDebounceTimeMilliseconds),
                    takeUntil(this.destroyed),
                )
                .subscribe(() => {
                    if (this.canReloadOptions) {
                        this.getOptionsFromApi();
                    }
                });
        }
    }

    protected generateItems(): void {
        let selectOptions: Array<SelectOption> = new Array<SelectOption>();
        this.numberOfSelectOptionsShown = 0;
        let optionsContainsValue: boolean = false;
        let index: number = 0;
        for (let optionConfig of this.sourceItems) {
            if (optionConfig.value == this.getValueForExpressions()) {
                optionsContainsValue = true;
            }
            let option: SelectOption = <SelectOption>{
                label: '',
                value: optionConfig.value,
                disabled: true,
                icon: optionConfig.icon || null,
                cssClass: optionConfig.cssClass,
                render: false,
                filtered: false,
                id: this.getOptionHtmlId(index),
                definition: optionConfig,
                destroyed: new Subject<void>(),
            };

            // when this component is destroyed, also signal that the option is destroyed
            this.destroyed.subscribe(option.destroyed);

            // if there is no label use the value as the label
            let labelSource: string = optionConfig.label || optionConfig.value;
            option.searchableText = optionConfig.searchableText ?
                optionConfig.searchableText : optionConfig.label;
            option.properties = optionConfig.properties ? optionConfig.properties : null;
            this.setupSelectOptionLabelExpression(option, labelSource);
            let hideConditionExpression: string =
                optionConfig.hideConditionExpression;
            this.setupSelectOptionHiddenExpressions(
                option,
                hideConditionExpression);
            this.setupSelectOptionDisabledExpressions(
                option,
                this.optionsFieldConfiguration.disabledConditionExpression,
                optionConfig.disabledConditionExpression);
            selectOptions.push(option);
            if (option.properties && option.value == this.formControl.value) {
                this.data = option.properties || null;
            }
            index++;
        }
        let optionsChanged: boolean = false;
        if (this.selectOptions.length == 0 && selectOptions.length > 0
            || this.optionSetKey != this.optionsFieldConfiguration.optionSetKey
        ) {
            this.selectOptions = selectOptions;
            optionsChanged = true;
        } else {
            let stats: ArraySyncStats = ArrayHelper.synchronise<SelectOption>(
                selectOptions,
                this.selectOptions,
                (item1: SelectOption, item2: SelectOption): boolean => {
                    let isEqual: boolean = _.isEqual(item1.definition, item2.definition);
                    return isEqual;
                },
                (item: SelectOption): void => {
                    item.destroyed.next();
                    item.destroyed.complete();
                });
            optionsChanged = stats.added > 0 || stats.removed > 0;
        }
        this.optionSetKey = this.optionsFieldConfiguration.optionSetKey;
        if (optionsChanged) {
            this.onSelectOptionsListChange();
            if (!optionsContainsValue) {
                this.clearValue();
            }
        }
    }

    /**
     * adds/removes options to match the given new list
     * @param newSelectOptions
     */
    private clearSelectOptions(): void {
        let oldSelectOptions: Array<SelectOption> = this.selectOptions;
        if (oldSelectOptions) {
            for (let oldOption of oldSelectOptions) {
                if (oldOption.hiddenExpressionSetOnTheOption) {
                    oldOption.hiddenExpressionSetOnTheOption.dispose();
                }
                if (oldOption.labelExpression) {
                    oldOption.labelExpression.dispose();
                }
                if (oldOption.disabledExpression) {
                    oldOption.disabledExpression.dispose();
                }
            }
        }
        this.numberOfSelectOptionsShown = 0;
        this.onSelectOptionsListChange();
    }

    protected setupSelectOptionHiddenExpressions(
        selectOption: SelectOption,
        hiddenExpressionSource: string,
    ): void {
        if (!hiddenExpressionSource && !this.optionsFieldConfiguration.optionHideConditionExpression) {
            selectOption.render = true;
            this.numberOfSelectOptionsShown++;
        } else {
            this.setupSelectOptionHiddenExpressionSetOnTheOption(
                selectOption,
                hiddenExpressionSource);
            this.setupSelectOptionHiddenExpressionSetOnTheField(selectOption);
        }
    }

    protected setupSelectOptionHiddenExpressionSetOnTheOption(
        selectOption: SelectOption,
        hiddenExpressionSource: string,
    ): void {
        if (hiddenExpressionSource) {
            selectOption.hiddenExpressionSetOnTheOption = new Expression(
                hiddenExpressionSource,
                this.expressionDependencies,
                this.fieldKey + ' option hidden set on the option',
                this.getFixedArgumentsForSelectOptionExpression(selectOption),
                this.getObservableArgumentsForSelectOptionExpression(selectOption),
                this.scope);
            selectOption.hiddenExpressionSetOnTheOption.nextResultObservable.pipe(takeUntil(selectOption.destroyed))
                .subscribe((hidden: boolean) => {
                    let hasBecomeRendered: boolean = !selectOption.render && !hidden;
                    let hasBecomeUnrendered: boolean = selectOption.render && hidden;
                    selectOption.render = !hidden;
                    if (hasBecomeRendered && !selectOption.filtered) {
                        this.numberOfSelectOptionsShown++;
                    } else if (hasBecomeUnrendered && !selectOption.filtered) {
                        this.numberOfSelectOptionsShown--;
                    }
                    let selected: boolean = selectOption.value == this.formControl.value;
                    if (selected && hidden) {
                        this.onSelectedOptionHidden(selectOption);
                    }
                    if (hasBecomeRendered || hasBecomeUnrendered) {
                        this.onSelectOptionsListChange();
                    }
                });
            selectOption.hiddenExpressionSetOnTheOption.triggerEvaluation();
        }
    }

    protected setupSelectOptionHiddenExpressionSetOnTheField(selectOption: SelectOption): void {
        const hiddenExpressionSource: string = this.optionsFieldConfiguration.optionHideConditionExpression;
        if (hiddenExpressionSource) {
            selectOption.hiddenExpressionSetOnTheField = new Expression(
                hiddenExpressionSource,
                this.expressionDependencies,
                this.fieldKey + ' option hidden set on the field',
                this.getFixedArgumentsForSelectOptionExpression(selectOption),
                this.getObservableArgumentsForSelectOptionExpression(selectOption),
                this.scope);
            selectOption.hiddenExpressionSetOnTheField.nextResultObservable.pipe(takeUntil(selectOption.destroyed))
                .subscribe((hidden: boolean) => {
                    let hasBecomeRendered: boolean = !selectOption.render && !hidden;
                    let hasBecomeUnrendered: boolean = selectOption.render && hidden;
                    selectOption.render = !hidden;
                    if (hasBecomeRendered && !selectOption.filtered) {
                        this.numberOfSelectOptionsShown++;
                    } else if (hasBecomeUnrendered && !selectOption.filtered) {
                        this.numberOfSelectOptionsShown--;
                    }
                    let selected: boolean = selectOption.value == this.formControl.value;
                    if (selected && hidden) {
                        this.onSelectedOptionHidden(selectOption);
                    }
                    if (hasBecomeRendered || hasBecomeUnrendered) {
                        this.onSelectOptionsListChange();
                    }
                });
            selectOption.hiddenExpressionSetOnTheField.triggerEvaluation();
        }
    }

    /**
     * Called when the currently selected select option becomes hidden
     */
    protected onSelectedOptionHidden(selectOption: SelectOption): void {
        setTimeout(
            () => {
                let formControl: AbstractControl = this.form.controls[this.fieldKey];
                formControl.setValue('');
                formControl.markAsUntouched();
                formControl.markAsPristine();
                this.onChange(/* this.generateEventObject(new CustomEvent('custom'), true)*/);
            }, 0);
    }

    protected setupSelectOptionDisabledExpressions(
        selectOption: SelectOption,
        allDisabledExpressionSource: string,
        optionDisabledExpressionSource: string,
    ): void {
        // join the two expressions into one using the OR operator, since the option should be disabled
        // if the entire select is disabled, or if just the option is disabled.
        let joinedDisabledExpressionSource: string =
            [allDisabledExpressionSource, optionDisabledExpressionSource].filter(Boolean).join(' || ');
        if (!joinedDisabledExpressionSource || joinedDisabledExpressionSource == '') {
            selectOption.disabled = false;
        } else {
            selectOption.disabledExpression = new Expression(
                joinedDisabledExpressionSource,
                this.expressionDependencies,
                `${this.fieldKey} option "${selectOption.label}" disabled`,
                this.getFixedArgumentsForSelectOptionExpression(selectOption),
                this.getObservableArgumentsForSelectOptionExpression(selectOption),
                this.scope);
            selectOption.disabledExpression.nextResultObservable.pipe(takeUntil(selectOption.destroyed))
                .subscribe((disabled: boolean) => {
                    let hasBecomeDisabled: boolean = !selectOption.disabled && disabled;
                    let hasBecomeEnabled: boolean = selectOption.disabled && !disabled;
                    selectOption.disabled = disabled;
                    let selected: boolean = !this.formControl.disabled && selectOption.value == this.formControl.value;
                    if (disabled && selected) {
                        this.onSelectedOptionDisabled();
                    }
                    if (hasBecomeDisabled || hasBecomeEnabled) {
                        this.onSelectOptionsListChange();
                    }
                });
            selectOption.disabledExpression.triggerEvaluation();
        }
    }

    protected getFixedArgumentsForSelectOptionExpression(selectOption: SelectOption): FixedArguments {
        let fieldArguments: FixedArguments = this.getFixedExpressionArguments();
        let optionArguments: FixedArguments = {
            optionProperties: selectOption.properties,
            optionValue: selectOption.value,
            optionSearchableText: selectOption.searchableText,
        };
        // merge the two objects
        return _.merge({}, fieldArguments, optionArguments);
    }

    protected getObservableArgumentsForSelectOptionExpression(selectOption: SelectOption): ObservableArguments {
        let fieldArguments: ObservableArguments = this.getObservableExpressionArguments();
        let optionArguments: ObservableArguments = {
            optionLabel: selectOption.labelExpression.latestResultObservable,
        };
        // merge the two objects
        return _.merge({}, fieldArguments, optionArguments);
    }

    protected setupSelectOptionLabelExpression(selectOption: SelectOption, labelExpressionSource: string): void {
        selectOption.labelExpression = new TextWithExpressions(
            labelExpressionSource,
            this.expressionDependencies,
            this.fieldKey + ' option label',
            this.getFixedArgumentsForSelectOptionExpression(selectOption),
            this.getObservableExpressionArguments(),
            this.scope);
        selectOption.labelExpression.nextResultObservable.pipe(takeUntil(selectOption.destroyed))
            .subscribe((value: string) => selectOption.label = value.replace(/&lt;/g, '<'));
        selectOption.labelExpression.triggerEvaluation();
    }

    protected onClick(e: any, disabled: any): void {
        if (disabled) {
            e.preventDefault();
        } else {
            this.formControl.markAsTouched();
        }
    }

    private getOptionsFromApi(): void {
        if (this.hidden) {
            // don't bother fetching options if we have just been hidden.
            return;
        }
        this.hasLoadedOptionsFromApi = true;
        let payload: string = this.optionsRequestSettings.payload ? this.optionsRequestSettings.payload : '';
        this.onApiRequestStart();
        this.clearSelectOptions();
        this.activeApiCalls++;
        this.isLoading = this.activeApiCalls > 0;
        this.errorMessage = null;
        if (this.optionsRequestSubscription) {
            this.optionsRequestSubscription.unsubscribe();
        }
        let body: any = payload;
        let url: string = this.optionsRequestSettings.url;
        if (StringHelper.equalsIgnoreCase(this.optionsRequestSettings.httpVerb, 'GET')) {
            body = null;
            if (!StringHelper.isNullOrEmpty(payload)) {
                url += '?' + payload;
            }
        } else {
            if (QueryStringHelper.isQueryString(body)) {
                body = QueryStringHelper.queryStringToJson(body);
            }
        }
        this.optionsRequestSubscription
            = this.webhookService.sendRequest(
                this.fieldPath,
                this.optionsRequestSettings.httpVerb,
                url,
                body,
                this.cacheOptionsRequestWithMaxAgeSeconds)
                .pipe(
                    finalize(() => {
                        this.activeApiCalls--;
                        this.isLoading = this.activeApiCalls > 0;
                        this.onApiRequestFinish();
                    }),
                )
                .subscribe((data: Array<any>) => {
                    if (!Array.isArray(data)) {
                        if (data['options'] && Array.isArray(data['options'])) {
                            data = data['options'];
                        } else {
                            throw Errors.Product.FieldOptionDataInvalid(
                                this.fieldKey,
                                ["The call to fetch options did not return an array."]);
                        }
                    }
                    if (data.length > 0) {
                        this.validateOptionFromApi(data[0]);
                    }
                    this.sourceItems = data;
                    this.generateItems();
                },
                (err: HttpErrorResponse) => {
                    this.errorMessage = 'There was a problem loading the options from the remote server. '
                        + 'Please reload the page and try again.';
                    throw err;
                });
    }

    private async getSelectedOptionFromApi(): Promise<void> {
        this.loadingSelectedOptionFromApi = true;
        let payload: string = this.selectedOptionRequestSettings.payload ?
            this.selectedOptionRequestSettings.payload : '';
        this.placeholderText = 'Loading selected option...';
        this.onApiRequestStart();
        this.activeApiCalls++;
        this.isLoading = this.activeApiCalls > 0;
        this.errorMessage = null;
        let body: any = payload;
        let url: string = this.selectedOptionRequestSettings.url;
        if (StringHelper.equalsIgnoreCase(this.selectedOptionRequestSettings.httpVerb, 'GET')) {
            body = null;
            if (!StringHelper.isNullOrEmpty(payload)) {
                url += '?' + payload;
            }
        } else {
            if (QueryStringHelper.isQueryString(body)) {
                body = QueryStringHelper.queryStringToJson(body);
            }
        }

        this.webhookService.sendRequest(
            this.fieldPath,
            this.selectedOptionRequestSettings.httpVerb,
            url,
            body,
            this.cacheSelectedOptionRequestWithMaxAgeSeconds)
            .pipe(
                finalize(() => {
                    this.activeApiCalls--;
                    this.onApiRequestFinish();
                    this.isLoading = this.activeApiCalls > 0;
                    this.applyDefaultPlaceholderText();
                }),
            )
            .toPromise().then((data: any) => {
                this.receiveOptionData(data, this.formControl.value);
            },
            (err: HttpErrorResponse) => {
                this.errorMessage = 'There was a problem loading the selected option from the remote server. '
                    + 'Please reload the page and try again.';
                throw err;
            });
    }

    /**
     *
     * @param data
     * @param formControlValue we need to pass this in because for some reason ng-select is wiping it
     */
    private receiveOptionData(data: any, formControlValue: any): void {
        this.formControl.setValue(formControlValue);
        if (!Array.isArray(data)) {
            this.validateOptionFromApi(data);
            data = [data];
        } else if (data.length == 0) {
            return;
        } else if (data.length > 0) {
            this.validateOptionFromApi(data[0]);
        }
        this.sourceItems = <Array<any>>data;
        this.generateItems();
        this.onSelectedOptionLoaded(data[0]);
        this.loadingSelectedOptionFromApi = false;
    }

    private validateOptionFromApi(data: any): void {
        let errors: Array<string> = new Array<string>();
        if (data.label == null) {
            errors.push('The property "label" was missing.');
        }
        if (data.value == null) {
            errors.push('The property "value" was missing.');
        }
        if (errors.length > 0) {
            throw Errors.Product.FieldOptionDataInvalid(this.fieldKey, errors);
        }
    }

    protected onApiRequestStart(): void {
        // can be overridden to show a loading icon
    }

    protected onApiRequestFinish(): void {
        // can be overridden to hide the loading icon
    }

    protected onSelectedOptionLoaded(option: SelectOption): void {
        // can be overridden to perform specific actions after loading
    }

    /**
     * filters the option to only those matching the filter string either by label or searchableText (if there is )
     * @param filterString
     */
    protected filterOptions(filterString: string): void {
        let numberOfSelectOptionsShown: number = 0;
        let anyChanges: boolean = false;
        this.selectOptions.forEach((option: SelectOption) => {
            let wasFiltered: boolean = option.filtered;
            if (StringHelper.isNullOrEmpty(filterString)) {
                option.filtered = false;
            } else {
                option.filtered = !this.optionMatches(filterString, option);
            }
            anyChanges = wasFiltered != option.filtered ? true : anyChanges;
            numberOfSelectOptionsShown += option.render && !option.filtered ? 1 : 0;
        });
        this.numberOfSelectOptionsShown = numberOfSelectOptionsShown;
        if (anyChanges) {
            this.onSelectOptionsListChange();
        }
    }

    /**
     * @returns true if the option matches the filter string
     * @param filterString
     * @param option
     */
    public optionMatches(filterString: string, option: SelectOption): boolean {
        const labelMatches: boolean = StringHelper.containsIgnoreCase(option.label, filterString);
        const searchableTextMatches: boolean = StringHelper.containsIgnoreCase(option.searchableText, filterString);
        return labelMatches || searchableTextMatches;
    }

    public onChange(event: any = null): void {
        let controlValue: any = this.formControl.value || '';
        if (controlValue != this.valueSubject.value) {
            this.updateFieldValueProperties();

            // reset the formControl value when loading selected option from API
            if (this.loadingSelectedOptionFromApi && !controlValue ){
                controlValue = this.valueSubject.value;
            }

            // this is more like a hack to update the form model immediately when the field change value.
            // somehow there is a problem with formly that updates it with a delay, we need it updated immediately.
            this.form.controls[this.fieldKey].setValue(controlValue);
        }
        super.onChange(event);
    }

    protected updateFieldValueProperties(): void {
        let clearValueProperties: boolean = false;
        if (this.selectOptions) {
            let matchingOptions: Array<SelectOption> = this.selectOptions.filter(
                (option: SelectOption) => option.value == this.formControl.value);
            if (matchingOptions.length) {
                // there should be only one so grab the first
                let matchingOption: SelectOption = matchingOptions[0];
                this.data = matchingOption.properties || null;
            } else {
                clearValueProperties = true;
            }
        } else {
            clearValueProperties = true;
        }
        if (clearValueProperties) {
            this.data = null;
        }
    }

    /**
     * Callback for when the list of select options changes. Can be overridden to respond to changes.
     */
    protected onSelectOptionsListChange(): void {
        // can be overridden
    }

    protected onSelectedOptionDisabled(): void {
        // can be overridden
    }

    /**
     * @returns generates a html id for the option using the field path replacing "[]." characters with "-",
     * and appending the the index of the option.
     * e.g. "claims0-settled-1"
     */
    private getOptionHtmlId(index: number): string {
        return `${this.fieldPath}-${index}`;
    }

    public publishChange(): void {
        super.publishChange();
        this.updateFieldValueProperties();
    }
}
