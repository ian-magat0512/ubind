import { Directive, OnDestroy, OnInit } from "@angular/core";
import { AnyHelper } from "@app/helpers/any.helper";
import { InputMaskGenerator } from "@app/helpers/Input-mask-generator";
import { StringHelper } from "@app/helpers/string.helper";
import { FieldDataType } from "@app/models/field-data-type.enum";
import { InputMask } from "@app/models/input-mask.model";
import { InputFieldMaskConfiguration } from "@app/resource-models/configuration/fields/input-field-mask-configuration";
import { KeyboardInputMode } from "@app/models/keyboard-input-mode.enum";
import { debounceTime, takeUntil } from "rxjs/operators";
import { KeyboardInputField } from "./keyboard-Input-field";
import { Expression } from "@app/expressions/expression";
import { ConditionalInputFieldMaskConfiguration }
    from "@app/resource-models/configuration/fields/conditional-input-field-mask-configuration";
import { Subject } from "rxjs";
import { InputMaskConfiguration } from "@app/resource-models/configuration/fields/input-mask-configuration";
import { InputMaskListModel } from "@app/resource-models/input-mask-list-model";
import { InputMaskType } from "@app/models/input-mask-type.enum";
import { FormService } from "@app/services/form.service";
import { WorkflowService } from "@app/services/workflow.service";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { CalculationService } from "@app/services/calculation.service";
import { ApplicationService } from "@app/services/application.service";
import { FieldMetadataService } from "@app/services/field-metadata.service";
import { EventService } from "@app/services/event.service";
import { FieldEventLogRegistry } from "@app/services/debug/field-event-log-registry";
import { MaskPipe } from "ngx-mask";

/**
 * This class is used to set input masking for the field. An input field can have an inputMask or an inputMaskList. 
 * inputMaskList contains a list of input mask which also has a showWhenExpression property.
 * The way we determine which input mask to use is to check the inputMaskList where showWhenExpression is 
 * evaluated as true and return the first item from the inputMaskList.
 * if there are no inputMaskList items showWhenExpression evaluated as true, then return the input mask.
 */
@Directive()
export abstract class InputFieldWithMasking extends KeyboardInputField implements OnInit, OnDestroy {

    public keyboardInputMode: KeyboardInputMode;
    public inputMask: InputMask;
    public inputMaskList: Array<InputMaskListModel>;
    private inputFieldMaskConfiguration: InputFieldMaskConfiguration;
    private useWhenExpressionSubject: Subject<void> = new Subject<void>();
    private inputMaskListExpressions: Array<Expression>;
    private conditionalInputFieldMaskConfiguration: Array<ConditionalInputFieldMaskConfiguration>;
    private inputMaskConfiguration: InputMaskConfiguration;
    private maskPipe: MaskPipe;

