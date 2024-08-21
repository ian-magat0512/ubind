import { ProductResourceModel } from './product.resource-model';

/**
 * Resource model for a system alert, which can be configured to send an email when 
 * policy/claim/invoice/credit-note numbers are running low
 */
export interface SystemAlertResourceModel {
    id: string;
    systemAlertType: SystemAlertTypeModel;
    tenantId: string;
    productId: string;
    systemAlertTypeId: string;
    product: ProductResourceModel;
    disabled: boolean;
    warningThreshold: number;
    criticalThreshold: number;
    alertMessage: string;
}
/**
 * Stores details for the type of system alert, including an icon and name.
 */
export interface SystemAlertTypeModel {
    id: string;
    name: string;
    icon: string;
}
