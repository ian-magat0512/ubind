import { Injectable } from "@angular/core";
import { NavigationExtras, Params } from "@angular/router";
import { NavProxyService } from "./nav-proxy.service";
import { AuthenticationService } from "./authentication.service";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";

/**
 * Service to redirect the user to the originally requested page after login.
 * If someone uses a deep link to access a portal page, but they are not logged in, then we
 * need to store their requested page, take them to the login page, and redirect to their originally requested
 * page after logging in.
 */
@Injectable({ providedIn: 'root' })
export class LoginRedirectService {
    private _pathSegments: Array<string>;
    private _queryParams: Params;

    public constructor(
        private navProxy: NavProxyService,
        private authService: AuthenticationService,
        private userPath: UserTypePathHelper,
    ) {
    }

    public set pathSegments(value: Array<string>) {
        this._pathSegments = value;
    }

    public set queryParams(value: Params) {
        this._queryParams = value;
    }

    public clear(): void {
        this._pathSegments = [];
        this._queryParams = null;
    }

    public redirect(): void {
        if (!this.authService.isAuthenticated()) {
            this.navProxy.navigateRoot(['login']);
        } else if (this._pathSegments && this._pathSegments.length > 0) {
            this.navProxy.navigateRoot(this._pathSegments, { queryParams: this._queryParams }, true);
            this.clear();
        } else {
            this.navProxy.navigateRoot([this.userPath.home]);
        }
    }

    public getRedirectPath(): string {
        let path: string = null;
        if (this._pathSegments && this._pathSegments.length > 0) {
            path = '/' + this._pathSegments.join('/');
            if (this._queryParams && Object.keys(this._queryParams).length > 0) {
                path += '?' + this.queryParamsToString(this._queryParams);
            }
        }
        return path;
    }

    public getRedirectCommandsAndExtras(): { commands: Array<string>; extras: NavigationExtras } {
        let commands: Array<string> = null;
        let extras: NavigationExtras = null;
        if (this._pathSegments && this._pathSegments.length > 0) {
            commands = this._pathSegments;
            if (this._queryParams && Object.keys(this._queryParams).length > 0) {
                extras = { queryParams: this._queryParams };
            }
        }
        return { commands, extras };
    }

    private queryParamsToString(queryParams: Params): string {
        let result: string = '';
        for (let key in queryParams) {
            if (result.length > 0) {
                result += '&';
            }
            result += key + '=' + queryParams[key];
        }
        return result;
    }
}
