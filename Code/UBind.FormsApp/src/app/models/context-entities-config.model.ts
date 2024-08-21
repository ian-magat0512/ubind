/**
 * Represents context entities config resource.
 */
export interface ContextEntitiesConfigResourceModel {
    includeContextEntities: Array<string>;
    reloadIntervalSeconds: number;
    reloadWithOperations: Array<string>;
}
