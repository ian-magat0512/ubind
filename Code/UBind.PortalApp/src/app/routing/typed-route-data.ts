import { Type } from "@angular/core";
import { CanActivate } from "@angular/router";
import { Permission } from "@app/helpers";
import { PageRouteIndentifier } from "@app/helpers/page-route-identifier.helper";
import { UserType } from "@app/models/user-type.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { MenuLayoutSetting, ScreenLayoutSetting } from "@app/services/layout-manager.service";

/**
 * Defines the 'data' that can be set against a route, that is used by the route guards and the layout manager.
 */
export interface TypedRouteData {

    /**
     * A list of user types that are allowed to access the page.
     * This gets enforced in the route guard "UserTypeGuard".
     */
    userTypes?: Array<UserType>;

    /**
     * A list of permissions that are required to access the page.
     * This gets enforced in the route gaurd "PermissionGuard".
     */
    mustHavePermissions?: Array<Permission>;

    /**
     * A list of permissions whereby the user must have at least one of them to access the page.
     * This gets enforced in the route gaurd "PermissionGuard".
     */
    mustHaveOneOfPermissions?: Array<Permission>;

    /**
     * A list of sets of permissions whereby the user must have a permission from each set to access the page.
     */
    mustHaveOneOfEachSetOfPermissions?: Array<Array<Permission>>;

    /**
     * The master component to show in the master view (left pane) when this page is loaded in full screen mode.
     */
    masterComponent?: any;

    /**
     * The CSS class to place on the master view container when this page is loaded in full screen mode.
     */
    masterContainerClass?: string;

    /**
     * The detail component to show in the detail view (right pane) when this page is loaded in full screen mode.
     * Requires the component used for the route to be to be "ShowMasterComponentWhenNotSplit".
     * 
     */
    detailComponent?: Type<DetailPage>;

    /**
     * The message to show when using the "ShowMasterComponentWhenNotSplit" component, and there is no selection.
     * This will ensure the right hand pane shows this message when there is no selection.
     */
    noSelectionMessage?: string;

    /**
     * The route to redirect to when the environment changes.
     * This should be a route locale to the current module's routes. You can't redirect to another module.
     * This is handled in EnvironmentChangeRedirectService
     */
    onEnvironmentChangeRedirectTo?: string;

    /**
     * Allows you to give a route an identifier to uniquely identify it.
     * This is necessary where you have multiple routes which use the same component, and you navigate from one to
     * the other. If they don't have a unique identifier, the router will think you are navigating to the same route,
     * and won't do anything. With a unique identifier, the router will navigate to the route, and create a new instance
     * of the component with the relevant data.
     */
    routeIdentifier?: PageRouteIndentifier;

    /**
     * Specifies whether the menu is to be shown or not. When true, the menu is not rendered on the page.
     * This is used on entry pages such as the login page, or activation page. There can be no menu on these pages
     * because we don't know who the user is yet, or what they can access.
     */
    menuDisabled?: boolean;

    /**
     * Specifies whether the route should use a split layout (master/detail) or not.
     * You should not normally need to set this, because if you specify a master component then
     * this is implied.
     */
    layout?: ScreenLayoutSetting;

    /**
     * Specifieds whether then should be initially collapsed or expanded.
     * This would only have an effect if the screen size is large enough that the menu is visible from the start.
     */
    initialMenuState?: MenuLayoutSetting;

    /**
     * A list of Type Guards which are executed sequentially, even if they return a Promise or Observable.
     * You will need to ensure the route's canActivate property contains: [SequentialRouteGuardChecker]
     */
    sequentialGuards?: Array<Type<CanActivate>>;
}
