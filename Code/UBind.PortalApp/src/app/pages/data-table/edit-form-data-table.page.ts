import { DetailPage } from "@app/pages/master-detail/detail.page";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { FormGroup } from "@angular/forms";
import { ElementRef, Injector } from "@angular/core";
import { EventService } from "@app/services/event.service";
import { RouteHelper } from "@app/helpers/route.helper";
import { NavProxyService } from "@app/services/nav-proxy.service";

/**
 * common base class of both create and edit form data table pages.
 */
export abstract class EditFormDataTablePage extends DetailPage {
    public cachingEnabled: boolean = false;
    protected dataTableDefinitionId: string;
    public detailList: Array<DetailsListFormItem>;
    public form: FormGroup;
    private defaultCacheExpiryInSeconds: number = 500;

    public constructor(
        protected routeHelper: RouteHelper,
        protected navProxy: NavProxyService,
        protected eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
    ) {
        super(eventService, elementRef, injector);
    }

    public setFormBehavior(): void {
        // Subscribe to memoryCachingEnabled value changes
        this.form.get('memoryCachingEnabled').valueChanges.subscribe((value: any) => {
            this.cachingEnabled = value;
            const cacheExpirySecondsItem: DetailsListFormItem =
                this.detailList.find((item: DetailsListFormItem) => item.Alias == 'cacheExpiryInSeconds');
            const csvDataItem: DetailsListFormItem =
                this.detailList.find((item: DetailsListFormItem) => item.Alias == 'csvData');
            cacheExpirySecondsItem.Visible = this.cachingEnabled;
            if (cacheExpirySecondsItem.FormControl.value === "") {
                cacheExpirySecondsItem.FormControl.setValue(this.defaultCacheExpiryInSeconds);
            }
            if (!this.cachingEnabled) {
                csvDataItem.FormControl.updateValueAndValidity();
            }
            cacheExpirySecondsItem.FormControl.updateValueAndValidity();
        });

        // needed to trigger subscribe to hide it immediately.
        this.form.patchValue({
            'memoryCachingEnabled': false,
            'cacheExpiryInSeconds': this.defaultCacheExpiryInSeconds,
        });
    }

    protected navigateBasedOnTheMode(): void {
        if (!this.dataTableDefinitionId) {
            this.navigateBackToList();
        } else {
            this.navigateBackToDetails();
        }
    }

    protected navigateEitherFromCreateOrEdit(mode: string): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments = pathSegments.filter((segment: string) => segment !== mode);
        this.navProxy.navigate(pathSegments);
    }

    protected navigateBackToList(): void {
        this.navigateEitherFromCreateOrEdit("create");
    }

    protected navigateBackToDetails(): void {
        this.navigateEitherFromCreateOrEdit("edit");
    }
}
