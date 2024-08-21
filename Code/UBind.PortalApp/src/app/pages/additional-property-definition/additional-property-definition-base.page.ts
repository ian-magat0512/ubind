import { Directive, ElementRef, Injector, OnInit } from "@angular/core";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { OrganisationResourceModel } from "../../resource-models/organisation/organisation.resource-model";
import { StringHelper } from "../../helpers";
import { RouteHelper } from "../../helpers/route.helper";
import { AdditionalPropertyDefinitionContextType } from "../../models/additional-property-context-type.enum";
import { EntityType } from "../../models/entity-type.enum";
import { OrganisationApiService } from "../../services/api/organisation-api.service";
import { EventService } from "../../services/event.service";
import { ProductService } from "../../services/product.service";
import { TenantService } from "../../services/tenant.service";
import { DetailPage } from "../master-detail/detail.page";
import { AppConfigService } from "@app/services/app-config.service";
import { ActivatedRoute } from "@angular/router";

/**
 * Master page class for additional property pages
 * This class contains common properties needed by additional property pages.
 */
@Directive({ selector: '[appAdditionalPropertyDefinitionBase]' })
export abstract class AdditionalPropertyDefinitionBasePage extends DetailPage implements OnInit {
    private _contextId: string;
    private _contextAlias: string;
    private _entityType: EntityType;
    private _contextName: string;
    private _contextType: AdditionalPropertyDefinitionContextType;
    private _parentContextTenantId: string;
    private _defaultGuidId: string = '00000000-0000-0000-0000-000000000000';
    public get contextId(): string {
        return this._contextId;
    }
    public get entityType(): EntityType {
        return this._entityType;
    }
    public get contextName(): string {
        return this._contextName;
    }
    public get contextType(): AdditionalPropertyDefinitionContextType {
        return this._contextType;
    }
    public get parentContextId(): string {
        return this._parentContextTenantId;
    }
    public get defaultGuidId(): string {
        return this._defaultGuidId;
    }
    protected pathTenantAlias: string;
    protected portalTenantAlias: string;

    public constructor(
        eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        public routeHelper: RouteHelper,
        protected tenantService: TenantService,
        protected productService: ProductService,
        protected organisationApiService: OrganisationApiService,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        private route: ActivatedRoute,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.route.params.subscribe((params: any) => {
            this.pathTenantAlias = params.tenantAlias;
            this.portalTenantAlias = params.portalTenantAlias;
        });
    }

    private getParentContextTenantId(): string {
        return this.pathTenantAlias || this.portalTenantAlias;
    }

    /**
     * We need to check the segment on a specific index because entity type is also being pased as part of the
     * url. If we only check the presence of it via index it is quite possible that some entity will have multiple
     * segments which has the same value
     * i.e portal/ubind/tenant/australian-reliance/additional-property-definition/product
     * stands for Context (Tenant)  and Entity Type (Product)
     * portal/australian-reliance/product/ar-prod/additional-property-definition/product
     * stands for Context (Product) and Entity Type (Product)
     * portal/ubind/tenant/australian-reliance/product/ar-prod/additional-property-definition/product
     * stands for Context (Product) and Entity Type (Product).
     */
    private getContextTypeBasedOnThePathSegments(pathSegments: Array<string>): AdditionalPropertyDefinitionContextType {
        let additionalPropertyDefinitionIndex: number
            = pathSegments.findIndex((pathSegment: string) => pathSegment === 'additional-property-definition');
        let index: number = pathSegments.findIndex((pathSegment: string) => pathSegment === 'organisation');
        if (index > -1 && index < additionalPropertyDefinitionIndex) {
            return AdditionalPropertyDefinitionContextType.Organisation;
        }
        index = pathSegments.findIndex((pathSegment: string) => pathSegment === 'product');
        if (index > -1 && index < additionalPropertyDefinitionIndex) {
            return AdditionalPropertyDefinitionContextType.Product;
        }
        index = pathSegments.findIndex((pathSegment: string) => pathSegment === 'tenant');
        if (index > -1 && index < additionalPropertyDefinitionIndex) {
            return AdditionalPropertyDefinitionContextType.Tenant;
        }

        throw new Error("When trying to get the context type of the additional property definition, we searched for "
            + "organisation, product, and tenant in the path segments, but could not find a match. We were therefore "
            + "unable to determine the context type of the additional property definition.");
    }