    public constructor(
        protected formService: FormService,
        protected workflowService: WorkflowService,
        protected expressionDependencies: ExpressionDependencies,
        protected calculationService: CalculationService,
        public applicationService: ApplicationService,
        protected fieldMetadataService: FieldMetadataService,
        protected eventService: EventService,
        protected fieldEventLogRegistry: FieldEventLogRegistry,
        protected myMaskPipe: MaskPipe,
    ) {
        super(formService,
            workflowService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
        );
        this.maskPipe = myMaskPipe;
    }


    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: InputMaskConfiguration; new: InputMaskConfiguration}) => {
                this.inputFieldMaskConfiguration = configs.new.inputMask;
                this.configureInputMaskListExpression();
            });
    }

    private configureInputMaskListExpression(): void {
        this.destroyInputMaskListExpressions();
        this.conditionalInputFieldMaskConfiguration = this.inputMaskConfiguration.inputMaskList;
        this.inputFieldMaskConfiguration = this.inputMaskConfiguration.inputMask;
        if(this.conditionalInputFieldMaskConfiguration == undefined) {
            return;
        }
        this.inputMaskListExpressions = new Array<Expression>();
        this.inputMaskList = new Array<InputMaskListModel>();
        this.conditionalInputFieldMaskConfiguration
            .forEach((inputMaskSetting: ConditionalInputFieldMaskConfiguration, index: number) => {
                let inputMaskListModel: InputMaskListModel = new InputMaskListModel();
                inputMaskListModel.inputMaskConfiguration = inputMaskSetting.inputMask;
                let expression: Expression = this.setupUseWhenExpression(inputMaskSetting, inputMaskListModel, index);
                this.inputMaskList.push(inputMaskListModel);
                this.inputMaskListExpressions.push(expression);
            });
    }

    private setupUseWhenExpression(
        inputMaskSetting: ConditionalInputFieldMaskConfiguration,
        inputMaskListModel: InputMaskListModel,
        index: number): Expression {
        let expression: Expression = new Expression(
            inputMaskSetting.useWhenExpression,
            this.expressionDependencies,
            this.fieldKey
            + `${this.fieldKey} ${this.fieldType} input mask ${index}`);
        expression.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe((useWhenExpressionResult: boolean) => {
                inputMaskListModel.evaluatedShowWhenExpression = useWhenExpressionResult;
                this.useWhenExpressionSubject.next();
                this.stripUnwantedValue(inputMaskListModel);

            });
        expression.triggerEvaluation();
        return expression;
    }

    private stripUnwantedValue(inputMaskListModel: InputMaskListModel): void {
        if (inputMaskListModel.evaluatedShowWhenExpression == false) {
            return;
        }
        let value: string = this.formControl.value;
        if (inputMaskListModel.inputMaskConfiguration.type == InputMaskType.Numeric) {
            value = InputMaskGenerator.removeNonInputCharacter(
                value,
                inputMaskListModel.inputMaskConfiguration.prefix,
                inputMaskListModel.inputMaskConfiguration.thousandSeparator);
            if (inputMaskListModel.inputMaskConfiguration.decimalPrecision > 0) {
                value = InputMaskGenerator.formatStringToDecimalWithLimitedPlaces(
                    value,
                    inputMaskListModel.inputMaskConfiguration.decimalPrecision,
                    inputMaskListModel.inputMaskConfiguration.thousandSeparator);
            }
        }

        if (value != this.formControl.value) {
            setTimeout(() => {
                this.formControl.patchValue(value, {
                    onlySelf: true,
                    emitEvent: true,
                    emitModelToViewChange: true,
                    emitViewToModelChange: true,
                });
            }, 200);
        }

    }

    protected initialiseField(): void {
        this.inputMaskConfiguration = this.field.templateOptions.fieldConfiguration;
        this.configureInputMaskListExpression();
        this.debounceInputMaskConfiguration();
        this.applyInputMask();
        super.initialiseField();
    }

    public ngOnDestroy(): void {
        super.destroyExpressions();
        this.destroyInputMaskListExpressions();
        super.ngOnDestroy();
    }

    private debounceInputMaskConfiguration(): void {
        this.useWhenExpressionSubject.pipe(
            takeUntil(this.destroyed),
            debounceTime(100),
        ).subscribe(() => {
            this.applyInputMask();
        });
    }

    private destroyInputMaskListExpressions(): void {
        if (this.inputMaskListExpressions) {
            this.inputMaskListExpressions.forEach((expression: Expression) => {
                expression.dispose();
            });
            delete this.inputMaskListExpressions;
        }
    }

    public onFocus(event: any): void {
        super.onFocus(event);
        if(this.inputMask && this.inputMask.placeholder != "" && AnyHelper.hasNoValue(this.formControl.value)) {
            this.inputMask.placeholderText = "";
        }
    }

    public onBlur(event: any): void {
        if(this.inputMask && this.inputMask.placeholder != "" && AnyHelper.hasNoValue(this.formControl.value)) {
            this.inputMask.placeholderText = this.inputMask.placeholder;
        }
        super.onBlur(event);
    }

    public onChange(event: any): void {
        this.overrideValueOnChanged(this.formControl.value);
        super.onChange(event);
    }

    private getApplicableInputMask(): InputFieldMaskConfiguration {
        if (this.inputMaskList == undefined || this.inputMaskList.length == 0) {
            return this.inputFieldMaskConfiguration;
        }

        let inputMaskList: Array<InputMaskListModel> = this.inputMaskList
            .filter((i: InputMaskListModel)=> i.evaluatedShowWhenExpression == true);

        let applicableInputMask: InputFieldMaskConfiguration = inputMaskList.length > 0
            ? inputMaskList[0].inputMaskConfiguration
            : this.inputFieldMaskConfiguration;
        return applicableInputMask;
    }

    private applyInputMask() {
        const fieldDataType: FieldDataType = this.dataStoringFieldConfiguration.dataType;
        const inputMaskFieldConfiguration: InputFieldMaskConfiguration = this.getApplicableInputMask();
        this.inputMask = InputMaskGenerator.generateInputMask(inputMaskFieldConfiguration, fieldDataType);
    }

    private overrideValueOnChanged(value: any) {
        if (this.inputMask
            && ((this.inputMask.includeSuffixInValue && this.inputMask.suffix)
            || (this.inputMask.hidePrefixWhenInputValueIsEmpty && this.inputMask.prefix)
            || (this.inputMask.isNumericDataType))
        ) {
            let valueChanged: boolean = false;
            if (this.inputMask.includeSuffixInValue && this.inputMask.suffix && !StringHelper.isNullOrEmpty(value)) {
                value = value.endsWith(this.inputMask.suffix) ? value : value + this.inputMask.suffix;
                valueChanged = true;
            }

            if (this.inputMask.hidePrefixWhenInputValueIsEmpty && this.inputMask.prefix) {
                value = value.startsWith(this.inputMask.prefix) ? value.replace(this.inputMask.prefix, "") : value;
                valueChanged = true;
            }

            let numberWithPeriod: boolean = value.toString().trim().endsWith(".");
            if (this.inputMask.isNumericDataType && !numberWithPeriod && !this.inputMask.includeSuffixInValue) {
                if (isNaN(Number(value.toString()))) {
                    value = InputMaskGenerator.removeNonInputCharacter(value.toString(),
                        this.inputMask.thousandSeparator, this.inputMask.prefix);
                    valueChanged = true;
                }
                if (!StringHelper.isNullOrEmpty(value) && !isNaN(Number(value.toString()))) {
                    value = Number(value.toString());
                    valueChanged = true;
                }
            }

            if (valueChanged) {
                this.formControl.patchValue(value, {
                    onlySelf: true,
                    emitEvent: true,
                    emitModelToViewChange: false,
                    emitViewToModelChange: false,
                });
            }
        }
    }
}
