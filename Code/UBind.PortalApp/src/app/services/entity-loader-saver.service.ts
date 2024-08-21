import { EntityLoaderService } from "./entity-loader.service";
import { Observable } from "rxjs";

/**
 * Represents a service which can load and save entities, e.g. via a set of api endpoints
 */
export interface EntityLoaderSaverService<EntityType> extends EntityLoaderService<EntityType> {
    create(model: any): Observable<EntityType>;
    update(id: string, model: any): Observable<EntityType>;
}
