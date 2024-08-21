import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FormsAppContextModel } from '@app/models/forms-app-context.model';

/**
 * API service for requesting the contextual information that the form will execute in.
 */
@Injectable()
export class AppContextApiService {
    public constructor(private httpClient: HttpClient) {}

    public getFormsAppContext(
        tenant: string,
        product: string,
        organisation: string,
        portal: string,
        quoteId: string,
    ): Observable<FormsAppContextModel> {
        let options: HttpParams = new HttpParams()
            .append('tenant', tenant)
            .append('product', product);
        if (organisation && organisation != 'null') {
            options = options.append('organisation', organisation);
        }
        if (portal && portal != 'null') {
            options = options.append('portal', portal);
        }
        if (quoteId && quoteId != 'null') {
            options = options.append('quoteId', quoteId);
        }

        const baseUrl: string = '/api/v1/app-context';
        return this.httpClient.get<FormsAppContextModel>(`${baseUrl}/forms`, { params: options });
    }
}
