import { Injectable } from "@angular/core";
import { AdditionalPropertyDefinitionContextType } from "@app/models/additional-property-context-type.enum";
import { AdditionalPropertyDefinition } from "@app/models/additional-property-item-view.model";
import { EntityType } from "@app/models/entity-type.enum";
import { UserType } from "@app/models/user-type.enum";
import { AdditionalPropertyDefinitionApiService } from "./api/additional-property-definition-api.service";

/**
 * Service for additional property value.
 */
@Injectable({ providedIn: 'root' })
export class AdditionalPropertyDefinitionService {
    public constructor(private additionalPropertyDefinitionApiService: AdditionalPropertyDefinitionApiService) {
    }
    public verifyIfUserIsAllowedToProceed(
        tenant: string,
        userType: UserType,
        contextType: AdditionalPropertyDefinitionContextType,
        entityType: EntityType,
        contextId: string,
        parentContextId: string,
        mergeResult: boolean,
        canEditAdditionalPropertyValues: boolean,
        successCallback: () => void,
        falseCallback: () => void,
    ): void {
        if (userType === UserType.Client || userType === UserType.Customer) {
            this.additionalPropertyDefinitionApiService
                .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextIdAndParentContextId(
                    tenant,
                    contextType,
                    entityType,
                    contextId,
                    parentContextId,
                    mergeResult,
                )
                .subscribe(
                    (result: Array<AdditionalPropertyDefinition>) => {
                        if (result && result.length > 0) {
                            let requiredProperties: Array<AdditionalPropertyDefinition> = result.filter(
                                (res: AdditionalPropertyDefinition) => res.isRequired,
                            );
                            if (requiredProperties.length == 0) {
                                successCallback();
                            } else {
                                if (!canEditAdditionalPropertyValues) {
                                    falseCallback();
                                } else {
                                    successCallback();
                                }
                            }
                        } else {
                            successCallback();
                        }
                    },
                    (err: any) => {
                        throw err;
                    },
                );
        } else {
            successCallback();
        }
    }
}