    private async getContextName(): Promise<string> {
        let contextName: string = "";
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        switch (this._contextType) {
            case AdditionalPropertyDefinitionContextType.Product: {
                contextName = await this.productService
                    .getProductNameFromAlias(this.routeHelper.getContextTenantAlias(), this._contextAlias);
                break;
            }
            case AdditionalPropertyDefinitionContextType.Organisation: {
                let organisationResourceModel: OrganisationResourceModel =
                    await this.organisationApiService.getById(this._contextAlias, params).toPromise();
                contextName = organisationResourceModel.name;
                break;
            }
            case AdditionalPropertyDefinitionContextType.Tenant: {
                contextName = await this.tenantService.getTenantNameFromAlias(this.routeHelper.getContextTenantAlias());
                break;
            }
            default: {
                throw new Error(
                    `When trying to get the context name, the context type "${this._contextType}" was not one of the "
                    + "expected types`,
                );
            }
        }
        return contextName;
    }

    // returns contextId that is guid instead of strings coming from the url.
    private async getGuidContextId(contextId: string): Promise<string> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        switch (this._contextType) {
            case AdditionalPropertyDefinitionContextType.Tenant: {
                this._contextAlias = contextId;
                contextId = await this.tenantService.getTenantIdFromAlias(contextId);
                break;
            }
            case AdditionalPropertyDefinitionContextType.Product: {
                this._contextAlias = contextId;
                contextId = await this.productService.getProductIdFromAlias(this._parentContextTenantId, contextId);
                break;
            }
            case AdditionalPropertyDefinitionContextType.Organisation: {
                this._contextAlias = contextId;
                let organisationResourceModel: OrganisationResourceModel =
                    await this.organisationApiService.getById(contextId, params).toPromise();
                contextId = organisationResourceModel.id;
                break;
            }
            default:
                break;
        }

        return contextId;
    }

    /**
     * Since that we are now making an api call to retrieve the context name then to be safe it is safe to call this
     * method from ionViewWillEnter just to make sure that every base parameters are loaded.
     * so we need to get the childmost route and then pull the params from there.
     * @param postAction
     */
    protected async loadAllBaseParametersBeforeLoadingOtherData(postAction: { (): void }): Promise<void> {

        this._contextId = this.routeHelper.getParam('contextId') || this.routeHelper.getParam('tenantAlias');
        let entityTypeAsKebabCase: string = this.routeHelper.getParam('entityType') as string;
        let pascalCaseEntityType: string = StringHelper.toPascalCase(entityTypeAsKebabCase);
        if (!EntityType[pascalCaseEntityType]) {
            this.navigateToEntityDetailPage();
            return;
        }
        this._entityType = pascalCaseEntityType as EntityType;
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        this._contextType = this.getContextTypeBasedOnThePathSegments(pathSegmentsAfter);
        this._parentContextTenantId = this.getParentContextTenantId();
        this._contextId = await this.getGuidContextId(this._contextId);
        this.getContextName()
            .then(
                (result: string) => {
                    this._contextName = result;
                    postAction();
                },
                (err: any) => {
                    this.errorMessage = 'There was a problem loading the context name';
                    throw err;
                },
            );
    }

    private navigateToEntityDetailPage(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        let newSegments: Array<string> = pathSegments.filter(
            (ps: string) => ps !== "additional-property-definition" && ps !== this.contextId,
        );
        this.navProxy.navigateForward(newSegments);
    }
}
