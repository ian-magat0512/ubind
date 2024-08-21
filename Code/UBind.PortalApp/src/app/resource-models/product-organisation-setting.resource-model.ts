/**
 * Represents a resource model of product organisation settings
 * to enable and disable the creation of new quotes for a Product on a per-Organisation basis
 */
export interface ProductOrganisationSettingResourceModel {
    name: string;
    organisationId: string;
    productId: string;
    isNewQuotesAllowed: boolean;
    createdTicksSinceEpoch: number;
}
