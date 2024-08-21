import { Component, OnInit, OnDestroy } from "@angular/core";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { SubscriptionLike } from "rxjs";

/**
 * Provides a backdrop, which shades the background so that the popup dialog has the users attention.
 */
@Component({
    selector: 'app-backdrop',
    templateUrl: `./backdrop.component.html`,
})
export class BackdropComponent implements OnInit, OnDestroy {

    public showBackdrop: boolean = false;
    public showBackdropSub: SubscriptionLike;

    public constructor(
        private sharedLoaderService: SharedLoaderService,
    ) { }

    public ngOnInit(): void {
        this.showBackdropSub = this.sharedLoaderService.$showBackdrop
            .subscribe((showBackdrop: boolean) => this.showBackdrop = showBackdrop);
    }

    public ngOnDestroy(): void {
        if (this.showBackdropSub) {
            this.showBackdropSub.unsubscribe();
        }
    }
}
