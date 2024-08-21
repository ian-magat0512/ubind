import { AfterViewChecked, AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { OptionsField } from '../options/options.field';
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
import {
    ChangeContext, CustomStepDefinition, LabelType, Options as SliderConfigOptions,
} from '@angular-slider/ngx-slider';
import { SliderFieldConfiguration } from '@app/resource-models/configuration/fields/slider-field.configuration';
import { Expression, FixedArguments } from '@app/expressions/expression';
import { SelectOption } from '../options/select-option';
import { debounceTime, takeUntil, throttleTime } from 'rxjs/operators';
import { NumberHelper } from '@app/helpers/number.helper';
import { FieldDataType } from '@app/models/field-data-type.enum';
import { asyncScheduler, Subject, SubscriptionLike } from 'rxjs';
import { SliderStylingSettings } from '@app/resource-models/configuration/slider-styling-settings';
import * as _ from 'lodash-es';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { ColorHelper } from '@app/helpers/color.helper';
import { ColorHsl } from '@app/models/color-hsl';
import { LocaleService } from '@app/services/locale.service';

/**
 * Export slider field component class.
 * This class manage slider field functions.
 */
@Component({
    selector: '' + FieldSelector.Slider,
    templateUrl: './slider.field.html',
    styleUrls: ['./slider.field.scss'],
})
export class SliderField extends OptionsField implements OnInit, OnDestroy, AfterViewChecked, AfterViewInit {

    @ViewChild('sliderColorReference') public sliderColorReference: ElementRef;
    public sliderConfigOptions: SliderConfigOptions;
    public sliderFieldConfiguration: SliderFieldConfiguration;
    public sliderValue: number;

    /**
     * Since we can't update sliderValue too often (or the ngx-slider component will get itself into a loop
     * and go schitzophrenic) we will track what we want the sliderValue to be here. This is needed for keyboard
     * controls, since someone can hold down the key and that would send a lot of updates.
     */
    public shadowSliderValue: number;
    private visibleSelectOptions: Array<SelectOption>;
    private valueLabelExpressions: Array<Expression>;
    private legendItemExpressions: Array<Expression>;
    private axisStartLabelExpression: Expression;
    private axisEndLabelExpression: Expression;
    private thumbLabelExpression: Expression;
    private axisStartValue: number;
    private axisEndValue: number;
    private minSelectableValue?: number;
    private maxSelectableValue?: number;
    private stepInterval?: number;
    private tickMarkInterval?: number;
    private tickMarkStepIndexArray?: Array<number>;
    private showSelectionBarFromValue?: number;
    private axisStartValueExpression: Expression;
    private axisEndValueExpression: Expression;
    private minSelectableValueExpression: Expression;
    private maxSelectableValueExpression: Expression;
    private stepIntervalExpression: Expression;
    private tickMarkIntervalExpression: Expression;
    private tickMarkStepIndexArrayExpression: Expression;
    private showSelectionBarFromValueExpression: Expression;
    public usesOptions: boolean;
    public tickMarkStepIndexesDebug: string;
    private showSelectionBarFromAxisStart?: boolean;
    private totalStepsContinuous: number = 10000;
    private scalingMultiplier: number = 1;
    private sliderChangeSubject: Subject<void> = new Subject<void>();
    private sliderValueChangeSubject: Subject<any> = new Subject<any>();
    private hideBubbleDebounceTimeMillis: number = 1000;
    public showBubble: boolean = false;
    public vertical: boolean;
    public stylingSettings: SliderStylingSettings;
    public showTickMarks: boolean;
    private showLegend: boolean;
    private sliderEl: HTMLElement;
    public legendItemsVertical: boolean = false;
    public legendItemsDiagonal: boolean = false;
    private sliderConfigured: boolean = false;
    private needsOrientationCheck: boolean = false;
    private skipLegendItems: number = 0;
    private stepsArray: Array<CustomStepDefinition>;
    public defaultLegendItemWidthPixels: number;
    private rotateLabelsToFit: boolean;
    private needsReconfiguration: boolean;
    private formControlValueChangeSubject: Subject<any> = new Subject<any>();
    private formControlValueChangeSubscription: SubscriptionLike;
    public hasLabelsAbove: boolean;
    public hasAxisStartEndLabels: boolean;
    public sliderColorH: string;
    public sliderColorS: string;
    public sliderColorL: string;
    public unselectedBarColorH: string;
    public unselectedBarColorS: string;
    public unselectedBarColorL: string;
    public isOptionSetNumeric: boolean;
    private localeService: LocaleService;

    /**
     * Since expressions cause the sliders configuration to change, and expressions
     * can trigger hundreds of times a second, it causes the ngx-slider to get into
     * a strange state where it's position is jittering. To stop this from happening,
     * when we have to reconfigure the slider, we'll debounce it so it doesn't happen too
     * often, and thus doesn't overload the ngx-slider component and make it go
     * schitzophrenic.
     */
    private reconfigureSliderSubject: Subject<void> = new Subject<void>();

    public constructor(
        protected formService: FormService,
        protected webhookService: WebhookService,
        protected workflowService: WorkflowService,
        expressionDependencies: ExpressionDependencies,
        sanitizer: DomSanitizer,
        httpClient: HttpClient,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        configService: ConfigService,
        private elementRef: ElementRef,
        fieldEventLogRegistry: FieldEventLogRegistry,
        localeService: LocaleService,
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
        this.fieldType = FieldType.Slider;
        this.localeService = localeService;
    }

    public ngOnInit(): void {
        this.sliderFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.applySettings();
        this.usesOptions = this.sliderFieldConfiguration.optionSetKey != null
            || this.sliderFieldConfiguration.optionsRequest != null;
        this.throttleOrDebounceFormControlValueChange();
        this.throttleSliderValueChange();
        this.debounceHideBubble();
        this.debounceReconfiguration();
        super.ngOnInit();
        this.sliderEl = this.elementRef.nativeElement;
        this.eventService.windowResizeSubject.pipe(takeUntil(this.destroyed))
            .subscribe(() => setTimeout(() => {
                this.orientLegend();
            }));
    }

    public ngAfterViewChecked(): void {
        if (this.needsOrientationCheck) {
            this.needsOrientationCheck = false;
            setTimeout(() => {
                this.orientLegend();
            });
        }
    }

    public ngAfterViewInit(): void {
        this.generateSliderColorTheme();
    }

    private applySettings(): void {
        this.stylingSettings = _.merge(
            {},
            this.configService.configuration.theme?.sliderStylingSettings,
            this.sliderFieldConfiguration.styling);
        this.generateSliderColorTheme();
    }

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(<string>this.field.key).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration }) => {
                this.sliderFieldConfiguration = <SliderFieldConfiguration>configs.new;
                this.applySettings();
                this.throttleOrDebounceFormControlValueChange();
                this.configureSlider();
            });
    }

    public ngOnDestroy(): void {
        this.destroyExpressions();
    }

    protected destroyExpressions(): void {
        this.destroyValueLabelExpressions();
        this.destroyLegendItemExpressions();
        this.destroyThumbLabelExpression();
        this.destroyAxisStartLabelExpression();
        this.destroyAxisEndLabelExpression();
        this.destroyAxisStartValueExpression();
        this.destroyAxisEndValueExpression();
        this.destroyMinSelectableValueExpression();
        this.destroyMaxSelectableValueExpression();
        this.destroyStepIntervalExpression();
        this.destroyTickMarkIntervalExpression();
        this.destroyTickMarkStepIndexArrayExpression();
        this.destroyShowSelectionBarFromValueExpression();
        super.destroyExpressions();
    }

    private destroyLegendItemExpressions(): void {
        if (this.legendItemExpressions) {
            this.legendItemExpressions.forEach((expression: Expression) => {
                expression.dispose();
            });
            delete this.legendItemExpressions;
        }
    }

    private destroyValueLabelExpressions(): void {
        if (this.valueLabelExpressions) {
            this.valueLabelExpressions.forEach((expression: Expression) => {
                expression.dispose();
            });
            delete this.valueLabelExpressions;
        }
    }

    private destroyThumbLabelExpression(): void {
        if (this.thumbLabelExpression) {
            this.thumbLabelExpression.dispose();
            delete this.thumbLabelExpression;
        }
    }

    private destroyAxisStartLabelExpression(): void {
        if (this.axisStartLabelExpression) {
            this.axisStartLabelExpression.dispose();
            delete this.axisStartLabelExpression;
        }
    }

    private destroyAxisEndLabelExpression(): void {
        if (this.axisEndLabelExpression) {
            this.axisEndLabelExpression.dispose();
            delete this.axisEndLabelExpression;
        }
    }

    protected initialiseField(): void {
        super.initialiseField();
        this.initialised = false;
        if (!this.doesGetOptionsFromApi) {
            this.sliderConfigured = true;
            this.configureSlider();
        }

        // ensure that when the field's disabled/readonly status changes, we re-configure the slider.
        this.disabledExpression?.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe(() => this.reconfigureSliderSubject.next());
        this.readOnlyExpression?.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe(() => this.reconfigureSliderSubject.next());

        this.onExpressionValueChanges();
        this.initialised = true;
    }

    private debounceReconfiguration(): void {
        this.reconfigureSliderSubject.pipe(
            takeUntil(this.destroyed),
            debounceTime(100),
        ).subscribe(() => {
            this.needsReconfiguration = false;
            this.configureSlider();
        });
    }

    private configureSlider(): void {
        this.showSelectionBarFromAxisStart = this.sliderFieldConfiguration.showSelectionBarFromAxisStart ?? true;
        this.vertical = this.sliderFieldConfiguration.vertical ?? false;
        this.showLegend = this.sliderFieldConfiguration.showLegend !== false;
        this.hasAxisStartEndLabels = this.sliderFieldConfiguration.showAxisStartValueLabel
            || this.sliderFieldConfiguration.showAxisEndValueLabel
            || this.sliderFieldConfiguration.showLegend === false;
        this.hasLabelsAbove = this.sliderFieldConfiguration.showLegend === true
            && this.sliderFieldConfiguration.showTickMarkValueLabels === true
            && this.sliderFieldConfiguration.continuous === false;
        if (this.usesOptions) {
            this.configureSliderForOptions();
        } else {
            this.configureSliderForNumbers();
        }
        this.needsOrientationCheck = true;
        this.onValueChange(this.sliderValue);
    }

    protected initialiseOptions(): void {
        if (this.usesOptions) {
            super.initialiseOptions();
        }
    }

    protected updateOptions(): void {
        if (this.usesOptions) {
            super.updateOptions();
        }
    }

    private configureSliderForOptions(): void {
        // we need to tell ngx-slider we are always showing tick marks even if we're not, because if we don't then
        // the legend won't show up. So instead we hide the tick marks with css.
        this.showTickMarks = this.sliderFieldConfiguration.showTickMarks !== false;
        this.defaultLegendItemWidthPixels = 70;
        this.rotateLabelsToFit = this.sliderFieldConfiguration.rotateLabelsToFit ?? true;
        this.determineIfOptionSetIsNumeric();

        // we need to set totalStepsContinuous before calling configureContinuousSliderForOptions and 
        // configureDiscreteSliderForOptions so that it will use calculated totalStepsContinuous
        // when slider has initial value.
        if (this.usesOptions && this.isOptionSetNumeric) {
            let maxValue: number = Number(this.visibleSelectOptions[this.visibleSelectOptions.length - 1].value);
            this.totalStepsContinuous = this.getTotalStepsContinuous(maxValue);
        }
        if (this.sliderFieldConfiguration.continuous !== false) {
            this.configureContinuousSliderForOptions();
        } else {
            this.configureDiscreteSliderForOptions();
        }
    }

    private configureDiscreteSliderForOptions(): void {
        this.setupValueLabelExpressions();
        this.setupLegendItemExpressions();
        this.setupAxisStartLabelExpression();
        this.setupAxisEndLabelExpression();
        this.setupShowSelectionBarFromValue();
        this.setupMinSelectableValue();
        this.setupMaxSelectableValue();
        this.stepsArray = new Array<CustomStepDefinition>();
        this.sliderValue = this.shadowSliderValue = 0;
        for (let i: number = 0; i < this.visibleSelectOptions.length; i++) {
            let step: any = { value: i };
            this.stepsArray.push(step);
            if (this.visibleSelectOptions[i].value == this.formControl.value) {
                this.sliderValue = this.shadowSliderValue = i;
            }
        }
        this.sliderConfigOptions = {
            floor: 0,
            ceil: this.visibleSelectOptions.length - 1,
            stepsArray: this.stepsArray,
            showTicks: true,
            minLimit: this.minSelectableValue,
            maxLimit: this.maxSelectableValue,
            showTicksValues: this.sliderFieldConfiguration.showTickMarkValueLabels,
            translate: this.getValueLabelForOption.bind(this),
            keyboardSupport: true,
            vertical: this.vertical,
            rightToLeft: this.sliderFieldConfiguration.invertAxis ?? false,
            showSelectionBar: this.showSelectionBarFromAxisStart,
            showSelectionBarEnd: this.sliderFieldConfiguration.showSelectionBarFromAxisEnd ?? false,
            showSelectionBarFromValue: this.showSelectionBarFromValue,
            disabled: this.disabled,
            readOnly: this.readOnly,
            getStepLegend: this.sliderFieldConfiguration.showLegend !== false
                ? this.getLegendItemForStep.bind(this)
                : null,
        };
        this.setupSliderValueBelowMinOrAboveMax();
    }

    private configureContinuousSliderForOptions(): void {
        this.setupValueLabelExpressions();
        this.setupLegendItemExpressions();
        this.setupAxisStartLabelExpression();
        this.setupAxisEndLabelExpression();
        this.setupMinSelectableValue();
        this.setupMaxSelectableValue();
        this.setupShowSelectionBarFromValue();
        this.stepInterval = this.visibleSelectOptions.length > 0
            ? this.totalStepsContinuous / (this.visibleSelectOptions.length - 1)
            : null;
        this.sliderValue = this.shadowSliderValue = 0;
        if (this.formControl.value) {
            const selectedOptionIndex: number = this.getSelectedOptionIndexForValue(this.formControl.value);
            if (selectedOptionIndex > -1) {
                this.sliderValue = this.shadowSliderValue = selectedOptionIndex * this.stepInterval;
            }
        }
        this.sliderConfigOptions = {
            floor: 0,
            ceil: this.totalStepsContinuous,
            step: 1,
            minLimit: this.minSelectableValue,
            maxLimit: this.maxSelectableValue,
            tickStep: this.stepInterval,
            showTicks: true,
            showTicksValues: this.sliderFieldConfiguration.showTickMarkValueLabels,
            translate: this.getValueLabelForOption.bind(this),
            keyboardSupport: false,
            vertical: this.vertical,
            rightToLeft: this.sliderFieldConfiguration.invertAxis ?? false,
            showSelectionBar: this.showSelectionBarFromAxisStart,
            showSelectionBarEnd: this.sliderFieldConfiguration.showSelectionBarFromAxisEnd ?? false,
            showSelectionBarFromValue: this.showSelectionBarFromValue,
            disabled: this.disabled,
            readOnly: this.readOnly,
            getLegend: this.sliderFieldConfiguration.showLegend !== false
                ? this.getLegendItemForOption.bind(this)
                : null,
        };

        this.setupSliderValueBelowMinOrAboveMax();
    }

    private setupSliderValueBelowMinOrAboveMax(): void {
        // ensure the value of the slider isn't below the min or above the max
        if (this.minSelectableValue && this.sliderValue < this.minSelectableValue) {
            this.sliderValue = this.shadowSliderValue = this.minSelectableValue;
        }
        if (this.maxSelectableValue && this.sliderValue > this.maxSelectableValue) {
            this.sliderValue = this.shadowSliderValue = this.maxSelectableValue;
        }
    }

    private getSelectedOptionIndexForValue(value: any): number {
        return this.visibleSelectOptions
            .findIndex((selectOption: SelectOption) => selectOption.value == value);
    }

    private setupAxisStartLabelExpression(): void {
        this.destroyAxisStartLabelExpression();
        if (this.sliderFieldConfiguration.showTickMarkValueLabels
            || !this.sliderFieldConfiguration.showAxisStartValueLabel
            || !this.sliderFieldConfiguration.formatValueLabelExpression) {
            return;
        }
        this.axisStartLabelExpression = this.setupValueLabelExpressionForOption(0);
    }

    private setupAxisEndLabelExpression(): void {
        this.destroyAxisEndLabelExpression();
        if (this.sliderFieldConfiguration.showTickMarkValueLabels
            || !this.sliderFieldConfiguration.showAxisEndValueLabel
            || !this.sliderFieldConfiguration.formatValueLabelExpression) {
            return;
        }
        this.axisEndLabelExpression = this.setupValueLabelExpressionForOption(this.visibleSelectOptions.length - 1);
    }

    private setupValueLabelExpressions(): void {
        this.destroyValueLabelExpressions();
        if (this.sliderFieldConfiguration.showTickMarkValueLabels === false
            || !this.sliderFieldConfiguration.formatValueLabelExpression
        ) {
            return;
        }

        this.valueLabelExpressions = new Array<Expression>();
        for (let i: number = 0; i < this.visibleSelectOptions.length; i++) {
            let expression: Expression = this.setupValueLabelExpressionForOption(i);
            this.valueLabelExpressions.push(expression);
        }
    }

    private setupLegendItemExpressions(): void {
        this.destroyLegendItemExpressions();
        if (this.sliderFieldConfiguration.showLegend === false
            || !this.sliderFieldConfiguration.formatLegendItemExpression
        ) {
            return;
        }

        this.legendItemExpressions = new Array<Expression>();
        for (let i: number = 0; i < this.visibleSelectOptions.length; i++) {
            let expression: Expression = this.setupLegendItemExpressionForOption(i);
            this.legendItemExpressions.push(expression);
        }
    }

    private setupValueLabelExpressionForOption(optionIndex: number): Expression {
        let expressionSource: string = this.sliderFieldConfiguration.formatValueLabelExpression;
        let selectOption: SelectOption = this.visibleSelectOptions[optionIndex];
        let expression: Expression = new Expression(
            expressionSource,
            this.expressionDependencies,
            this.fieldKey
            + `slider value label for option ${optionIndex}: ${this.visibleSelectOptions[optionIndex].label}`,
            this.getFixedArgumentsForSelectOptionExpression(selectOption),
            this.getObservableArgumentsForSelectOptionExpression(selectOption),
            this.scope);
        let firstResult: boolean = true;
        expression.nextResultObservable.pipe(takeUntil(selectOption.destroyed))
            .subscribe((label: string) => {
                if (firstResult) {
                    firstResult = false;
                } else {
                    this.reconfigureSliderSubject.next();
                }
            });
        expression.triggerEvaluation();
        return expression;
    }

    private setupLegendItemExpressionForOption(optionIndex: number): Expression {
        let expressionSource: string = this.sliderFieldConfiguration.formatLegendItemExpression;
        let selectOption: SelectOption = this.visibleSelectOptions[optionIndex];
        let expression: Expression = new Expression(
            expressionSource,
            this.expressionDependencies,
            this.fieldKey
            + `slider legend item for option ${optionIndex}: ${this.visibleSelectOptions[optionIndex].label}`,
            this.getFixedArgumentsForSelectOptionExpression(selectOption),
            this.getObservableArgumentsForSelectOptionExpression(selectOption),
            this.scope);
        let firstResult: boolean = true;
        expression.nextResultObservable.pipe(takeUntil(selectOption.destroyed))
            .subscribe((label: string) => {
                if (firstResult) {
                    firstResult = false;
                } else {
                    this.reconfigureSliderSubject.next();
                }
            });
        expression.triggerEvaluation();
        return expression;
    }

    private getValueLabelForOption(value: number, labelType: LabelType): string {
        const optionIndex: number = this.getOptionIndexForValue(value);
        let gettingThumbLabel: boolean = labelType == LabelType.Low || labelType == LabelType.High;
        let thumbLabelsEnabled: boolean = this.sliderFieldConfiguration.showThumbValueLabel === undefined
            || this.sliderFieldConfiguration.showThumbValueLabel === true;
        if (gettingThumbLabel && !thumbLabelsEnabled) {
            return '';
        }
        if (!this.sliderFieldConfiguration.showTickMarkValueLabels && !gettingThumbLabel) {
            let showLegend: boolean = this.sliderFieldConfiguration.showLegend !== false;
            if (labelType == LabelType.Floor) {
                if (this.sliderFieldConfiguration.showAxisStartValueLabel === false
                    || (this.sliderFieldConfiguration.showAxisStartValueLabel === undefined && showLegend)
                ) {
                    return '';
                }
            } else if (labelType == LabelType.Ceil) {
                if (this.sliderFieldConfiguration.showAxisEndValueLabel === false
                    || (this.sliderFieldConfiguration.showAxisEndValueLabel === undefined && showLegend)
                ) {
                    return '';
                }
            } else if (labelType == LabelType.TickValue) {
                if (this.sliderFieldConfiguration.showThumbValueLabel === false) {
                    return '';
                }
            }
        }
        let label: string;
        if (gettingThumbLabel && this.sliderFieldConfiguration.formatThumbValueLabelExpression) {
            let thumbLabelExpression: Expression = new Expression(
                this.sliderFieldConfiguration.formatThumbValueLabelExpression,
                this.expressionDependencies,
                this.fieldKey + ' thumb label',
                this.getFixedArgumentsForSelectOptionExpression(this.visibleSelectOptions[optionIndex]),
                this.getObservableArgumentsForSelectOptionExpression(this.visibleSelectOptions[optionIndex]),
                this.scope);
            label = thumbLabelExpression.evaluateAndDispose();
        } else if (this.valueLabelExpressions) {
            label = this.valueLabelExpressions[optionIndex].latestResult;
        } else {
            label = this.visibleSelectOptions[optionIndex].label;
        }
        if (gettingThumbLabel) {
            return '<div class="bubble-contents-container"><div class="bubble-arrow"></div>'
                + label + '</div>';
        } else {
            if (this.skipLegendItems > 0) {
                // if there's not enough space, we'll skip rendering some legend items.
                if (optionIndex % (this.skipLegendItems + 1) != 0) {
                    return '';
                }
            }
            return label;
        }
    }

    private getOptionIndexForValue(value: number): number {
        if (isNaN(value)) {
            return 0;
        }
        if (this.sliderFieldConfiguration.continuous !== false) {
            const stepInterval: number = this.totalStepsContinuous / (this.visibleSelectOptions.length - 1);
            const step: number = value / stepInterval;
            return Math.round(step);
        } else {
            return value;
        }
    }

    private getStepIndexFromValue(value: number): number {
        for (let i: number = 0; i < this.visibleSelectOptions.length; i++) {
            if (value <= Number(this.visibleSelectOptions[i].value)) {
                return i;
            }
        }
    }

    private getTotalStepsContinuous(maxValue: number): number {
        if (maxValue >= 5000) {
            this.scalingMultiplier = 1;
            return maxValue;
        } else if (maxValue < 10) {
            this.scalingMultiplier = 1000;
        } else if (maxValue < 20) {
            this.scalingMultiplier = 500;
        } else if (maxValue < 50) {
            this.scalingMultiplier = 200;
        } else if (maxValue < 100) {
            this.scalingMultiplier = 100;
        } else if (maxValue < 250) {
            this.scalingMultiplier = 40;
        } else if (maxValue < 500) {
            this.scalingMultiplier = 20;
        } else if (maxValue < 1000) {
            this.scalingMultiplier = 10;
        } else if (maxValue < 2500) {
            this.scalingMultiplier = 4;
        } else if (maxValue < 5000) {
            this.scalingMultiplier = 2;
        }
        return maxValue * this.scalingMultiplier;
    }

    private getValueForOptionIndex(index: number): number {
        if (this.sliderFieldConfiguration.continuous !== false) {
            const stepInterval: number = this.totalStepsContinuous / (this.visibleSelectOptions.length - 1);
            return index * stepInterval;
        } else {
            return index;
        }
    }

    private getLegendItemForOption(value: number): string {
        const index: number = this.getOptionIndexForValue(value);
        if (this.skipLegendItems > 0) {
            // if there's not enough space, we'll skip rendering some legend items.
            if (index % (this.skipLegendItems + 1) != 0) {
                return '';
            }
        }
        if (this.legendItemExpressions && this.legendItemExpressions[index]) {
            return this.legendItemExpressions[index].latestResult;
        } else if (this.visibleSelectOptions[index]) {
            return this.visibleSelectOptions[index].label;
        } else {
            return '';
        }
    }

    private getLegendItemForStep(step: CustomStepDefinition): string {
        if (this.skipLegendItems > 0) {
            const index: number = this.stepsArray.findIndex((value: any) => step.value == value);
            // if there's not enough space, we'll skip rendering some legend items.
            if (index % (this.skipLegendItems + 1) != 0) {
                return '';
            }
        }
        if (this.legendItemExpressions && this.legendItemExpressions[step.value]) {
            return this.legendItemExpressions[step.value].latestResult;
        } else if (this.visibleSelectOptions[step.value]) {
            return this.visibleSelectOptions[step.value].label;
        } else {
            return '';
        }
    }

    protected onSelectOptionsListChange(): void {
        this.visibleSelectOptions = this.selectOptions
            .filter((selectOption: SelectOption) => selectOption.render && !selectOption.disabled);
        this.numberOfSelectOptionsShown = this.visibleSelectOptions.length;
        if (this.numberOfSelectOptionsShown > 0) {
            if (!this.sliderConfigured) {
                this.sliderConfigured = true;
                this.configureSlider();
            } else {
                this.needsReconfiguration = true;
                this.reconfigureSliderSubject.next();
            }
        }
    }

    private destroyAxisStartValueExpression(): void {
        if (this.axisStartValueExpression) {
            this.axisStartValueExpression.dispose();
            delete this.axisStartValueExpression;
        }
    }

    private destroyAxisEndValueExpression(): void {
        if (this.axisEndValueExpression) {
            this.axisEndValueExpression.dispose();
            delete this.axisEndValueExpression;
        }
    }

    private destroyMinSelectableValueExpression(): void {
        if (this.minSelectableValueExpression) {
            this.minSelectableValueExpression.dispose();
            delete this.minSelectableValueExpression;
        }
    }

    private destroyMaxSelectableValueExpression(): void {
        if (this.maxSelectableValueExpression) {
            this.maxSelectableValueExpression.dispose();
            delete this.maxSelectableValueExpression;
        }
    }

    private destroyStepIntervalExpression(): void {
        if (this.stepIntervalExpression) {
            this.stepIntervalExpression.dispose();
            delete this.stepIntervalExpression;
        }
    }

    private destroyTickMarkIntervalExpression(): void {
        if (this.tickMarkIntervalExpression) {
            this.tickMarkIntervalExpression.dispose();
            delete this.tickMarkIntervalExpression;
        }
    }

    private destroyTickMarkStepIndexArrayExpression(): void {
        if (this.tickMarkStepIndexArrayExpression) {
            this.tickMarkStepIndexArrayExpression.dispose();
            delete this.tickMarkStepIndexArrayExpression;
        }
    }

    private destroyShowSelectionBarFromValueExpression(): void {
        if (this.showSelectionBarFromValueExpression) {
            this.showSelectionBarFromValueExpression.dispose();
            delete this.showSelectionBarFromValueExpression;
        }
    }

    private configureSliderForNumbers(): void {
        this.defaultLegendItemWidthPixels = 40;
        this.rotateLabelsToFit = this.sliderFieldConfiguration.rotateLabelsToFit ?? false;
        this.setupAxisStartValue();
        this.setupAxisEndValue();
        this.setupMinSelectableValue();
        this.setupMaxSelectableValue();
        this.setupStepInterval();
        this.setupTickMarkInterval();
        this.setupTickMarkStepIndexArray();
        this.setupShowSelectionBarFromValue();
        const initialSliderValue: any = this.formControl.value != null && !isNaN(this.formControl.value)
            ? this.formControl.value
            : this.axisStartValue;
        this.sliderValue = this.shadowSliderValue = initialSliderValue;
        if (this.tickMarkInterval === 0) {
            this.tickMarkInterval = null;
        }
        this.showTickMarks = this.sliderFieldConfiguration.showTickMarks !== false
            && ((this.tickMarkInterval != null && this.tickMarkInterval > 0)
                || (this.stepInterval != null && this.stepInterval > 0)
                || (this.tickMarkStepIndexArray != null && this.tickMarkStepIndexArray.length > 0));
        this.tickMarkStepIndexesDebug = this.tickMarkStepIndexArray != null
            ? this.tickMarkStepIndexArray.join(',')
            : null;

        let range: number = this.axisEndValue - this.axisStartValue;
        let stepInterval: number = this.stepInterval;
        if (range >= this.totalStepsContinuous) {
            this.sliderFieldConfiguration.continuous = false;
        } else if (this.sliderFieldConfiguration.continuous !== false || this.stepInterval == null) {
            stepInterval = range / this.totalStepsContinuous;
            stepInterval = NumberHelper.roundPrecisionError(stepInterval);
        }

        if (this.stepInterval == null || this.stepInterval == undefined) {
            this.stepInterval = stepInterval;
            if (range > this.totalStepsContinuous && this.sliderFieldConfiguration.snapToStep == null) {
                this.sliderFieldConfiguration.snapToStep = false;
            }
        }

        if (this.showTickMarks) {
            this.tickMarkInterval = this.tickMarkInterval ?? this.stepInterval;
            if (this.tickMarkInterval) {
                const totalTicks: number = (this.axisEndValue - this.axisStartValue) / this.tickMarkInterval;
                // put in a hard limit of 200 on tick marks, for safety.
                if (totalTicks > 200) {
                    this.showTickMarks = false;
                }
            }
        }

        // if the sliderValue is not one of the steps, then make it one.
        this.sliderValue = this.shadowSliderValue = this.getStepValueForValue(this.sliderValue);

        this.sliderConfigOptions = {
            floor: this.axisStartValue,
            ceil: this.axisEndValue,
            minLimit: this.minSelectableValue,
            maxLimit: this.maxSelectableValue,
            step: stepInterval,
            tickStep: this.showTickMarks ? this.tickMarkInterval || this.stepInterval : null,
            ticksArray: this.showTickMarks ? this.tickMarkStepIndexArray : null,
            showTicks: this.showTickMarks,
            showTicksValues: this.showTickMarks ? this.sliderFieldConfiguration.showTickMarkValueLabels : null,
            keyboardSupport: this.sliderFieldConfiguration.continuous === false,
            vertical: this.vertical,
            rightToLeft: this.sliderFieldConfiguration.invertAxis ?? false,
            showSelectionBar: this.showSelectionBarFromAxisStart,
            showSelectionBarEnd: this.sliderFieldConfiguration.showSelectionBarFromAxisEnd ?? false,
            showSelectionBarFromValue: this.showSelectionBarFromValue,
            disabled: this.disabled,
            readOnly: this.readOnly,
            translate: this.getValueLabelForNumber.bind(this),
            getLegend: this.sliderFieldConfiguration.showLegend
                && this.sliderFieldConfiguration.formatLegendItemExpression
                ? this.getLegendForNumber.bind(this)
                : null,
        };

        this.setupSliderValueBelowMinOrAboveMax();
    }

    private getValueLabelForNumber(value: number, labelType: LabelType): string {
        let stepValue: number = this.getStepValueForValue(value);
        let gettingThumbLabel: boolean = labelType == LabelType.Low || labelType == LabelType.High;
        let thumbLabelsEnabled: boolean = this.sliderFieldConfiguration.showThumbValueLabel === undefined
            || this.sliderFieldConfiguration.showThumbValueLabel === true;
        if (gettingThumbLabel && !thumbLabelsEnabled) {
            return '';
        }
        if (!this.sliderFieldConfiguration.showTickMarkValueLabels && !gettingThumbLabel) {
            let showLegend: boolean = this.sliderFieldConfiguration.showLegend !== false;
            if (labelType == LabelType.Floor) {
                if (this.sliderFieldConfiguration.showAxisStartValueLabel === false
                    || (this.sliderFieldConfiguration.showAxisStartValueLabel === undefined && showLegend)) {
                    return '';
                }
            } else if (labelType == LabelType.Ceil) {
                if (this.sliderFieldConfiguration.showAxisEndValueLabel === false
                    || (this.sliderFieldConfiguration.showAxisEndValueLabel === undefined && showLegend)) {
                    return '';
                }
            } else if (labelType == LabelType.TickValue) {
                if (this.sliderFieldConfiguration.showThumbValueLabel === false) {
                    return '';
                }
            }
        }
        let label: string;
        if (gettingThumbLabel && this.sliderFieldConfiguration.formatThumbValueLabelExpression) {
            let fixedArgs: FixedArguments = this.getFixedExpressionArguments();
            fixedArgs.value = stepValue;
            let thumbLabelExpression: Expression = new Expression(
                this.sliderFieldConfiguration.formatThumbValueLabelExpression,
                this.expressionDependencies,
                this.fieldKey + ' thumb label',
                fixedArgs,
                this.getObservableExpressionArguments(),
                this.scope);
            label = thumbLabelExpression.evaluateAndDispose();
        } else if (this.sliderFieldConfiguration.formatValueLabelExpression) {
            let fixedArgs: FixedArguments = this.getFixedExpressionArguments();
            fixedArgs.value = stepValue;
            let valueLabelExpression: Expression = new Expression(
                this.sliderFieldConfiguration.formatValueLabelExpression,
                this.expressionDependencies,
                this.fieldKey + ' value label',
                fixedArgs,
                this.getObservableExpressionArguments(),
                this.scope);
            label = valueLabelExpression.evaluateAndDispose();
        } else {
            if (this.interactiveFieldConfiguration.dataType == FieldDataType.Currency) {
                label = this.expressionDependencies.expressionMethodService
                    .currencyAsString(stepValue, null, null);
            } else if (this.interactiveFieldConfiguration.dataType == FieldDataType.Percent) {
                label = stepValue + '%';
            } else {
                label = stepValue.toString();
            }
        }
        if (gettingThumbLabel) {
            return '<div class="bubble-contents-container"><div class="bubble-arrow"></div>'
                + label + '</div>';
        } else {
            if (this.shouldSkipDueToCrowding(value)) {
                return '';
            }
            return label;
        }
    }

    private getStepValueForValue(value: number): number {
        if (this.sliderFieldConfiguration.continuous !== false) {
            const remainder: number = value % this.stepInterval;
            let result: number;
            if (remainder < this.stepInterval / 2) {
                result = value - remainder;
            } else {
                result = value - remainder + this.stepInterval;
            }
            return NumberHelper.roundPrecisionError(result);
        } else {
            return value;
        }
    }

    private shouldSkipDueToCrowding(value: number): boolean {
        if (this.skipLegendItems > 0) {
            const index: number = Math.round(value / this.tickMarkInterval);
            // if there's not enough space, we'll skip rendering some legend items.
            if (index % (this.skipLegendItems + 1) != 0) {
                return true;
            }
        }
        return false;
    }

    private getLegendForNumber(value: number): string {
        if (this.shouldSkipDueToCrowding(value)) {
            return '';
        }
        let fixedArgs: FixedArguments = this.getFixedExpressionArguments();
        fixedArgs.value = value;
        let legendItemExpression: Expression = new Expression(
            this.sliderFieldConfiguration.formatLegendItemExpression,
            this.expressionDependencies,
            this.fieldKey + ' legend item',
            fixedArgs,
            this.getObservableExpressionArguments(),
            this.scope);
        return legendItemExpression.evaluateAndDispose();
    }

    private setupAxisStartValue(): void {
        this.destroyAxisStartValueExpression();
        if (this.sliderFieldConfiguration.axisStartValueExpression) {
            this.setupAxisStartValueExpression();
        } else {
            this.axisStartValue = 0;
        }
    }

    private setupAxisStartValueExpression(): void {
        this.axisStartValueExpression = new Expression(
            this.sliderFieldConfiguration.axisStartValueExpression,
            this.expressionDependencies,
            this.fieldKey + ' axis start value',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.axisStartValueExpression.nextResultObservable.subscribe((value: number) => {
            this.axisStartValue = NumberHelper.isNumber(value) ? NumberHelper.roundPrecisionError(value) : 0;
            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.axisStartValueExpression.triggerEvaluation();
    }

    private setupAxisEndValue(): void {
        this.destroyAxisEndValueExpression();
        if (this.sliderFieldConfiguration.axisEndValueExpression) {
            this.setupAxisEndValueExpression();
        } else {
            this.axisEndValue = 100;
        }
    }

    private setupAxisEndValueExpression(): void {
        this.axisEndValueExpression = new Expression(
            this.sliderFieldConfiguration.axisEndValueExpression,
            this.expressionDependencies,
            this.fieldKey + ' axis end value',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.axisEndValueExpression.nextResultObservable.subscribe((value: number) => {
            this.axisEndValue = NumberHelper.isNumber(value) ? NumberHelper.roundPrecisionError(value) : 100;
            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.axisEndValueExpression.triggerEvaluation();
    }

    private setupMinSelectableValue(): void {
        this.destroyMinSelectableValueExpression();
        if (this.sliderFieldConfiguration.minSelectableValueExpression) {
            this.setupMinSelectableValueExpression();
        } else {
            this.minSelectableValue = null;
        }
    }

    private setupMinSelectableValueExpression(): void {
        this.minSelectableValueExpression = new Expression(
            this.sliderFieldConfiguration.minSelectableValueExpression,
            this.expressionDependencies,
            this.fieldKey + ' min selectable value',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.minSelectableValueExpression.nextResultObservable.subscribe((value: number) => {
            const minSelectableValue: number = NumberHelper.isNumber(value)
                ? NumberHelper.roundPrecisionError(value)
                : null;

            this.minSelectableValue = this.usesOptions
                ? this.getSelectedOptionIndexForValue(minSelectableValue)
                : minSelectableValue;
            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.minSelectableValueExpression.triggerEvaluation();
    }

    private setupMaxSelectableValue(): void {
        this.destroyMaxSelectableValueExpression();
        if (this.sliderFieldConfiguration.maxSelectableValueExpression) {
            this.setupMaxSelectableValueExpression();
        } else {
            this.maxSelectableValue = null;
        }
    }

    private setupMaxSelectableValueExpression(): void {
        this.maxSelectableValueExpression = new Expression(
            this.sliderFieldConfiguration.maxSelectableValueExpression,
            this.expressionDependencies,
            this.fieldKey + ' max selectable value',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.maxSelectableValueExpression.nextResultObservable.subscribe((value: any) => {
            const maxSelectableValue: number = NumberHelper.isNumber(value)
                ? NumberHelper.roundPrecisionError(value)
                : null;

            this.maxSelectableValue = this.usesOptions
                ? this.getSelectedOptionIndexForValue(maxSelectableValue)
                : maxSelectableValue;

            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.maxSelectableValueExpression.triggerEvaluation();
    }

    private setupStepInterval(): void {
        this.destroyStepIntervalExpression();
        if (this.sliderFieldConfiguration.stepIntervalExpression) {
            this.setupStepIntervalExpression();
        } else {
            if (this.sliderFieldConfiguration.axisStartValueExpression == null
                && this.sliderFieldConfiguration.axisEndValueExpression == null
            ) {
                this.stepInterval = 1;
            } else {
                this.stepInterval = null;
            }
        }
    }

    private setupStepIntervalExpression(): void {
        this.stepIntervalExpression = new Expression(
            this.sliderFieldConfiguration.stepIntervalExpression,
            this.expressionDependencies,
            this.fieldKey + ' step interval',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.stepIntervalExpression.nextResultObservable.subscribe((value: any) => {
            this.stepInterval = NumberHelper.isNumber(value) ? NumberHelper.roundPrecisionError(value) : null;
            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.stepIntervalExpression.triggerEvaluation();
    }

    private setupTickMarkInterval(): void {
        this.destroyTickMarkIntervalExpression();
        if (this.sliderFieldConfiguration.tickMarkIntervalExpression) {
            this.setupTickMarkIntervalExpression();
        } else {
            this.tickMarkInterval = null;
        }
    }

    private setupTickMarkIntervalExpression(): void {
        this.tickMarkIntervalExpression = new Expression(
            this.sliderFieldConfiguration.tickMarkIntervalExpression,
            this.expressionDependencies,
            this.fieldKey + ' step interval',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.tickMarkIntervalExpression.nextResultObservable.subscribe((value: any) => {
            this.tickMarkInterval = NumberHelper.isNumber(value) ? NumberHelper.roundPrecisionError(value) : null;
            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.tickMarkIntervalExpression.triggerEvaluation();
    }

    private setupShowSelectionBarFromValue(): void {
        this.destroyShowSelectionBarFromValueExpression();
        if (this.sliderFieldConfiguration.showSelectionBarFromValueExpression) {
            this.setupShowSelectionBarFromValueExpression();
        } else {
            this.showSelectionBarFromValue = null;
        }
    }

    private setupShowSelectionBarFromValueExpression(): void {
        this.showSelectionBarFromValueExpression = new Expression(
            this.sliderFieldConfiguration.showSelectionBarFromValueExpression,
            this.expressionDependencies,
            this.fieldKey + ' show selection bar from value',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.showSelectionBarFromValueExpression.nextResultObservable.subscribe((value: any) => {
            this.showSelectionBarFromValue
                = NumberHelper.isNumber(value) ? NumberHelper.roundPrecisionError(value) : null;
            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.showSelectionBarFromValueExpression.triggerEvaluation();
    }

    private setupTickMarkStepIndexArray(): void {
        this.destroyTickMarkStepIndexArrayExpression();
        if (this.sliderFieldConfiguration.tickMarkStepIndexArrayExpression) {
            this.setupTickMarkStepIndexArrayExpression();
        } else {
            this.tickMarkStepIndexArray = null;
        }
    }

    private setupTickMarkStepIndexArrayExpression(): void {
        this.tickMarkStepIndexArrayExpression = new Expression(
            this.sliderFieldConfiguration.tickMarkStepIndexArrayExpression,
            this.expressionDependencies,
            this.fieldKey + ' tick mark step index array',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstResult: boolean = true;
        this.tickMarkStepIndexArrayExpression.nextResultObservable.subscribe((result: Array<number>) => {
            if (result != null && Array.isArray(result) && result.length > 0) {
                this.tickMarkStepIndexArray = result;
            } else {
                this.tickMarkStepIndexArray = null;
            }
            if (firstResult) {
                firstResult = false;
            } else {
                this.reconfigureSliderSubject.next();
            }
        });
        this.tickMarkStepIndexArrayExpression.triggerEvaluation();
    }

    public onValueChange($event: any): void {
        if (this.readOnly || this.disabled) {
            // readonly/disabled fields should never be getting changes coming through from the slider widget.
            return;
        }
        let value: any = this.getValueForSliderValue(this.sliderValue);
        if (this.formControl.value !== value) {
            this.formService.setFieldValueUnstable(this.fieldPath);
            this.formControlValueChangeSubject.next(value);
        }
    }

    private getValueForSliderValue(sliderValue: number): any {
        let result: any;
        if (this.usesOptions) {
            const index: number = this.getOptionIndexForValue(this.sliderValue);
            result = this.visibleSelectOptions[index].value;
        } else {
            let value: number = sliderValue;
            if (this.sliderFieldConfiguration.continuous !== false
                && this.sliderFieldConfiguration.snapToStep !== false
            ) {
                result = this.getStepValueForValue(value);
            }
            if (this.minSelectableValue !== null && value < this.minSelectableValue) {
                result = this.minSelectableValue;
            } else if (this.maxSelectableValue !== null && value > this.maxSelectableValue) {
                result = this.maxSelectableValue;
            } else {
                result = value;
            }
        }
        return result;
    }

    private throttleOrDebounceFormControlValueChange(): void {
        if (this.formControlValueChangeSubscription) {
            this.formControlValueChangeSubscription.unsubscribe();
            this.formControlValueChangeSubscription = null;
        }
        const shouldDebounce: boolean = this.sliderFieldConfiguration.debounceFieldValueUpdates !== false;
        const debounceMillis: number = this.sliderFieldConfiguration.fieldValueUpdateDebounceMilliseconds ?? 500;
        this.formControlValueChangeSubscription = this.formControlValueChangeSubject.pipe(
            shouldDebounce
                ? debounceTime(debounceMillis)
                : throttleTime(100, asyncScheduler, { leading: false, trailing: true }),
            takeUntil(this.destroyed),
        ).subscribe((value: any) => {
            if (!this.readOnly && !this.disabled && !this.needsReconfiguration
                && this.formControl.value !== value
            ) {
                this.setValue(value);
            }
            this.onStatusChange();
            this.formService.setFieldValueUnstable(this.fieldPath, false);
        });
    }


    /**
     * We're overriding Field.onChange here because we have implemented the debounce field value updates
     * locally here in the Slider field so we don't want it to double debounce.
     */
    public onChange(event: any = null): void {
        if (this.getValueForExpressions() !== this.valueSubject.value) {
            this.publishChange();
        }
    }

    private throttleSliderValueChange(): void {
        this.sliderValueChangeSubject.pipe(
            throttleTime(100, asyncScheduler, { leading: false, trailing: true }),
            takeUntil(this.destroyed),
        ).subscribe((value: any) => {
            if (this.sliderFieldConfiguration.continuous !== false
                && this.sliderFieldConfiguration.snapToStep !== false
            ) {
                this.sliderValue = this.getStepValueForValue(value);
            } else {
                this.sliderValue = value;
            }
        });
    }

    public onUserChangeStart(changeContext: ChangeContext): void {
        this.formService.setFieldValueUnstable(this.fieldPath);
        this.showBubble = true;
        this.sliderChangeSubject.next();
    }

    public onUserChange(changeContext: ChangeContext): void {
        this.showBubble = true;
        this.sliderChangeSubject.next();
    }

    public onUserChangeEnd(changeContext: ChangeContext): void {
        if (this.readOnly || this.disabled) {
            // readonly/disabled fields should never be getting changes coming through from the slider widget.
            return;
        }
        this.shadowSliderValue = changeContext.value;
        if (this.sliderFieldConfiguration.continuous !== false
            && this.sliderFieldConfiguration.snapToStep !== false
        ) {
            this.sliderValueChangeSubject.next(this.shadowSliderValue);
        }
    }

    private onExpressionValueChanges(): void {
        if (this.calculatedValueSubject) {
            this.calculatedValueSubject.pipe(
                takeUntil(this.destroyed),
                debounceTime(100),
            ).subscribe((value: any) => {
                if (!this.disabled) {
                    if (this.usesOptions && this.isOptionSetNumeric) {
                        let stepIndex: number = this.getStepIndexFromValue(value);
                        const stepInterval: number = this.totalStepsContinuous / (this.visibleSelectOptions.length - 1);
                        value = stepIndex * stepInterval;
                    }
                    this.shadowSliderValue = value;
                    this.sliderValueChangeSubject.next(this.shadowSliderValue);
                }
            });
        }
    }

    private determineIfOptionSetIsNumeric(): void {
        let isFirstOptionNumeric: boolean = this.visibleSelectOptions.length > 0
            && NumberHelper.isNumber(this.visibleSelectOptions[0].value);
        let isSecondOptionNumeric: boolean = this.visibleSelectOptions.length > 1
            && NumberHelper.isNumber(this.visibleSelectOptions[1].value);
        this.isOptionSetNumeric = isFirstOptionNumeric && isSecondOptionNumeric;
    }

    public onKeyDown(event: KeyboardEvent): void {
        if (this.sliderFieldConfiguration.continuous !== false && !this.disabled && !this.readOnly) {
            if (this.usesOptions) {
                const maxValue: number = this.visibleSelectOptions.length - 1;
                const minValue: number = 0;
                const tenPercent: number = (maxValue - minValue) / 10;
                const minPageChange: number = Math.max(tenPercent, 1);
                if (event.key == 'ArrowUp' || event.key == 'ArrowRight') {
                    const selectedOptionIndex: number = this.getSelectedOptionIndexForValue(this.formControl.value);
                    if (selectedOptionIndex < this.visibleSelectOptions.length - 1) {
                        this.sliderValueChangeSubject.next(this.getValueForOptionIndex(selectedOptionIndex + 1));
                    }
                } else if (event.key == 'PageUp') {
                    const selectedOptionIndex: number = this.getSelectedOptionIndexForValue(this.formControl.value);
                    const newValue: number
                        = Math.min(Math.round(selectedOptionIndex + minPageChange), maxValue);
                    this.sliderValueChangeSubject.next(this.getValueForOptionIndex(newValue));
                } else if (event.key == 'ArrowDown' || event.key == 'ArrowLeft') {
                    const selectedOptionIndex: number = this.getSelectedOptionIndexForValue(this.formControl.value);
                    if (selectedOptionIndex > 0) {
                        this.sliderValueChangeSubject.next(this.getValueForOptionIndex(selectedOptionIndex - 1));
                    }
                } else if (event.key == 'PageDown') {
                    const selectedOptionIndex: number = this.getSelectedOptionIndexForValue(this.formControl.value);
                    const newValue: number
                        = Math.max(Math.round(selectedOptionIndex - minPageChange), minValue);
                    this.sliderValueChangeSubject.next(this.getValueForOptionIndex(newValue));
                } else if (event.key == 'End') {
                    this.sliderValueChangeSubject.next(
                        this.getValueForOptionIndex(this.visibleSelectOptions.length - 1));
                } else if (event.key == 'Home') {
                    this.sliderValueChangeSubject.next(this.getValueForOptionIndex(0));
                }
            } else {
                const maxValue: number = this.maxSelectableValue ?? this.axisEndValue;
                const minValue: number = this.minSelectableValue ?? this.axisStartValue;
                const tenPercent: number = (maxValue - minValue) / 10;
                if (event.key == 'ArrowUp' || event.key == 'ArrowRight') {
                    const movementAmount: number
                        = Math.max(this.getStepsPerPixel() * this.stepInterval, this.stepInterval);
                    const newValue: number
                        = Math.min(this.shadowSliderValue + movementAmount, maxValue);
                    this.shadowSliderValue = newValue;
                    this.sliderValueChangeSubject.next(this.shadowSliderValue);
                } else if (event.key == 'PageUp') {
                    const newValue: number
                        = Math.min(this.shadowSliderValue + tenPercent, maxValue);
                    this.shadowSliderValue = newValue;
                    this.sliderValueChangeSubject.next(this.shadowSliderValue);
                } else if (event.key == 'ArrowDown' || event.key == 'ArrowLeft') {
                    const movementAmount: number
                        = Math.max(this.getStepsPerPixel() * this.stepInterval, this.stepInterval);
                    const newValue: number
                        = Math.max(this.shadowSliderValue - movementAmount, minValue);
                    this.shadowSliderValue = newValue;
                    this.sliderValueChangeSubject.next(this.shadowSliderValue);
                } else if (event.key == 'PageDown') {
                    const newValue: number
                        = Math.max(this.shadowSliderValue - tenPercent, minValue);
                    this.shadowSliderValue = newValue;
                    this.sliderValueChangeSubject.next(this.shadowSliderValue);
                } else if (event.key == 'End') {
                    this.shadowSliderValue = maxValue;
                    this.sliderValueChangeSubject.next(this.shadowSliderValue);
                } else if (event.key == 'Home') {
                    this.shadowSliderValue = minValue;
                    this.sliderValueChangeSubject.next(this.shadowSliderValue);
                }
            }
            event.preventDefault();
            this.onUserChange(null);
        }
    }

    private debounceHideBubble(): void {
        this.sliderChangeSubject.pipe(
            debounceTime(this.hideBubbleDebounceTimeMillis),
            takeUntil(this.destroyed),
        ).subscribe(() => {
            this.showBubble = false;
        });
    }

    private orientLegend(): void {
        if (this.vertical || !this.showLegend) {
            return;
        }
        const numberOfLegendItems: number = this.getNumberOfLegendItems();
        const sliderWidth: number = this.sliderEl.getBoundingClientRect().width;
        let legendItemEl: HTMLElement
            = <HTMLElement>this.sliderEl.getElementsByClassName('ngx-slider-tick-legend')[0];
        if (!legendItemEl) {
            legendItemEl = <HTMLElement>this.sliderEl.getElementsByClassName('ngx-slider-tick-value')[0];
        }
        let legendItemWidth: number = this.defaultLegendItemWidthPixels;
        if (this.stylingSettings.legend?.itemWidth) {
            if (this.stylingSettings.legend.itemWidth.endsWith('px')) {
                legendItemWidth = Number.parseInt(this.stylingSettings.legend.itemWidth, 10);
            } else {
                if (legendItemEl) {
                    legendItemWidth = legendItemEl.clientWidth;
                }
            }
        }
        const legendItemHeight: number = legendItemEl ? Math.max(legendItemEl.clientHeight, 18) : 18;
        const numItemsThatCanFitHorizontally: number = Math.floor(sliderWidth / legendItemWidth);
        if (numItemsThatCanFitHorizontally < numberOfLegendItems) {
            if (this.rotateLabelsToFit) {
                const numItemsThatCanFitVertically: number = Math.floor(sliderWidth / legendItemHeight);
                if (numItemsThatCanFitVertically < (numberOfLegendItems * 1.75)) {
                    this.legendItemsVertical = true;
                    this.legendItemsDiagonal = false;
                    this.skipLegendItems
                        = Math.floor((numberOfLegendItems * 1.1) / numItemsThatCanFitVertically);
                } else {
                    this.legendItemsVertical = false;
                    this.legendItemsDiagonal = true;
                    this.skipLegendItems = 0;
                }
            } else {
                this.legendItemsVertical = false;
                this.legendItemsDiagonal = false;
                if (numItemsThatCanFitHorizontally < numberOfLegendItems) {
                    this.skipLegendItems
                        = Math.floor(numberOfLegendItems / numItemsThatCanFitHorizontally);
                } else {
                    this.skipLegendItems = 0;
                }
            }
        } else {
            this.legendItemsVertical = false;
            this.legendItemsDiagonal = false;
            this.skipLegendItems = 0;
        }
    }

    private getNumberOfLegendItems(): number {
        if (this.usesOptions) {
            return this.visibleSelectOptions.length;
        } else {
            return ((this.axisEndValue - this.axisStartValue) / this.tickMarkInterval) + 1;
        }
    }

    /**
     * @returns the number of steps in a pixel, so that when we're moving using the keyboard, we will at least move by
     * one pixel.
     * This is applicable to continuous sliders.
     */
    private getStepsPerPixel(): number {
        const ngxSliderEl: HTMLElement = this.elementRef.nativeElement.getElementsByTagName('ngx-slider')[0];
        const totalPixels: number = ngxSliderEl.clientWidth - 32;
        const totalSteps: number = (this.axisEndValue - this.axisStartValue) / this.stepInterval;
        return totalSteps / totalPixels;
    }

    /**
     * This is to calculate the slider color from the given selectionBar color into HSL. 
     * Also checks if an accent color is to be used, else use the selection bar color
     * HSL is hue, saturation and lightness. this is to set all the property color of the slider.
     */
    private generateSliderColorTheme(): void {
        let selectionBarColor: string = this.stylingSettings.selectionBar?.color;
        let accentColor1: string = this.applicationService.embedOptions?.accentColor1;
        let accentColor2: string = this.applicationService.embedOptions?.accentColor2;
        let accentColor3: string = this.applicationService.embedOptions?.accentColor3;
        let accentColor4: string = this.applicationService.embedOptions?.accentColor4;

        let accentColor: string = accentColor1
            ? accentColor1
            : accentColor2
                ? accentColor2
                : accentColor3
                    ? accentColor3
                    : accentColor4
                        ? accentColor4
                        : null;
        selectionBarColor = ColorHelper.getComputedColor(selectionBarColor, this.sliderColorReference);
        let color: string = selectionBarColor ? selectionBarColor : accentColor;
        let colorHsl: ColorHsl = ColorHelper.convertCssColorToHsl(color);

        if (colorHsl) {
            this.sliderColorH = colorHsl.hue;
            this.sliderColorS = `${colorHsl.saturation}%`;
            this.sliderColorL = `${colorHsl.lightness}%`;

            let unselectedBarColor: ColorHsl = ColorHelper.getUnselectedBarColor(colorHsl);
            this.unselectedBarColorH = unselectedBarColor.hue;
            this.unselectedBarColorS = unselectedBarColor.saturation;
            this.unselectedBarColorL = unselectedBarColor.lightness;
        }
    }
}
