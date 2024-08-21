import { AdditionalPropertyDefinition } from '@app/models/additional-property-item-view.model';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailsListItemGroupType } from '@app/models/details-list/details-list-item-type.enum';
import { DetailListItemHelper } from '../helpers/detail-list-item.helper';
import { EntityType } from '../models/entity-type.enum';
import { AdditionalPropertyDefinitionResourceModel }
    from '../resource-models/additional-property-definition.resource-model';
import { titleCase } from 'title-case';
import * as ChangeCase from 'change-case';
import { AdditionalPropertyDefinitionContextType } from '../models/additional-property-context-type.enum';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { AdditionalPropertyDefinitionTypeEnum } from '../models/additional-property-definition-types.enum';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DetailsListFormSelectItem } from '@app/models/details-list/details-list-form-select-item';
import { DetailsListFormCheckboxGroupItem } from '@app/models/details-list/details-list-form-checkbox-group-item';
import { DetailsListFormTextAreaItem } from '@app/models/details-list/details-list-form-text-area-item';
import { AdditionalPropertyDefinitionSchemaTypeEnum } from '@app/models/additional-property-schema-type.enum';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { OtherSetting } from './additional-property-definition-other-setting.viewmodel';
import { ValidationMessages } from '@app/models/validation-messages';

/**
 * View model for additional properties.
 */
export class AdditionalPropertyDefinitionViewModel {
    public alias: string;
    public defaultValue: string;
    public name: string;
    public otherSettings: Array<boolean> = [];
    public type: string;
    public id: string;
    public schemaType?: string;
    public customSchema?: string;
    public defaultValueTextArea: string;
    public typeDisplayName: string;

