import { ReleaseType } from '../models/release-type.enum';
import { Entity } from '../models/entity';

/**
 * Resource model for a release
 */
export interface ReleaseResourceModel extends Entity {
    tenantId: string;
    productId: string;
    productAlias: string;
    productName: string;
    number: number;
    createdDateTime: string;
    description: string;
    status: string;
    type: ReleaseType;
    truncatedDescription: boolean;
}
