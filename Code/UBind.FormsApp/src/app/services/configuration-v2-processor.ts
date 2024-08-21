import { FieldConfigurationHelper } from "@app/helpers/field-configuration.helper";
import { Field } from "@app/models/configuration/field";
import { FieldType } from "@app/models/field-type.enum";
import { FieldGroup } from "@app/models/configuration/field-group";
import { WorkingConfiguration } from "@app/models/configuration/working-configuration";
import { Errors } from "@app/models/errors";
import { FieldDataType } from "@app/models/field-data-type.enum";
import { QuestionMetadata } from "@app/models/question-metadata";
import { ComponentConfiguration } from "@app/resource-models/configuration/component.configuration";
import { FieldConfiguration } from "@app/resource-models/configuration/fields/field.configuration";
import {
    InteractiveFieldConfiguration,
} from "@app/resource-models/configuration/fields/interactive-field.configuration";
import { VisibleFieldConfiguration } from "@app/resource-models/configuration/fields/visible-field.configuration";
import { OptionConfiguration } from "@app/resource-models/configuration/option.configuration";
import { ThemeConfiguration } from "@app/resource-models/configuration/theme.configuration";
import defaultTextElementsJson from './form-configuration/default-text-elements.json';
import { StringHelper } from "@app/helpers/string.helper";
import * as _ from 'lodash-es';
import {
    SingleLineTextFieldConfiguration,
} from "@app/resource-models/configuration/fields/single-line-text-field.configuration";
import { Validators } from "@angular/forms";
import { EvaluateService } from "./evaluate.service";
import { Injectable } from "@angular/core";
import { FormControlValidatorFunction, ValidationService } from "./validation.service";

/**
 * Processes configuration which comes from the version 2.x formats, and converts it into the standard
 * WorkingConfiguration format.
 */
@Injectable()
export class ConfigurationV2Processor {

    private config: WorkingConfiguration;
    private rawConfig: ComponentConfiguration;

    public constructor(
        private evaluateService: EvaluateService,
        private validationService: ValidationService,
    ) { }

    public process(rawConfig: object): WorkingConfiguration {
        this.config = <WorkingConfiguration>{};
        this.rawConfig = <ComponentConfiguration>rawConfig;
        this.config.icons = new Array<string>();
        this.config.questionMetadata = {
            questionSets: {},
            repeatingQuestionSets: {},
        };
        this.generatePrivateFieldKeys();
        this.generateFieldsOrderedByCalculationWorkbookRow();
        this.generateRepeatingFieldsOrderedByCalculationWorkbookRow();
        this.generateSettings();
        this.processTextElements();
        this.generateTriggers();
        this.generateWorkflowRoles();
        this.processformDataLocator();
        this.processQuestionSets();
        this.processRepeatingQuestionSets();
        this.processOptions();
        this.processContextEntities();
        this.config.workflowSteps = this.rawConfig.form.workflowConfiguration;
        this.generateStyles();
        this.config.formModel = this.rawConfig.form.formModel || {};
        this.config.calculatesUsingStandardWorkbook = this.rawConfig.calculatesUsingStandardWorkbook;
        this.config.repeatingInstanceMaxQuantity = this.rawConfig.form.repeatingInstanceMaxQuantity;
        return this.config;
    }

    private generatePrivateFieldKeys(): void {
        this.config.privateFieldKeys = new Array<string>();
        for (const questionSet of this.rawConfig.form.questionSets) {
            for (const field of questionSet.fields) {
                if (FieldConfigurationHelper.isDataStoringField(field) && field.private) {
                    this.config.privateFieldKeys.push(field.key);
                }
            }
        }
    }

    private processQuestionSets(): void {
        if (!this.config.questionSets) {
            this.config.questionSets = {};
        }
        for (const questionSet of this.rawConfig.form.questionSets) {
            this.config.questionSets[questionSet.key] = this.generateFieldGroups(questionSet.fields);
            this.config.questionMetadata.questionSets[questionSet.key] = {};
            for (const field of questionSet.fields) {
                this.config.questionMetadata.questionSets[questionSet.key][field.key] =
                    this.createMetadataForField(field);
                if (FieldConfigurationHelper.isToggleField(field) && !StringHelper.isNullOrEmpty(field.icon)) {
                    this.config.icons.push(field.icon);
                } else if (FieldConfigurationHelper.isLineInputField(field)) {
                    if (field.iconLeft) {
                        this.config.icons.push(field.iconLeft);
                    }
                    if (field.iconRight) {
                        this.config.icons.push(field.iconRight);
                    }
                }
            }
        }
    }

