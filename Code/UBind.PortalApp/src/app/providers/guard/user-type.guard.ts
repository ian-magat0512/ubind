import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthenticationService } from '@app/services/authentication.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { Errors } from '@app/models/errors';
import { UserType } from '../../models/user-type.enum';

/**
 * Route guard to stop access to routes based on user type.
 * Each route can specify a property "allowUserTypes" whic is what's checked during route activation.
 */
@Injectable({ providedIn: 'root' })
export class UserTypeGuard implements CanActivate {

    public constructor(
        private authService: AuthenticationService,
        private errorHandlerService: ErrorHandlerService,
    ) {
    }

    public canActivate(
        next: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): boolean | UrlTree {
        const allowedUserTypes: Array<UserType> = next.data.allowedUserTypes;
        const userType: UserType = this.authService.userType;
        if (allowedUserTypes.includes(userType)) {
            return true;
        } else {
            this.errorHandlerService.handleError(Errors.User.AccessDenied());
            return false;
        }
    }
}
