import { SessionExpiryMode } from "@app/models/session-expiry-mode.enum";

/**
 * Tenant's session settings resource model
 */
export interface TenantSessionSettingResourceModel {
    sessionExpiryMode: SessionExpiryMode;
    idleTimeoutPeriodType: string;
    idleTimeout: any;
    fixLengthTimeoutInPeriodType: string;
    fixLengthTimeout: any;
}
