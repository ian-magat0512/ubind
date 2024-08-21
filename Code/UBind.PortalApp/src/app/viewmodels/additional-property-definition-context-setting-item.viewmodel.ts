import { EntityType } from "@app/models/entity-type.enum";

/**
 * Context setting model usually being displayed for additional property definition.
 */
export interface AdditionalPropertyDefinitionContextSettingItemViewModel {
    entityType: EntityType;
    icon: string;
    isRoundIcon: boolean;
    count: number;
}
export const instanceOfAdditionalPropertyContextSettingItemModel = (
    data: any,
): data is AdditionalPropertyDefinitionContextSettingItemViewModel => {
    return 'entityType' in data;
};
