/**
 * Tenant's password expiry settings resource model
 */
export interface TenantPasswordExpirySettingResourceModel {
    passwordExpiryEnabled: boolean;
    maxPasswordAgeDays: number;
}
