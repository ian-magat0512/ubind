import { Entity } from "../models/entity";
import { Tag, Relationship } from "@app/resource-models/message.resource-model";

/**
 * Email resource model
 */
export interface EmailResourceModel extends Entity {
    recipient: string;
    subject: string;
    createdDateTime: string;
    hasAttachment: boolean;
    bbc: Array<string>;
    cc: Array<string>;
    documents: any;
    customer: any;
    policy: any;
    quote: any;
    organisation: any;
    claim: any;
    user: any;
    policyTransaction: any;
    from: string;
    htmlMessage: string;
    plainMessage: string;
    tags: Array<Tag>;
    relationship: Array<Relationship>;
}
