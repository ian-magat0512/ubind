/**
 * Represents a person detail.
 */
export interface PersonDetailModel {
    id: string;
    tenantId: string;
    mobilePhoneNumber: string;
    homePhoneNumber: string;
    workPhoneNumber: string;
    environment: string;
    profilePictureId: string;
    userType: string;
    blocked: boolean;
    status: string;
    createdDateTime: string;
    lastModifiedDateTime: string;
    fullName: string;
    preferredName: string;
    email: string;
    alternativeEmail: string;
    mobilePhone: string;
    homePhone: string;
    workPhone: string;
    loginEmail: string;
    namePrefix: string;
    firstName: string;
    middleNames: string;
    lastName: string;
    nameSuffix: string;
    company: string;
    title: string;
    repeatingFields: Array<RepeatingFieldsModel>;
    repeatingAddressFields: Array<RepeatingAddressFieldsModel>;
    serializedRepeatingFields: string;
    serializedRepeatingAddressFields: string;
    organisationId: string;
    organisationAlias: string;
}

/**
 * Represents a repeating field.
 */
export interface RepeatingFieldsModel {
    referenceId: string;
    parentFieldName: string;
    name: string;
    label: string;
    customLabel: string;
    value: string;
    sequenceNo: number;
}

/**
 * Represents a repeating address field.
 */
export interface RepeatingAddressFieldsModel {
    address: string;
    suburb: string;
    state: string;
    postcode: string;
    referenceId: string;
    parentFieldName: string;
    name: string;
    label: string;
    customLabel: string;
    value: string;
    sequenceNo: number;
}
