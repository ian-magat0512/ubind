import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup, ValidationErrors } from '@angular/forms';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { AdditionalPropertyDefinition, AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { debounceTime, filter } from 'rxjs/operators';
import { AdditionalPropertyValueApiService } from './api/additional-property-value-api.service';
import { AdditionalPropertyDefinitionTypeEnum } from '@app/models/additional-property-definition-types.enum';
import { JsonValidator } from '@app/helpers/json-validator';
import { AdditionalPropertyDefinitionSchemaTypeEnum } from '@app/models/additional-property-schema-type.enum';
import { AdditionalPropertyDefinitionApiService } from './api/additional-property-definition-api.service';
import { Errors } from '@app/models/errors';
import { switchMap } from 'rxjs/internal/operators/switchMap';

/**
 * Service for additional property value.
 */
@Injectable({ providedIn: 'root' })
export class AdditionalPropertyValueService {
    private optionListSchema: string;

    public constructor(
        private additionalPropertyValueApiService: AdditionalPropertyValueApiService,
        private additionalPropertyDefinitionApiService: AdditionalPropertyDefinitionApiService,
    ) { }

    public registerUniqueAdditionalPropertyFieldsOnValueChanges(
        additionalPropertyValues: Array<AdditionalPropertyValue>,
        form: FormGroup,
        entityId: string,
        tenantId: string,
        detailList: Array<DetailsListFormItem>,
    ): void {
        if (!additionalPropertyValues || additionalPropertyValues.length === 0) {
            return;
        }

        additionalPropertyValues.forEach((apv: AdditionalPropertyValue) => {
            if (apv.additionalPropertyDefinitionModel.isUnique) {
                const controlId: string = AdditionalPropertiesHelper.generateControlId(
                    apv.additionalPropertyDefinitionModel.id,
                );
                const control: AbstractControl = form.get(controlId);
                if (control) {
                    control.valueChanges.pipe(
                        debounceTime(500),
                        filter((value: any) => !!value),
                        switchMap((value: any) =>
                            this.additionalPropertyValueApiService.isUnique(
                                apv.additionalPropertyDefinitionModel.id,
                                value,
                                apv.additionalPropertyDefinitionModel.type,
                                tenantId,
                                apv.additionalPropertyDefinitionModel.entityType,
                                entityId,
                            ),
                        ),
                    ).subscribe((isUnique: boolean) => {
                        if (!isUnique) {
                            this.handleError(control, { uniqueness: true });
                        }
                    });
                }
            }
        });
    }

    public async validateAdditionalPropertyValues(
        additionalPropertyValues: Array<AdditionalPropertyValue>,
        entityId: string,
        form: FormGroup,
        value: any,
        tenantId: string,
    ): Promise<boolean> {
        if (!additionalPropertyValues || additionalPropertyValues.length === 0) {
            return true; // No additional property values to validate, so return early.
        }

        let areAllValuesValid: boolean = true;

        for (const additionalPropertyValue of additionalPropertyValues) {
            let valueDefinitionDetails: AdditionalPropertyDefinition
                = additionalPropertyValue.additionalPropertyDefinitionModel;
            const controlId: string = AdditionalPropertiesHelper.generateControlId(
                valueDefinitionDetails.id,
            );
            const control: AbstractControl = form.get(controlId) as AbstractControl;
            const updatedValue: string = value[controlId]?.trim();

            if (valueDefinitionDetails.isRequired
                && updatedValue && updatedValue.trim() === "") {
                this.handleError(control, { required: true });
                control.markAsTouched({ onlySelf: true });
                control.setErrors({ required: true });
                areAllValuesValid = false;
            } else if (valueDefinitionDetails.isUnique
                && updatedValue && updatedValue !== "") {
                const isUnique: boolean = await this.additionalPropertyValueApiService.isUnique(
                    valueDefinitionDetails.id,
                    updatedValue,
                    valueDefinitionDetails.type,
                    tenantId,
                    valueDefinitionDetails.entityType,
                    entityId)
                    .toPromise();

                if (!isUnique) {
                    this.handleError(control, { uniqueness: true });
                    areAllValuesValid = false;
                }
            } else if (valueDefinitionDetails.type == AdditionalPropertyDefinitionTypeEnum.StructuredData
                && updatedValue && updatedValue.trim() !== "") {
                if (valueDefinitionDetails.schemaType == AdditionalPropertyDefinitionSchemaTypeEnum.None) {
                    const isValidJson: boolean = JsonValidator.isValidJson(updatedValue);
                    if (!isValidJson) {
                        form.get(controlId).markAsTouched({ onlySelf: true });
                        form.get(controlId).setErrors({ invalidJson: true });
                        areAllValuesValid = false;
                    }
                } else {
                    let schema: string;
                    switch (valueDefinitionDetails.schemaType) {
                        case AdditionalPropertyDefinitionSchemaTypeEnum.Custom:
                            schema = valueDefinitionDetails.customSchema;
                            break;
                        default:
                            schema = await this.getDefaultSchema(valueDefinitionDetails.schemaType);
                            break;
                    }
                    const result: string = JsonValidator.assertSchema(schema, updatedValue);
                    if (result != null) {
                        let errorKey: any = { invalidSchema: true };
                        if (result.includes('Invalid JSON object')) {
                            errorKey = { invalidJson: true };
                        } else if (result.includes('JSON object does not pass schema assertion')) {
                            errorKey = { jsonAssertionFailed: true };
                        }
                        form.get(controlId).markAsTouched({ onlySelf: true });
                        form.get(controlId).setErrors(errorKey);
                        areAllValuesValid = false;
                    }
                }
            }
        }

        return areAllValuesValid;
    }

    public async getDefaultSchema(schemaType: AdditionalPropertyDefinitionSchemaTypeEnum): Promise<string> {
        if (schemaType == AdditionalPropertyDefinitionSchemaTypeEnum.OptionList) {
            if (!this.optionListSchema) {
                this.optionListSchema = await this.additionalPropertyDefinitionApiService.getDefaultSchema(schemaType)
                    .toPromise();
            }
            return this.optionListSchema;
        }

        throw Errors.General.UnexpectedEnumValue(
            'AdditionalPropertyDefinitionSchemaTypeEnum',
            schemaType,
            'get the default schema to validate an additional property value');
    }

    private handleError(control: AbstractControl, errors: ValidationErrors): void {
        control.markAsTouched({ onlySelf: true });
        control.setErrors(errors);
    }
}
