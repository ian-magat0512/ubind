import { AdditionalPropertyDefinitionContextType } from "../models/additional-property-context-type.enum";
import { Entity } from "../models/entity";
import { EntityType } from '../models/entity-type.enum';
/**
 * Resource Model model of additional property definition.
 */
export interface AdditionalPropertyDefinitionResourceModel extends Entity {
    tenant: string;
    name: string;
    type: string;
    defaultValue: string;
    isRequired: boolean;
    isUnique: boolean;
    setToDefaultValue: boolean;
    alias: string;
    entityType: EntityType;
    parentContextId?: string;
    contextType: AdditionalPropertyDefinitionContextType;
    contextId?: string;
    schemaType?: string;
    customSchema?: string;
}
