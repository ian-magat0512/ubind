import { Entity } from "@app/models/entity";
import { Relationship, Tag } from "@app/resource-models/message.resource-model";

/**
 * Sms resource model.
 */
export interface SmsResourceModel extends Entity {
    to: string;
    from: string;
    message: string;
    createdDateTime: string;
    customer: any;
    policy: any;
    quote: any;
    claim: any;
    user: any;
    policyTransaction: any;
    organisation: any;
    tags: Array<Tag>;
    relationship: Array<Relationship>;
}
