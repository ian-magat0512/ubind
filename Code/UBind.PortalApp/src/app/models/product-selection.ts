/**
 * Model to hold the selection of a product, which is typically used on the
 * dashboard/charts page to know which products to show chart data for
 */
export interface ProductSelection {
    id: string;
    name: string;
    alias: string;
    isChecked: boolean;
}
