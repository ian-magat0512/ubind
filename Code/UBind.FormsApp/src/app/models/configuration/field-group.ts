import { Field } from "./field";

/**
 * Represents a group of fields to be rendered in a single row.
 */
export interface FieldGroup {
    className: string;
    fieldGroup: Array<Field>;
}