    public static createDetailListForCreate(): Array<DetailsListFormItem> {
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            "Details");
        let details: Array<DetailsListFormItem> = [];
        let itemsToSet: Array<string> = ["defaultValue", "schemaType", "customSchema", "defaultValueTextArea"];
        details = this.buildInitialDetailList(detailsCard);
        for (let itemToSet of itemsToSet) {
            let foundIndex: number = details.findIndex((item: DetailsListFormItem) => item.Alias === itemToSet);
            if (foundIndex > -1) {
                details[foundIndex].Visible = false;
            }
        }
        return details;
    }

    public static createDetailListForEdit(
        additionalProperty: AdditionalPropertyDefinition,
    ): Array<DetailsListFormItem> {
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            "Details");
        let details: Array<DetailsListFormItem> = [];
        details = this.buildInitialDetailList(detailsCard);

        const defaultValueKey: string = "defaultValue";
        let defaultDetailItem: DetailsListFormItem = details.filter(
            (detail: DetailsListFormItem) => detail.Alias === defaultValueKey)[0];
        defaultDetailItem.Visible = additionalProperty.setToDefaultValue
            && additionalProperty.type == AdditionalPropertyDefinitionTypeEnum.Text;

        const schemaTypeKey: string = "schemaType";
        let schemaTypeDetailItem: DetailsListFormItem = details.filter(
            (detail: DetailsListFormItem) => detail.Alias === schemaTypeKey)[0];
        schemaTypeDetailItem.Visible = additionalProperty.type == AdditionalPropertyDefinitionTypeEnum.StructuredData;

        const customSchemaKey: string = "customSchema";
        let customSchemaDetailItem: DetailsListFormItem = details.filter(
            (detail: DetailsListFormItem) => detail.Alias === customSchemaKey)[0];
        customSchemaDetailItem.Visible = additionalProperty.type == AdditionalPropertyDefinitionTypeEnum.StructuredData
            && additionalProperty.schemaType == AdditionalPropertyDefinitionSchemaTypeEnum.Custom;

        const defaulValueTextAreaKey: string = "defaultValueTextArea";
        let defaultValueTextAreaDetailItem: DetailsListFormItem = details.filter(
            (detail: DetailsListFormItem) => detail.Alias === defaulValueTextAreaKey)[0];
        defaultValueTextAreaDetailItem.Visible = additionalProperty.setToDefaultValue
            && additionalProperty.type == AdditionalPropertyDefinitionTypeEnum.StructuredData;

        return details;
    }

    private static buildInitialDetailList(
        card: DetailsListItemCard,
    ): Array<DetailsListFormItem> {
        const icons: any = DetailListItemHelper.detailListItemIconMap;
        let validator: typeof FormValidatorHelper = FormValidatorHelper;
        const items: Array<DetailsListFormItem> = [];
        items.push(DetailsListFormTextItem.create(
            card,
            'name',
            'Property Name')
            .withIcon(icons.brush, IconLibrary.IonicV4)
            .withValidator(validator.alphaNumericValidator(
                true,
                ValidationMessages.errorKey.Name)));
        items.push(DetailsListFormTextItem.create(
            card,
            'alias',
            'Alias')
            .withValidator(validator.aliasValidator(true)));
        items.push(DetailsListFormSelectItem.create(
            card,
            'type',
            'Type'));
        items.push(DetailsListFormSelectItem.create(
            card,
            'schemaType',
            'JSON Schema'));
        items.push(DetailsListFormTextAreaItem.create(
            card,
            'customSchema',
            'Custom JSON Schema'));
        items.push(DetailsListFormCheckboxGroupItem.create(
            card,
            'otherSettings',
            'Other Settings'));
        items.push(DetailsListFormTextItem.create(
            card,
            'defaultValue',
            'Default Value'));
        items.push(DetailsListFormTextAreaItem.create(
            card,
            'defaultValueTextArea',
            'Default Value'));
        return items;
    }

    public static createResourceModel(
        tenant: string,
        context: AdditionalPropertyDefinitionContextType,
        entityType: EntityType,
        parentContextId: string,
        contextId: string,
        formModel: AdditionalPropertyDefinitionViewModel,
    ): AdditionalPropertyDefinitionResourceModel {
        let otherSettingOptions: Array<OtherSetting> = this.generateOtherSettingOptions();
        let defaultValue: string
            = AdditionalPropertyDefinitionTypeEnum[formModel.type]
                == AdditionalPropertyDefinitionTypeEnum.StructuredData
                ? formModel.defaultValueTextArea
                : formModel.defaultValue;
        let schemaTypeValue: string
            = AdditionalPropertyDefinitionTypeEnum[formModel.type]
                == AdditionalPropertyDefinitionTypeEnum.StructuredData
                ? formModel.schemaType
                : null;
        let model: AdditionalPropertyDefinitionResourceModel = {
            tenant: tenant,
            alias: formModel.alias,
            name: formModel.name,
            type: AdditionalPropertyDefinitionTypeEnum[formModel.type],
            isRequired: true,
            setToDefaultValue: true,
            isUnique: true,
            defaultValue: defaultValue,
            entityType: entityType,
            parentContextId: parentContextId,
            id: "00000000-0000-0000-0000-000000000000",
            createdDateTime: null,
            contextId: contextId,
            contextType: context,
            schemaType: schemaTypeValue,
            customSchema: formModel.customSchema,
        };
        for (let i: number = 0; i < formModel.otherSettings.length; i++) {
            switch (otherSettingOptions[i].id) {
                case "required":
                    model.isRequired = formModel.otherSettings[i];
                    break;
                case "unique":
                    model.isUnique = formModel.otherSettings[i];
                    break;
                case "setToDefaultValue":
                    model.setToDefaultValue = formModel.otherSettings[i];
                    break;
            }
        }
        if (!model.setToDefaultValue) {
            model.defaultValue = "";
        }
        if (model.schemaType != "Custom") {
            model.customSchema = "";
        }
        return model;
    }

    public constructor(private additionalProperty: AdditionalPropertyDefinition) {
        this.alias = additionalProperty.alias;
        this.defaultValue = additionalProperty.defaultValue;
        this.defaultValueTextArea = additionalProperty.defaultValue;
        this.id = additionalProperty.id;
        this.name = additionalProperty.name;
        this.type = additionalProperty.type;
        this.otherSettings = this.generateSettingOptionsByAdditionalProperty(additionalProperty);
        this.schemaType = additionalProperty.schemaType;
        this.customSchema = additionalProperty.customSchema;
    }

    public static createDetailListItem(
        additionalProperty: AdditionalPropertyDefinition,
        schema?: string): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "Context",
                additionalProperty.contextName + ' ' + additionalProperty.contextType),
            DetailsListGroupItemModel.create(
                "Entity Type",
                titleCase(ChangeCase.sentenceCase(additionalProperty.entityType))),
            DetailsListGroupItemModel.create(
                "Property Name",
                additionalProperty.name),
            DetailsListGroupItemModel.create(
                "Property Alias",
                additionalProperty.alias),
            DetailsListGroupItemModel.create(
                "Property Type",
                titleCase(this.formatEnumValue(additionalProperty.type.toString()))),
            DetailsListGroupItemModel.create(
                "Required",
                additionalProperty.isRequired ? 'Yes' : 'No'),
            DetailsListGroupItemModel.create(
                "Unique",
                additionalProperty.isUnique ? 'Yes' : 'No'),
            DetailsListGroupItemModel.create(
                "Default Value",
                additionalProperty.defaultValue),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.brush,
        ));

        if (additionalProperty.schemaType != 'None') {
            const jsonSchemaCard: DetailsListItemCard =
                new DetailsListItemCard(DetailsListItemCardType.JsonSchema, 'Schema');

            details.push(
                DetailsListItem.createItem(
                    jsonSchemaCard,
                    DetailsListItemGroupType.JsonSchema,
                    schema,
                    'JSON Schema',
                    'code'));
        }
        return details;
    }

    public static generateOtherSettingOptions(defaultValueToggleLabel: string = ""): Array<OtherSetting> {
        let otherSettings: Array<OtherSetting> = [];
        otherSettings.push({ defaultValue: false, description: "Required property", id: "required" });
        otherSettings.push({ defaultValue: false, description: "Value must be unique", id: "unique" });
        otherSettings.push({
            defaultValue: false,
            description: defaultValueToggleLabel,
            id: "setToDefaultValue",
        });
        return otherSettings;
    }

    public generateOtherSettingOptionsForEdit(defaultValueToogleLabel: string = ""): Array<OtherSetting> {
        let defaultOtherSettings: Array<OtherSetting> =
            AdditionalPropertyDefinitionViewModel.generateOtherSettingOptions(defaultValueToogleLabel);
        for (let i: number = 0; i < this.otherSettings.length; i++) {
            defaultOtherSettings[i].defaultValue = this.otherSettings[i];
        }
        return defaultOtherSettings;
    }

    public generateSettingOptionsByAdditionalProperty(
        additionalProperty: AdditionalPropertyDefinition,
    ): Array<boolean> {
        let retValue: Array<boolean> = [];
        retValue.push(additionalProperty.isRequired);
        retValue.push(additionalProperty.isUnique);
        retValue.push(additionalProperty.setToDefaultValue);
        return retValue;
    }

    private static formatEnumValue(inputString: string): string {
        return inputString.replace(/([a-z])([A-Z])/g, '$1 $2');
    }
}
