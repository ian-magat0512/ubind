import { Observable } from "rxjs";

/**
 * Represents a service that can load entities from somewhere, e.g. from an API
 */
export interface EntityLoaderService<EntityType> {
    getList(params?: Map<string, string | Array<string>>): Observable<Array<EntityType>>;
    getById(id: string, params?: Map<string, string | Array<string>>): Observable<EntityType>;
}
