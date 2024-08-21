import { EntityType } from './entity-type.enum';
import { AdditionalPropertyDefinitionContextType } from './additional-property-context-type.enum';
import { AdditionalPropertyDefinitionTypeEnum } from './additional-property-definition-types.enum';
import { AdditionalPropertyDefinitionSchemaTypeEnum } from './additional-property-schema-type.enum';

/**
 * Model of additional property definition being utilized by pages.
 */
export interface AdditionalPropertyDefinition {
    id: string;
    contextType: AdditionalPropertyDefinitionContextType;
    contextId: string;
    entityType: EntityType;
    name: string;
    type: AdditionalPropertyDefinitionTypeEnum;
    defaultValue: string;
    isRequired: boolean;
    isUnique: boolean;
    setToDefaultValue: boolean;
    alias: string;
    parentContextId: string;
    contextName: string;
    schemaType: AdditionalPropertyDefinitionSchemaTypeEnum;
    customSchema: string;
}
/**
 * Model that contains the mapping of the additional property definition and its value.
 */
export interface AdditionalPropertyValue {
    id?: string;
    value: string;
    entityId?: string;
    additionalPropertyDefinitionModel: AdditionalPropertyDefinition;
}
