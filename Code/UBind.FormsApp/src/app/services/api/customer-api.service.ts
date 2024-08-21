import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * Export customer API service class.
 * TODO: Write a better class header: customer API functions.
 */
@Injectable({
    providedIn: 'root',
})
export class CustomerApiService {
    public constructor(
        private httpClient: HttpClient,
    ) {
    }

    public hasAccount(tenantId: string, customerId: string): Observable<boolean> {
        const options: object = { params: { tenant: tenantId } };
        let requestUrl: string = '/api/v1/customer/' + customerId + '/has-account';
        return this.httpClient.get<boolean>(requestUrl, options);
    }
}
