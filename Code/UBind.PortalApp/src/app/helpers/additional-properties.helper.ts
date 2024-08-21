import { additionalPropertyCategories } from './additional-property-categories';
import {
    AdditionalPropertyDefinition,
    AdditionalPropertyValue,
} from '@app/models/additional-property-item-view.model';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListItemGroupType } from '@app/models/details-list/details-list-item-type.enum';
import { Permission } from '.';
import { DetailListItemHelper } from './detail-list-item.helper';
import { AdditionalPropertyValueUpsertResourceModel } from '../resource-models/additional-property.resource-model';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { FormValidatorHelper } from './form-validator.helper';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { IconLibrary } from '@app/models/icon-library.enum';
import { AdditionalPropertyDefinitionTypeEnum } from '@app/models/additional-property-definition-types.enum';
import { DetailsListFormTextAreaItem } from '@app/models/details-list/details-list-form-text-area-item';
import {
    AdditionalPropertyDefinitionContextSettingItemViewModel,
} from '@app/viewmodels/additional-property-definition-context-setting-item.viewmodel';

/**
 * Helper for additional properties.
 */
export class AdditionalPropertiesHelper {

    public static additionalPropertyPrefixId: string = "properties_";
    public static defaultGuid: string = '00000000-0000-0000-0000-000000000000';

    public static generateAdditionalPropertyInputFieldsForEdit(
        additionalPropertyInputFields: Array<AdditionalPropertyValue>,
        additionalPropertyValues: Array<AdditionalPropertyValue>,
    ): Array<AdditionalPropertyValue> {

        const callback = (apv: AdditionalPropertyValue): AdditionalPropertyValue => {
            if (!additionalPropertyValues) {
                return apv;
            }

            const index: number = additionalPropertyValues.findIndex((ap: AdditionalPropertyValue) => {
                return ap.additionalPropertyDefinitionModel.id === apv.additionalPropertyDefinitionModel.id;
            });

            return index > -1 ? additionalPropertyValues[index] : apv;
        };

        return additionalPropertyInputFields.map<AdditionalPropertyValue>(callback);
    }

    public static generateAdditionalPropertyInputFieldsForCreateForm(
        productAdditionalProperties: Array<AdditionalPropertyDefinition>,
    ): Array<AdditionalPropertyValue> {

        const callback = (ap: AdditionalPropertyDefinition): AdditionalPropertyValue => {
            return {
                value: ap.setToDefaultValue ? ap.defaultValue : '',
                additionalPropertyDefinitionModel: ap,
            };
        };

        return productAdditionalProperties.map<AdditionalPropertyValue>(callback).sort();
    }

    public static generateAdditionalPropertyDefinitionsByContext(
        additionalPropertyDefinitions: Array<AdditionalPropertyDefinition>,
        contextType: AdditionalPropertyDefinitionContextType,
    ): Array<AdditionalPropertyDefinitionContextSettingItemViewModel> {
        const result: Array<AdditionalPropertyDefinitionContextSettingItemViewModel> = [];
        for (const additionalPropertyCategory of additionalPropertyCategories) {
            if (additionalPropertyCategory.applicableContext.includes(contextType)) {
                const filteredAdditionalPropertyDefinitions: Array<AdditionalPropertyDefinition>
                    = additionalPropertyDefinitions.filter((apc: AdditionalPropertyDefinition) =>
                        apc.entityType === additionalPropertyCategory.entityType);
                result.push(
                    {
                        ...additionalPropertyCategory,
                        count: filteredAdditionalPropertyDefinitions !== undefined
                            ? filteredAdditionalPropertyDefinitions.length
                            : 0,
                    },
                );
            }
        }
        return result;
    }

    public static createDetailsList(
        detailListItem: Array<DetailsListItem>,
        additionalPropertyValues: Array<AdditionalPropertyValue>,
    ): void {

        if (!additionalPropertyValues) {
            return;
        }

        const additionalPropertiesCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.AdditionalProperties, 'Properties');

