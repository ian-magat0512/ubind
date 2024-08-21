import { Entity } from "@app/models/entity";

/**
 * Model for message.
 */
export interface MessageResourceModel extends Entity {
    recipient: string;
    subject: string;
    message: string;
    tags: Array<Tag>;
    relationship: Array<Relationship>;
    type: 'email' | 'sms';
}

/**
 * Tag resource model
 */
export interface Tag {
    value: string;
    tagType: string;
    entityId: string;
    entityType: string;
}

/**
 * Tag resource model
 */
export interface Relationship {
    entityId: string;
    entityType: string;
    relationshipType: string;
}
