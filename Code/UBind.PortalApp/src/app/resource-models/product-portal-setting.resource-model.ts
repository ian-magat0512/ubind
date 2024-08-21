/**
 * Represents a resource model of portal product settings
 * to enable and disable the creation of new quotes for a Product on a per-Portal basis
 */
export interface ProductPortalSettingResourceModel {
    name: string;
    portalId: string;
    productId: string;
    isNewQuotesAllowed: boolean;
    createdTicksSinceEpoch: number;
}