    private processRepeatingQuestionSets(): void {
        if (!this.config.repeatingQuestionSets) {
            this.config.repeatingQuestionSets = {};
        }
        if (this.rawConfig.form && this.rawConfig.form.repeatingQuestionSets) {
            for (const questionSet of this.rawConfig.form.repeatingQuestionSets) {
                this.config.repeatingQuestionSets[questionSet.key] = this.generateFieldGroups(questionSet.fields);
                this.config.questionMetadata.repeatingQuestionSets[questionSet.key] = {};
                for (const field of questionSet.fields) {
                    this.config.questionMetadata.repeatingQuestionSets[questionSet.key][field.key] =
                        this.createMetadataForField(field);
                    if (FieldConfigurationHelper.isToggleField(field) && !StringHelper.isNullOrEmpty(field.icon)) {
                        this.config.icons.push(field.icon);
                    } else if (FieldConfigurationHelper.isLineInputField(field)) {
                        if (field.iconLeft) {
                            this.config.icons.push(field.iconLeft);
                        }
                        if (field.iconRight) {
                            this.config.icons.push(field.iconRight);
                        }
                    }
                }
            }
        }
    }

    private generateFieldsOrderedByCalculationWorkbookRow(): void {
        let nonRepeatingFields: Array<FieldConfiguration> = this.getNonRepeatingFields();
        nonRepeatingFields.sort(FieldConfigurationHelper.compareByCalculationWorkbookRow);
        this.config.fieldsOrderedByCalculationWorkbookRow = nonRepeatingFields;
    }

    private generateRepeatingFieldsOrderedByCalculationWorkbookRow(): void {
        let repeatingFields: Array<FieldConfiguration> = this.getRepeatingFields();
        repeatingFields.sort(FieldConfigurationHelper.compareByCalculationWorkbookRow);
        this.config.repeatingFieldsOrderedByCalculationWorkbookRow = repeatingFields;
    }

    private createMetadataForField(field: FieldConfiguration): QuestionMetadata {
        let questionMetadata: QuestionMetadata = {
            dataType: field.dataType,
            displayable: FieldConfigurationHelper.isDataStoringField(field)
                ? (field.displayable !== undefined ? field.displayable : true)
                : false,
            canChangeWhenApproved: FieldConfigurationHelper.isDataStoringField(field)
                ? field.canChangeWhenApproved
                : null,
            private: FieldConfigurationHelper.isDataStoringField(field) ? field.private : null,
            resetForNewQuotes: FieldConfigurationHelper.isDataStoringField(field)
                ? field.resetForNewQuotes
                : null,
            resetForNewRenewalQuotes: FieldConfigurationHelper.isDataStoringField(field)
                ? field.resetForNewRenewalQuotes
                : null,
            resetForNewAdjustmentQuotes: FieldConfigurationHelper.isDataStoringField(field)
                ? field.resetForNewAdjustmentQuotes
                : null,
            resetForNewCancellationQuotes: FieldConfigurationHelper.isDataStoringField(field)
                ? field.resetForNewCancellationQuotes
                : null,
            resetForNewPurchaseQuotes: FieldConfigurationHelper.isDataStoringField(field)
                ? field.resetForNewPurchaseQuotes
                : null,
            tags: field.tags,
            currencyCode: field.currencyCode,
            name: field.name,
        };

        if (FieldConfigurationHelper.isVisibleField(field)) {
            questionMetadata.summaryLabel = field.summaryLabel;
            questionMetadata.summaryPositionExpression = field.summaryPositionExpression;
        }

        return questionMetadata;
    }

    private getRepeatingFields(): Array<FieldConfiguration> {
        let fields: Array<FieldConfiguration> = new Array<FieldConfiguration>();
        if (this.rawConfig.form && this.rawConfig.form.repeatingQuestionSets) {
            for (let questionSet of this.rawConfig.form.repeatingQuestionSets) {
                fields.push(...questionSet.fields);
            }
        }
        return fields;
    }

