import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { ReleaseType } from '../models/release-type.enum';
/**
 * Request model for creating or editing a release.
 */
export class ReleaseRequestModel {
    public constructor(release: ReleaseResourceModel) {
        this.id = release.id;
        this.tenant = release.tenantId;
        this.product = release.productId;
        this.description = release.description;
        this.type = release.type;
    }

    public id: string;
    public tenant: string;
    public product: string;
    public description: string;
    public type: ReleaseType;

}
