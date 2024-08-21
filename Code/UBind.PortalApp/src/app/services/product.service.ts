import { ProductApiService } from "./api/product-api.service";
import { Injectable } from "@angular/core";

/**
 * Export product service class.
 * This class manage product services functions.
 */
@Injectable({ providedIn: 'root' })
export class ProductService {
    public constructor(private productApiService: ProductApiService) {
    }

    public async getProductIdFromAlias(tenant: string, productAlias: string): Promise<string> {
        return await this.productApiService.getIdByAlias(productAlias, tenant).toPromise();
    }

    public async getProductNameFromAlias(tenant: string, productAlias: string): Promise<string> {
        return await this.productApiService.getNameByAlias(productAlias, tenant).toPromise();
    }
}
