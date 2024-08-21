import { ProductResourceModel } from "@app/resource-models/product.resource-model";
import { QueryRequestHelper } from "@app/helpers/query-request.helper";
import { ProductFilter } from "@app/models/product-filter";
import { ProductApiService } from "@app/services/api/product-api.service";
import { EventService } from "@app/services/event.service";
import { FilterSelection } from "@app/viewmodels/filter-selection.viewmodel";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";

/**
 * The base class for products entity list 
 * to filter out the products.
 */
export abstract class ProductBaseEntityListPage {
    public isProductLoading: boolean = false;
    public filterProducts: Array<ProductFilter> = [];
    public products: Array<ProductResourceModel> = [];
    public tenantId: string;
    public destroyed: Subject<void> = new Subject<void>();

    protected abstract handleProductsWasLoaded(): void;

    public constructor(
        protected productApiService: ProductApiService,
        protected eventService: EventService,
    ) { }

    public loadProducts(): void {
        this.isProductLoading = true;
        this.productApiService.getProductsByTenantId(this.tenantId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isProductLoading = false),
            )
            .subscribe((products: Array<ProductResourceModel>) => {
                this.products = products;
                this.filterProducts = [];
                this.products.forEach((product: ProductResourceModel) => {
                    this.filterProducts.push({
                        id: product.id,
                        name: product.name,
                        value: false,
                    });
                });

                this.handleProductsWasLoaded();
            });
    }

    public handleProductFilterUpdateEvent(filterSelections: Array<FilterSelection>): void {
        this.eventService.productFilterUpdate(QueryRequestHelper.constructProductFilters(
            this.filterProducts,
            filterSelections,
        ));
    }
}
