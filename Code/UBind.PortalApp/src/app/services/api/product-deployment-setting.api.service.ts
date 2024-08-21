import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '@app/services/api.service';
import { ProductDeploymentSettingResourceModel } from '@app/resource-models/product-deployment-setting.resource-model';

/**
 * Export product deployment setting API service class.
 * TODO: Write a better class header: product deployment settings API functions.
 */
@Injectable({ providedIn: 'root' })
export class ProductDeploymentSettingApiService {
    public constructor(private api: ApiService) {
    }

    public getById(tenant: string, product: string): Observable<ProductDeploymentSettingResourceModel> {
        const _url: string = '/' + tenant + '/' + product + '/productdeploymentsettings';
        return this.api.get(_url).pipe(map((res: ProductDeploymentSettingResourceModel) => res));
    }

    public update(
        tenant: string,
        product: string,
        model: ProductDeploymentSettingResourceModel,
    ): Observable<ProductDeploymentSettingResourceModel> {
        const _url: string = '/' + tenant + '/' + product + '/productdeploymentsettings';
        return this.api.put(_url, model, null).pipe(map((res: ProductDeploymentSettingResourceModel) => res));
    }
}
