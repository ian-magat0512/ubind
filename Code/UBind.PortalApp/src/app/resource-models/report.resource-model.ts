import { Entity } from "../models/entity";
import { ProductResourceModel } from "./product.resource-model";

/**
 * Report resource model
 */
export interface ReportResourceModel extends Entity {
    tenantId: string;
    name: string;
    description: string;
    products: Array<ProductResourceModel>;
    sourceData: string;
    mimeType: string;
    filename: string;
    body: string;
    isDeleted: boolean;
    createdDateTime: string;
    lastModifiedDateTime: string;
}

/**
 * Report creation model
 */
export interface ReportCreateModel extends Entity {
    tenantId: string;
    name: string;
    description: string;
    productIds: Array<string>;
    sourceData: string;
    mimeType: string;
    filename: string;
    body: string;
    isDeleted: boolean;
}

/**
 * Report generation request resource model
 */
export interface ReportGenerateModel {
    from: string;
    to: string;
    includeTestData: boolean;
    environment?: string;
    tenantId?: string;
    timeZoneId: string;
}

/**
 * Report file model, which is the file produced from generating the report
 */
export interface ReportFileResourceModel {
    reportFileId: string;
    filename: string;
    size: number;
    createdDateTime: string;
    mimeType: string;
}
