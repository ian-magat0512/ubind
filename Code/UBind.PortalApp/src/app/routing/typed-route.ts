import { Route } from "@angular/router";
import { TypedRouteData } from "./typed-route-data";

/**
 * Extends angular's Route interace to allow for a strongly typed route data property.
 */
export interface TypedRoute extends Route {
    data?: TypedRouteData;
}

export declare type TypedRoutes = Array<TypedRoute>;
