import { RoutePaths } from "./route-path";

/**
 * The helper class for route paths.
 */
export class RoutePathsHelper {
    /**
     * Returns 'true' if the given search value is within the route paths, otherwise, return 'false'.
     * @param searchValue the route path to search
     */
    public static isInRoutePaths(searchValue: string): boolean {
        for (let path in RoutePaths) {
            if (RoutePaths[path] == searchValue) {
                return true;
            }
        }
        return false;
    }
}
