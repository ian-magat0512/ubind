import { HttpClient } from '@angular/common/http';
import { formatDate } from "@angular/common";
import { Injectable } from '@angular/core';
import * as forge from 'node-forge';
import { ApplicationService } from '@app/services/application.service';
import { take } from 'rxjs/operators';

/**
 * Tool for encyrpting and decrypting data to be sent/received from the uBind system.
 */
@Injectable()
export class EncryptionService {

    private publicKey: string;
    private apiRequestUrl: string = `/api/v1/security/rsa-key-pair/public`;

    public constructor(private http: HttpClient, appService: ApplicationService) {
        let apiOrigin: string = appService.apiOrigin;
        if (apiOrigin) {
            let absoluteUrl: string = apiOrigin + this.apiRequestUrl;
            this.apiRequestUrl = absoluteUrl;
        }
        this.loadPublicKey();
    }

    public encryptData(plainText: string): string {
        if (!this.publicKey) {
            throw new Error(`Unable to encrypt data. The security configuration key is missing.`);
        }
        let keyExpression: string = this.publicKey.replace('\r\n', '');
        const pk: any = forge.pki.publicKeyFromPem(keyExpression);
        return forge.util.encode64(pk.encrypt(this.tagDataWithTimestamp(plainText))).toString();
    }

    private loadPublicKey(): void {
        this.http.get(this.apiRequestUrl)
            .pipe(take(1))
            .subscribe(
                (res: any) => {
                    this.publicKey = res;
                },
                (err: any) => {
                    let errorString: string = err.error.error.toString();
                    if (!errorString.startsWith(`SyntaxError:`)) {
                        throw err;
                    }

                    // grab pemkey from error. For some reason, http client tries to parse it
                    // to JSON which causes an error.
                    this.publicKey = err.error.text;
                },
            );
    }

    private tagDataWithTimestamp(plainText: string): string {
        if (!plainText) {
            return plainText;
        }

        let currentLocalTimestamp: any = new Date();
        let currentUTCTimestamp: any = new Date(currentLocalTimestamp.toUTCString().slice(0, -4));
        let formattedTimestamp: string = formatDate(currentUTCTimestamp, "yyyyMMddHHmmss", "en-AU");
        return `${plainText}||${formattedTimestamp}`;
    }
}