    private getNonRepeatingFields(): Array<FieldConfiguration> {
        let fields: Array<FieldConfiguration> = new Array<FieldConfiguration>();
        if (this.rawConfig.form && this.rawConfig.form.questionSets) {
            for (let questionSet of this.rawConfig.form.questionSets) {
                fields.push(...questionSet.fields);
            }
        }
        return fields;
    }

    private generateSettings(): void {
        let theme: ThemeConfiguration = this.rawConfig.form.theme;
        this.config.theme = theme;
        this.config.settings = <any>{
            financial: <any>{},
            styling: <any>{
                other: <any>{},
            },
            theme: theme,
        };

        this.config.settings.financial.defaultCurrency = this.rawConfig.form.defaultCurrencyCode
            ? this.rawConfig.form.defaultCurrencyCode
            : 'AUD';
        this.config.settings.paymentForm = this.rawConfig.paymentFormConfiguration;
    }

    private processTextElements(): void {
        this.config.textElements = _.cloneDeep(<any>defaultTextElementsJson.textElements);
        if (this.rawConfig.form?.textElements) {
            let hasSidebarPurchaseCategory: boolean = false;
            for (let textElement of this.rawConfig.form.textElements) {
                if (!textElement.category) {
                    throw Errors.Product.Configuration("When parsing the configuration, we came across a text element "
                        + `"{textElement.name}" which did not have a category.`);
                } else if (!textElement.name) {
                    throw Errors.Product.Configuration("When parsing the configuration, we came across a text element "
                        + `"in category "{textElement.category}" `
                        + (textElement.subcategory
                            ? `"{textElement.subcategory} "`
                            : '') + 'which did not have a name.');
                }

                let categoryCamelCase: string = StringHelper.toCamelCase(textElement.category);
                if (!hasSidebarPurchaseCategory && categoryCamelCase == 'sidebarPurchase') {
                    hasSidebarPurchaseCategory = true;
                }
                let nameCamelCase: string = StringHelper.toCamelCase(textElement.name);
                let icon: string;
                if (textElement.icon) {
                    if (textElement.icon.startsWith('fa-')) {
                        icon = `fa ${textElement.icon}`;
                    } else if (textElement.icon.startsWith('glyphicon-')) {
                        icon = `glyphicon ${textElement.icon}`;
                    } else {
                        icon = textElement.icon;
                    }

                    // add it to the list of icons for preloading
                    this.config.icons.push(icon);
                }
                if (!this.config.textElements[categoryCamelCase]) {
                    this.config.textElements[categoryCamelCase] = {};
                }
                if (!textElement.subcategory) {
                    this.config.textElements[categoryCamelCase][nameCamelCase] = textElement;
                } else {
                    let subcategoryCamelCase: string = StringHelper.toCamelCase(textElement.subcategory);
                    if (!hasSidebarPurchaseCategory && subcategoryCamelCase == 'sidebarPurchase') {
                        hasSidebarPurchaseCategory = true;
                    }
                    if (!this.config.textElements[categoryCamelCase][subcategoryCamelCase]) {
                        this.config.textElements[categoryCamelCase][subcategoryCamelCase] = {};
                    }
                    this.config.textElements[categoryCamelCase][subcategoryCamelCase][nameCamelCase] = {
                        text: textElement.text,
                        icon: icon,
                    };
                }
            }

            // if it's an old product configuration and it only defines "sidebar" and not the other text elements, 
            // then let's honour that and use the "sidebar" elements for "sidebarPurchase"
            if (!hasSidebarPurchaseCategory) {
                this.config.textElements['sidebarPurchase'] = _.merge(
                    {},
                    this.config.textElements['sidebarPurchase'],
                    this.config.textElements['sidebar']);
            }
        }
    }

    private generateTriggers(): void {
        this.config.triggers = {};
        if (this.rawConfig.triggers) {
            for (let triggerConfig of this.rawConfig.triggers) {
                if (!this.config.triggers[triggerConfig.type]) {
                    this.config.triggers[triggerConfig.type] = {};
                }
                this.config.triggers[triggerConfig.type][triggerConfig.key] = triggerConfig;
            }
        }
    }

