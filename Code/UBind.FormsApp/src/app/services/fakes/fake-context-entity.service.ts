import { ContextEntitiesConfigResourceModel } from "@app/models/context-entities-config.model";

/**
 * Fake context entity service class
 */
export class FakeContextEntityService {
    private contextEntity: any = {
        "tenant": {
            "id": "00ce0e29-9f42-4f7c-bdda-9443826b0bca",
            "alias": "carl",
            "name": "Carl",
            "createdTicksSinceEpoch": 16577012277606844,
            "createdDateTime": "2022-07-13T08:33:47.7606844+00:00",
            "createdLocalDate": "13 Jul 2022",
            "createdLocalTime": "6:33 PM",
            "createdLocalTimeZone": "Australian Eastern Standard Time",
            "createdLocalTimeZoneAlias": "AEST",
            "lastModifiedTicksSinceEpoch": 16577012278441976,
            "lastModifiedDateTime": "2022-07-13T08:33:47.8441977+00:00",
            "lastModifiedLocalDate": "13 Jul 2022",
            "lastModifiedLocalTime": "6:33 PM",
            "lastModifiedLocalTimeZone": "Australian Eastern Standard Time",
            "lastModifiedLocalTimeZoneAlias": "AEST",
            "organisationId": null,
            "organisation": null,
            "disabled": false,
            "additionalProperties": null,
        },
        "organisation": {
            "id": "96956d06-1db7-4428-817e-1d33eed874e8",
            "alias": "carl",
            "name": "Carl",
            "createdTicksSinceEpoch": 16577012277857632,
            "createdDateTime": "2022-07-13T08:33:47.7857631+00:00",
            "createdLocalDate": "13 Jul 2022",
            "createdLocalTime": "6:33 PM",
            "createdLocalTimeZone": "Australian Eastern Standard Time",
            "createdLocalTimeZoneAlias": "AEST",
            "lastModifiedTicksSinceEpoch": 16577012277857632,
            "lastModifiedDateTime": "2022-07-13T08:33:47.7857631+00:00",
            "lastModifiedLocalDate": "13 Jul 2022",
            "lastModifiedLocalTime": "6:33 PM",
            "lastModifiedLocalTimeZone": "Australian Eastern Standard Time",
            "lastModifiedLocalTimeZoneAlias": "AEST",
            "tenantId": "00ce0e29-9f42-4f7c-bdda-9443826b0bca",
            "tenant": null,
            "disabled": false,
            "additionalProperties": null,
        },
        "product": {
            "id": "ae799b02-6b3f-4897-9c36-3002d76ad835",
            "alias": "dev6841",
            "name": "Dev 6841",
            "createdTicksSinceEpoch": 16577012615600272,
            "createdDateTime": "2022-07-13T08:34:21.5600273+00:00",
            "createdLocalDate": "13 Jul 2022",
            "createdLocalTime": "6:34 PM",
            "createdLocalTimeZone": "Australian Eastern Standard Time",
            "createdLocalTimeZoneAlias": "AEST",
            "lastModifiedTicksSinceEpoch": 16577012616152996,
            "lastModifiedDateTime": "2022-07-13T08:34:21.6152996+00:00",
            "lastModifiedLocalDate": "13 Jul 2022",
            "lastModifiedLocalTime": "6:34 PM",
            "lastModifiedLocalTimeZone": "Australian Eastern Standard Time",
            "lastModifiedLocalTimeZoneAlias": "AEST",
            "tenantId": "00ce0e29-9f42-4f7c-bdda-9443826b0bca",
            "tenant": null,
            "assetUrl": "https://localhost:44366/assets/carl/dev6841",
            "disabled": false,
            "additionalProperties": null,
        },
    };

    public getContextEntities(): any {
        return this.contextEntity;
    }

    public async getContextEntitiesConfig(): Promise<ContextEntitiesConfigResourceModel> {
        let config: ContextEntitiesConfigResourceModel = {
            "includeContextEntities": ["/tenant", "/organisation", "/product", "/customer", "/quote"],
            "reloadIntervalSeconds": 60,
            "reloadWithOperations": ["customer", "actualise", "bind"],
        };
        return config;
    }

    public async loadContextEntities(): Promise<any> {
        return this.contextEntity;
    }

    public async setIntervalLoadingContextEntities(formType: string, quoteType: string): Promise<void> {
        return;
    }
}
