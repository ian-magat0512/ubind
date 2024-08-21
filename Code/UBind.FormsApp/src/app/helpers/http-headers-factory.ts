import { HttpHeaders } from '@angular/common/http';

/**
 * Export http headers factory class.
 * TODO: Write a better class header: http header functions.
 */
export class HttpHeadersFactory {
    private headers: HttpHeaders = new HttpHeaders();

    public static create(): HttpHeadersFactory {
        return new HttpHeadersFactory();
    }

    public withContentType(mediaType: MediaType): HttpHeadersFactory {
        this.headers = this.headers.append('Content-Type', mediaType);
        return this;
    }

    public withAccept(...mediaTypes: Array<MediaType>): HttpHeadersFactory {
        this.headers = this.headers.append('Accept', mediaTypes.toString());
        return this;
    }

    public withBearerAuthentication(authenticationToken: string): HttpHeadersFactory {
        this.headers = this.headers.append('Authorization', 'Bearer ' + authenticationToken);
        return this;
    }

    public withHeader(name: string, value: string): HttpHeadersFactory {
        this.headers = this.headers.append(name, value);
        return this;
    }

    public build(): HttpHeaders {
        return this.headers;
    }
}

export enum MediaType {
    Json = 'application/json',
    Text = 'text/plain',
    Any = '*/*',
    UrlEncodedForm = 'application/x-www-form-urlencoded'
}
