import { Injectable } from "@angular/core";
import { ResilientStorage } from "@app/storage/resilient-storage";

/**
 * Service for authenticating user.
 */
@Injectable()
export class AuthenticationService {

    public constructor(public storage: ResilientStorage) {
    }

    public getAuthenticationToken(): string {
        return this.storage.getItem('ubind.accessToken') || '';
    }
}
