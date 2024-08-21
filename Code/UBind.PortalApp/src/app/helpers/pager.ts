import { SubscriptionLike } from 'rxjs';

/**
 * Manging paging in lists, including when to load more data as part of infinite scrolling functionality.
 */
export class Pager {
    public constructor() {
        this.currentPage = 1;
        this.isLoadMoreDataEnabled = true;
        this.pageSize = this.isIE ? this.pageSizeForIE : this.pageSize;
    }

    public isIE: boolean = !!window['MSInputMethodContext'] && !!document['documentMode'];
    public isLoadMoreDataEnabled: boolean;
    public currentPage: number;
    public pageSize: number = 50;
    public pageSizeForIE: number = 18;

    public nextPage(): void {
        this.currentPage++;
    }

    public resetPage(): void {
        this.currentPage = 1;
        this.isLoadMoreDataEnabled = true;
    }

    public checkListHeight(event: any, isItemsStillLoading: SubscriptionLike): boolean {
        const listContainer: any = event.target;
        if ((listContainer.offsetHeight + listContainer.scrollTop) >= (listContainer.scrollHeight - 50)
            && this.isLoadMoreDataEnabled && (!isItemsStillLoading || isItemsStillLoading.closed)) {
            return true;
        } else {
            return false;
        }
    }

    public checkScrollIfEnabled(listLength: number): boolean {
        if (listLength < this.pageSize) {
            return false;
        } else {
            return true;
        }
    }

    public activateInfiniteScroll(): void {
        this.currentPage = 2;
        this.isLoadMoreDataEnabled = true;
    }

    public deactivateInfiniteScroll(): void {
        this.isLoadMoreDataEnabled = false;
        if (this.currentPage > 1) {
            this.currentPage--;
        }
    }
    public getPagingQueryParameters(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('page', this.currentPage.toString());
        params.set('pageSize', this.pageSize.toString());
        return params;
    }
}
