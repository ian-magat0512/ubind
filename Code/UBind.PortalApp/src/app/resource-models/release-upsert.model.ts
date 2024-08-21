import { ReleaseType } from "@app/models/release-type.enum";

export interface ReleaseUpsertModel {
    tenant: string;
    product: string;
    description: string;
    type: ReleaseType;
}
