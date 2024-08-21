import { Injectable } from "@angular/core";
import { AuthenticationService } from "../services/authentication.service";

/**
 * provides the user type specific route names
 */
@Injectable({
    providedIn: 'root',
})
export class UserTypePathHelper {
    public constructor(
        private authService: AuthenticationService,
    ) { }

    public get customer(): string {
        return 'customer';
    }

    public get user(): string {
        return 'user';
    }

    public get role(): string {
        return 'role';
    }

    public get policyTransaction(): string {
        return 'transaction';
    }

    public get customerHome(): string {
        return 'my-home';
    }

    public get organisation(): string {
        return 'organisation';
    }

    public get quote(): string {
        if (this.authService.isCustomer()) {
            return 'my-quotes';
        } else {
            return 'quote';
        }
    }

    public get policy(): string {
        if (this.authService.isCustomer()) {
            return 'my-policies';
        } else {
            return 'policy';
        }
    }

    public get person(): string {
        return 'person';
    }

    public get claim(): string {
        if (this.authService.isCustomer()) {
            return 'my-claims';
        } else {
            return 'claim';
        }
    }

    public get home(): string {
        if (this.authService.isCustomer()) {
            return this.customerHome;
        } else if (this.authService.isMasterUser()) {
            return 'master-home';
        } else {
            return 'home';
        }
    }

    public get message(): string {
        if (this.authService.isCustomer()) {
            return 'my-message';
        } else if (this.authService.isMasterUser()) {
            return 'master-message';
        } else {
            return 'message';
        }
    }

    public get account(): string {
        if (this.authService.isCustomer()) {
            return 'my-account';
        } else {
            return 'account';
        }
    }

    public get report(): string {
        return 'report';
    }

    public get portal(): string {
        return 'portal';
    }

}
