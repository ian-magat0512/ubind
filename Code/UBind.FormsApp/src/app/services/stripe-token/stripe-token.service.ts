import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { CreditCardDetails } from '../../models/credit-card-details';
import { StripeTokenResponse } from './stripe-token-response';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpHeadersFactory, MediaType } from '../../helpers/http-headers-factory';

/**
 * Export stripe token service class.
 * TODO: Write a better class header: stripe token functions.
 */
@Injectable()
export class StripeTokenService {

    public constructor(private httpService: HttpClient) { }

    public getToken(
        publicApiKey: string,
        creditcardDetails: CreditCardDetails,
    ): Observable<StripeTokenResponse> {
        let tokenEndpoint: string = "https://api.stripe.com/v1/tokens";
        let body: string = `card[number]=${creditcardDetails.number}&` +
            `card[exp_month]=${creditcardDetails.expiryMonthMM}&` +
            `card[exp_year]=${creditcardDetails.expiryYearYYYY}&` +
            `card[cvc]=${creditcardDetails.ccv}&` +
            `card[name]=${creditcardDetails.name}`;
        let headers: HttpHeaders = this.getHeaders(publicApiKey);
        return this.httpService
            .post(tokenEndpoint, body, {
                headers: headers,
            })
            .pipe(map((response: Response) => new StripeTokenResponse(response)));
    }

    private getHeaders(publicApiKey: string): HttpHeaders {
        return HttpHeadersFactory
            .create()
            .withContentType(MediaType.UrlEncodedForm)
            .withBearerAuthentication(publicApiKey)
            .build();
    }
}