    private generateWorkflowRoles(): void {
        this.config.workflowRoles = {};
        for (let questionSet of this.rawConfig.form.questionSets) {
            for (let field of questionSet.fields) {
                if (field.workflowRole) {
                    this.config.workflowRoles[field.workflowRole] = field.key;
                }
            }
        }
    }

    private processformDataLocator(): void {
        this.config.dataLocators = this.rawConfig.dataLocators;
    }

    private generateFieldGroups(questionSetFields: Array<FieldConfiguration>): Array<FieldGroup> {
        let fieldGroups: Array<FieldGroup> = new Array<FieldGroup>();
        let fieldGroupFields: Array<Field>;
        let firstFieldForQuestionSet: boolean = true;
        for (let fieldConfig of questionSetFields) {
            if (firstFieldForQuestionSet || FieldConfigurationHelper.isVisibleField(fieldConfig)
                && fieldConfig.startsNewRow
            ) {
                fieldGroupFields = new Array<Field>();
                let fieldGroup: FieldGroup = {
                    className: 'row',
                    fieldGroup: fieldGroupFields,
                };
                fieldGroups.push(fieldGroup);
                firstFieldForQuestionSet = false;
            }
            let field: Field = <any>{};
            this.generateFieldSettings(fieldConfig, field);
            fieldGroupFields.push(field);
        }
        return fieldGroups;
    }

