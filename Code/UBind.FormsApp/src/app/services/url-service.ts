import { Injectable } from "@angular/core";

/**
 * Provides facilities to access the URL
 */
@Injectable({
    providedIn: 'root',
})
export class UrlService {

    public getQueryStringParams(): URLSearchParams {
        return new URLSearchParams(window.location.search);
    }
}
