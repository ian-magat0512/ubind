import { Entity } from "@app/models/entity";
import { DataTableSchema } from "@app/models/data-table-definition.model";

/**
 * Resource data table definition
 */
export interface DataTableDefinitionResourceModel extends Entity {
    entityType: string;
    entityId: string;
    name: string;
    alias: string;
    databaseTableName: string;
    recordCount: number;
    columnCount: number;
    lastModifiedTimestamp: string;
    lastModifiedTicksSinceEpoch: number;
    id: string;
    tableSchema: DataTableSchema;
    createdTimestamp: string;
    createdTicksSinceEpoch: number;
    memoryCachingEnabled: boolean;
    cacheExpiryInSeconds: number;
  }
