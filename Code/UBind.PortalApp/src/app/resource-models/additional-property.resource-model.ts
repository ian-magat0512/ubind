import { AdditionalPropertyDefinitionTypeEnum } from "../models/additional-property-definition-types.enum";
import { DeploymentEnvironment } from "../models/deployment-environment.enum";
import { Entity } from "../models/entity";
import { EntityType } from '../models/entity-type.enum';

/**
 * Resource Model for additional property definition
 */
export interface AdditionalPropertyResourceModel extends Entity {
    name: string;
    type: string;
    defaultvalue: string;
    required: boolean;
    unique: boolean;
    settodefaultvalue: boolean;
    alias: string;
    entitytype: EntityType;
    parentcontextid: string;
}

/**
 * Resource Model for additional property value
 */
export interface AdditionalPropertyValueUpsertResourceModel {
    value: string;
    definitionId: string;
    propertyType: AdditionalPropertyDefinitionTypeEnum;
}

/**
 * Resource Model for update additional property value
 */
export interface UpdateAdditionalPropertyValuesResourceModel {
    entityType: EntityType;
    properties: Array<AdditionalPropertyValueUpsertResourceModel>;
    environment: DeploymentEnvironment;
}
