/**
 * Resource model for repeating fields
 */
export interface RepeatingFieldResourceModel {
    referenceId: string;
    parentFieldName: string;
    name: string;
    label: string;
    customLabel: string;
    value: string;
    sequenceNo: number;
    default: boolean;

    /**
     * The group name of the field for when the repeating field is a group
     */
    groupName?: string;
}

/**
 * Resource model for repeating address fields
 */
export interface RepeatingAddressFieldResourceModel extends RepeatingFieldResourceModel {
    address: string;
    suburb: string;
    state: string;
    postcode: string;
}