    private generateFieldSettings(fieldConfig: FieldConfiguration, field: Field): void {
        field.key = fieldConfig.key;
        field.type = FieldConfigurationHelper.getFieldSelector(fieldConfig.$type);
        field.templateOptions = <any>{
            fieldConfiguration: fieldConfig,
            hideCondition: fieldConfig.hideConditionExpression,
        };
        if (FieldConfigurationHelper.isVisibleField(fieldConfig)) {
            field.className = this.generateClassName(fieldConfig);
            let validation: Array<FormControlValidatorFunction> = this.generateValidation(fieldConfig);
            if (validation != null && validation.length) {
                field.validators = <any>{};
                field.validators.validation = validation;
            }
        }
        if (FieldConfigurationHelper.isInteractiveField(fieldConfig)) {
            // If only a calculated value is set, then we should make the field read only
            // since typing would always change it.
            if (fieldConfig.$type != FieldType.Hidden) {
                if (!StringHelper.isNullOrWhitespace(fieldConfig.calculatedValueExpression)) {
                    if (StringHelper.isNullOrWhitespace(fieldConfig.calculatedValueTriggerExpression)
                        && StringHelper.isNullOrWhitespace(fieldConfig.calculatedValueConditionExpression)
                    ) {
                        fieldConfig.readOnlyConditionExpression = 'true';
                    }
                }
            }
            this.generateAddOnIcons(fieldConfig, field);
        }
        if (FieldConfigurationHelper.isTextInputField(fieldConfig)) {
            if (!(fieldConfig as SingleLineTextFieldConfiguration).textInputFormat) {
                (fieldConfig as SingleLineTextFieldConfiguration).textInputFormat
                    = FieldConfigurationHelper.getTextInputFormat(fieldConfig.dataType);
            }
        }
        if (FieldConfigurationHelper.isLineInputField(fieldConfig)) {
            if (fieldConfig.iconLeft != null || fieldConfig.textLeft != null) {
                field.templateOptions.addonLeft = {
                    class: fieldConfig.iconLeft,
                    text: fieldConfig.textLeft,
                };
            }
            if (fieldConfig.iconRight != null || fieldConfig.textRight != null) {
                field.templateOptions.addonRight = {
                    class: fieldConfig.iconRight,
                    text: fieldConfig.textRight,
                };
            }
            if (fieldConfig.$type == FieldType.SearchSelect) {
                if (!field.templateOptions.addonLeft) {
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-search',
                    };
                }
            }
        }
    }

    private generateClassName(fieldConfig: VisibleFieldConfiguration): string {
        if (fieldConfig.bootstrapColumnsExtraSmall == -1
            || fieldConfig.bootstrapColumnsSmall == -1
            || fieldConfig.bootstrapColumnsMedium == -1
            || fieldConfig.bootstrapColumnsLarge == -1
        ) {
            // TODO: Go through all workbooks and set the defaults so it's 
            // automatically "display: none" and you don't have to enter -1
            return "no-layout";
        }

        let className: string = '';
        className += fieldConfig.bootstrapColumnsExtraSmall !== undefined
            ? `col-xs-${fieldConfig.bootstrapColumnsExtraSmall} ` : '';
        className += fieldConfig.bootstrapColumnsSmall !== undefined
            ? `col-sm-${fieldConfig.bootstrapColumnsSmall} ` : '';
        className += fieldConfig.bootstrapColumnsMedium !== undefined
            ? `col-md-${fieldConfig.bootstrapColumnsMedium} ` : '';
        className += fieldConfig.bootstrapColumnsLarge !== undefined
            ? `col-lg-${fieldConfig.bootstrapColumnsLarge} ` : '';
        if (className == '') {
            if (fieldConfig.$type == FieldType.Buttons
                || fieldConfig.$type == FieldType.TextArea
                || fieldConfig.$type == FieldType.Repeating
            ) {
                className = 'col-xs-12';
            } else {
                className = 'col-xs-12 col-md-6';
            }
        }
        return className;
    }

    private generateValidation(fieldConfig: VisibleFieldConfiguration): Array<FormControlValidatorFunction> {
        let validation: Array<any> = new Array<any>();
        let validators: Array<string> = new Array<string>();
        if (FieldConfigurationHelper.isInteractiveField(fieldConfig)) {
            if (fieldConfig.required) {
                validators.push(`required(true)`);
            }
        }
        switch (fieldConfig.dataType) {
            case FieldDataType.Number:
                validators.push('isNumber()');
                break;
            case FieldDataType.Email:
                validators.push('isEmail()');
                break;
            case FieldDataType.Phone:
                validators.push('isPhoneNumber()');
                break;
            case FieldDataType.Name:
                validators.push('isName()');
                break;
            case FieldDataType.Currency:
                validators.push('isCurrency()');
                break;
            case FieldDataType.Percent:
                validators.push('isPercent()');
                break;
            case FieldDataType.Date:
                validators.push('isDate()');
                break;
            case FieldDataType.Postcode:
                validators.push('isPostcode()');
                break;
            case FieldDataType.Url:
                validators.push('isURL()');
                break;
            case FieldDataType.Abn:
                validators.push('isABN()');
                break;
            case FieldDataType.Acn:
                validators.push('isACN()');
                break;
            case FieldDataType.NumberPlate:
                validators.push('isNumberPlate()');
                break;
            default:
                if (!Object.values(FieldDataType).includes(fieldConfig.dataType)) {
                    throw Errors.Product.Configuration('When trying to process the product component configuration, we '
                        + `came across a field with a data type "${fieldConfig.dataType}" which is unknown.`);
                }
        }
        switch (fieldConfig.$type) {
            case FieldType.Currency:
                if (!validators.includes('isCurrency()')) {
                    validators.push('isCurrency()');
                }
        }
        if (FieldConfigurationHelper.isVisibleField(fieldConfig)) {
            if (!StringHelper.isNullOrWhitespace(fieldConfig.validationRules)) {
                let pattern: RegExp = /\)\s*;\s*/g;
                let validationRules: string = fieldConfig.validationRules.replace(pattern, ')%SPLIT_TOKEN%');
                let splitRules: Array<string> = validationRules.split('%SPLIT_TOKEN%');
                validators.push(...splitRules);
            }
        }
        if (!validators.length) {
            return null;
        }

        for (let validator of validators) {
            let validationSource: string = 'ValidationService.' + validator;
            let validatorFunction: any = this.evaluateService.evaluate(
                this.addEmptyFunctionParentheses(validationSource),
                // eslint-disable-next-line @typescript-eslint/naming-convention
                { "Validators": Validators, "ValidationService": this.validationService });
            validation.push(validatorFunction);
        }

        return validation;
    }

    private addEmptyFunctionParentheses(validationConfig: string): string {
        return validationConfig
            // eslint-disable-next-line no-useless-escape
            ? validationConfig.replace(/ValidationService.([a-zA-Z]+)([^a-zA-Z\(])/g, 'ValidationService.$1()$2')
            : validationConfig;
    }

    private generateAddOnIcons(fieldConfig: InteractiveFieldConfiguration, field: Field): void {
        if (FieldConfigurationHelper.isTextInputField(fieldConfig)
            || fieldConfig.$type == FieldType.DropDownSelect
        ) {
            switch (fieldConfig.dataType) {
                case FieldDataType.Abn:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-certificate',
                    };
                    break;
                case FieldDataType.Acn:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-certificate',
                    };
                    break;
                case FieldDataType.Currency:
                    // We will set the currency icon in AddonWrapper
                    field.templateOptions.addonLeft = {};
                    break;
                case FieldDataType.Date:
                    field.templateOptions.addonLeft = {
                        class: 'glyphicon glyphicon-calendar',
                    };
                    break;
                case FieldDataType.Email:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-envelope',
                    };
                    break;
                case FieldDataType.Name:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-user',
                    };
                    break;
                case FieldDataType.NumberPlate:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-rectangle-wide',
                    };
                    break;
                case FieldDataType.Password:
                    field.templateOptions.addonRight = {
                        class: 'fa fa-key',
                    };
                    break;
                case FieldDataType.Percent:
                    field.templateOptions.addonRight = {
                        class: 'fa fa-percent',
                    };
                    break;
                case FieldDataType.Phone:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-phone',
                    };
                    break;
                case FieldDataType.Postcode:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-map-marker',
                    };
                    break;
                case FieldDataType.Time:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-clock',
                    };
                    break;
                case FieldDataType.Url:
                    field.templateOptions.addonLeft = {
                        class: 'fa fa-globe',
                    };
                    break;
            }
        }
    }

    private generateStyles(): void {
        let css: string = '';
        if (this.rawConfig.form.theme && this.rawConfig.form.theme.styles) {
            for (let s of this.rawConfig.form.theme.styles) {
                let properties: string = ''
                    + (!StringHelper.isNullOrWhitespace(s.fontFamily) ? `font-family:'${s.fontFamily}'; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.fontSize) ? `font-size:${s.fontSize}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.fontWeight) ? `font-weight:${s.fontWeight}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.colour) ? `color:${s.colour}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.background) ? `background:${s.background}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.border) ? `border:${s.border}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.borderRadius) ? `border-radius:${s.borderRadius}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.marginTop) ? `margin-top:${s.marginTop}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.marginRight) ? `margin-right:${s.marginRight}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.marginBottom) ? `margin-bottom:${s.marginBottom}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.marginLeft) ? `margin-left:${s.marginLeft}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.paddingTop) ? `padding-top:${s.paddingTop}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.paddingRight) ? `padding-right:${s.paddingRight}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.paddingBottom) ? `padding-bottom:${s.paddingBottom}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.paddingLeft) ? `padding-left:${s.paddingLeft}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.borderTop) ? `border-top:${s.borderTop}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.borderRight) ? `border-right:${s.borderRight}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.borderBottom) ? `border-bottom:${s.borderBottom}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.borderLeft) ? `border-left:${s.borderLeft}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.width) ? `width:${s.width}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.height) ? `height:${s.height}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.top) ? `top:${s.top}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.right) ? `right:${s.right}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.bottom) ? `bottom:${s.bottom}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.left) ? `left:${s.left}; ` : '')
                    + (!StringHelper.isNullOrWhitespace(s.customCss) ? `${s.customCss} ` : '');
                if (properties != '') {
                    let cssLine: string = `${s.selector} { ${properties}}`;
                    if (!StringHelper.isNullOrWhitespace(s.wrapper)) {
                        cssLine = `${s.wrapper} { ${cssLine} }`;
                    }
                    css += `${cssLine}\n`;
                }
            }
        }
        this.config.styles = {
            css: css,
        };
    }

    /**
     * Grabs the icons from any options and prepares them into a list for preloading
     */
    private processOptions(): void {
        this.config.optionSets = new Map<string, Array<OptionConfiguration>>();
        if (this.rawConfig.form && this.rawConfig.form.optionSets) {
            for (const optionSet of this.rawConfig.form.optionSets) {
                this.config.optionSets.set(optionSet.key, optionSet.options);
                for (const option of optionSet.options) {
                    if (!StringHelper.isNullOrEmpty(option.icon)) {
                        this.config.icons.push(option.icon);
                    }
                }
            }
        }
    }

    private processContextEntities(): void {
        if (this.rawConfig.contextEntities) {
            this.config.contextEntities = this.rawConfig.contextEntities;
        }
    }
}
