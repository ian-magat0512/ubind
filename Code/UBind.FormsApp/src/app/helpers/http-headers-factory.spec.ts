import { } from 'jasmine';
import { HttpHeadersFactory, MediaType } from './http-headers-factory';
import { HttpHeaders } from '@angular/common/http';

describe('HttpHeadersFactory', () => {
    it('should generate correct content type header for json', () => {
        const headers: HttpHeaders = HttpHeadersFactory.create().withContentType(MediaType.Json).build();
        expect(headers.get('Content-Type')).toBe('application/json');
    });

    it('should generate correct accept header for json', () => {
        const headers: HttpHeaders = HttpHeadersFactory.create().withAccept(MediaType.Json).build();
        expect(headers.get('Accept')).toBe('application/json');
    });

    it('should generate correct accept header for multiple mime types', () => {
        const headers: HttpHeaders = HttpHeadersFactory.create().withAccept(MediaType.Json,
            MediaType.Text, MediaType.Any).build();
        expect(headers.get('Accept')).toBe('application/json,text/plain,*/*');
    });
});