        additionalPropertyValues.forEach((apv: AdditionalPropertyValue) => {
            detailListItem.push(DetailsListItem
                .createItem(
                    additionalPropertiesCard,
                    DetailsListItemGroupType.Properties,
                    apv.value,
                    apv.additionalPropertyDefinitionModel.name,
                    'brush',
                )
                .withAllowedPermissions(Permission.ViewAdditionalPropertyValues));
        });
    }

    public static updateDetailList(
        detailListItem: Array<DetailsListItem>,
        additionalPropertyValues: Array<AdditionalPropertyValue>,
    ): void {

        if (!additionalPropertyValues) {
            return;
        }

        additionalPropertyValues.forEach((apv: AdditionalPropertyValue) => {
            let index: number = detailListItem.findIndex(
                (dtl: DetailsListItem) => dtl.Description === apv.additionalPropertyDefinitionModel.name,
            );

            if (index != -1) {
                detailListItem[index].DisplayValue = apv.value;
            }
        });
    }

    public static getDetailListForEdit(
        additionalPropertyValueFields: Array<AdditionalPropertyValue>,
    ): Array<DetailsListFormItem> {
        const additionalPropertiesCard: DetailsListItemCard
            = new DetailsListItemCard(DetailsListItemCardType.AdditionalProperties, 'Properties');
        let additionalPropertyDetailListFormItems: Array<DetailsListFormItem> = [];
        const validator: typeof FormValidatorHelper = FormValidatorHelper;
        const icons: any = DetailListItemHelper.detailListItemIconMap;
        if (!additionalPropertyValueFields) {
            return additionalPropertyDetailListFormItems;
        }
        additionalPropertyValueFields.forEach(
            (propertyField: AdditionalPropertyValue) => {
                let item: DetailsListFormItem;
                if (propertyField.additionalPropertyDefinitionModel.type
                    == AdditionalPropertyDefinitionTypeEnum.StructuredData
                ) {
                    item = DetailsListFormTextAreaItem.create(
                        additionalPropertiesCard,
                        AdditionalPropertiesHelper.generateControlId(
                            propertyField.additionalPropertyDefinitionModel.id),
                        propertyField.additionalPropertyDefinitionModel.name)
                        .withGroupName(DetailsListItemGroupType.Properties)
                        .withIcon(icons.brush, IconLibrary.IonicV4)
                        .withValidator(
                            propertyField.additionalPropertyDefinitionModel.isRequired ? validator.required() : []);
                } else {
                    item = DetailsListFormTextItem.create(
                        additionalPropertiesCard,
                        AdditionalPropertiesHelper.generateControlId(
                            propertyField.additionalPropertyDefinitionModel.id),
                        propertyField.additionalPropertyDefinitionModel.name)
                        .withGroupName(DetailsListItemGroupType.Properties)
                        .withIcon(icons.brush, IconLibrary.IonicV4)
                        .withValidator(
                            propertyField.additionalPropertyDefinitionModel.isRequired ? validator.required() : []);
                }

                item.customAdditionalPropertyField = true;
                item.customErrorMessageOnRequiredField =
                    `Please enter a value for ${propertyField.additionalPropertyDefinitionModel.name}`;
                additionalPropertyDetailListFormItems.push(item);
            });
        return additionalPropertyDetailListFormItems;
    }

    public static buildProperties(
        additionalPropertyValues: Array<AdditionalPropertyValue>,
        value: any,
    ): Array<AdditionalPropertyValueUpsertResourceModel> {
        let retValue: Array<AdditionalPropertyValueUpsertResourceModel> = [];
        let keys: Array<string> = Object.keys(value);
        if (additionalPropertyValues?.length > 0) {
            additionalPropertyValues.forEach(
                (additionalPropertyValue: AdditionalPropertyValue) => {
                    let controlId: string = AdditionalPropertiesHelper.generateControlId(
                        additionalPropertyValue.additionalPropertyDefinitionModel.id,
                    );
                    let item: AdditionalPropertyValueUpsertResourceModel = {
                        definitionId: additionalPropertyValue.additionalPropertyDefinitionModel.id,
                        propertyType: additionalPropertyValue.additionalPropertyDefinitionModel.type,
                        value: additionalPropertyValue.value,
                    };
                    let index: number = keys.findIndex((key: string) => key === controlId);
                    if (index > -1) {
                        let mapValue: any = value[controlId];
                        item.value = mapValue;
                    }
                    retValue.push(item);
                },
            );
        }
        return retValue;
    }

    public static updateAdditionalPropertyValues(
        defaultAdditionalPropertyValues: Array<AdditionalPropertyValue>,
        value: any,
    ): Array<AdditionalPropertyValue> {
        let keys: Array<string> = Object.keys(value);
        if (defaultAdditionalPropertyValues && defaultAdditionalPropertyValues.length > 0) {
            for (let i: number; i < defaultAdditionalPropertyValues.length; i++) {
                let controlId: string =
                    this.additionalPropertyPrefixId +
                    defaultAdditionalPropertyValues[i].additionalPropertyDefinitionModel.id;
                let index: number = keys.findIndex((key: string) => key === controlId);
                if (index > -1) {
                    let mapValue: any = value[controlId];
                    defaultAdditionalPropertyValues[i].value = mapValue;
                }
            }
        }
        return defaultAdditionalPropertyValues;
    }

    public static setFormValue(
        formValue: any,
        defaultAdditionalPropertyValues: Array<AdditionalPropertyValue>,
        setAdditionalPropertyValues: Array<AdditionalPropertyValue>,
    ): any {
        const properties: any = {};
        if (setAdditionalPropertyValues) {
            setAdditionalPropertyValues.forEach(
                (p: AdditionalPropertyValue) => properties[p.additionalPropertyDefinitionModel.id] = p.value,
            );
        }
        let keys: Array<string> = Object.keys(properties);
        defaultAdditionalPropertyValues.forEach((defaultAdditionalPropertyValue: AdditionalPropertyValue) => {
            let index: number = keys.findIndex(
                (key: string) => key === defaultAdditionalPropertyValue.additionalPropertyDefinitionModel.id,
            );
            let id: string = AdditionalPropertiesHelper.generateControlId(
                defaultAdditionalPropertyValue.additionalPropertyDefinitionModel.id,
            );
            if (index > -1) {
                formValue[id] = properties[defaultAdditionalPropertyValue.additionalPropertyDefinitionModel.id];
            } else {
                formValue[id] = defaultAdditionalPropertyValue.value;
            }
        });
        return formValue;
    }

    /**
     * Replacing the "-" in the id with "_" and appending a prefix 'properties_'
     * @param id The additional property definition id in guid form
     */
    public static generateControlId(id: string): string {
        return `${this.additionalPropertyPrefixId}${id}`;
    }
}
