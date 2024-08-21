import { DataTableSchema } from "@app/models/data-table-definition.model";

/**
 * This is a model for creating a data table definition 
 * and data table content from CSV data. 
 */
export interface DataTableCreateFromCsvModel {
    definitionId?: string;
    entityType?: string;
    entityId?: string;
    name: string;
    alias: string;
    csvData: string;
    tableSchema: DataTableSchema;
    memoryCachingEnabled: boolean;
    cacheExpiryInSeconds: number;
}
