import { Injectable } from '@angular/core';
import { Observable, SubscriptionLike } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Pager } from '@app/helpers/pager';

/* eslint-disable @typescript-eslint/ban-types */
/**
 * Export Load data service class.
 * TODO: Write a better class header: loading of data functions.
 */
@Injectable({ providedIn: 'root' })
export class LoadDataService {
    public constructor() {
    }

    private subscription: SubscriptionLike;
    private infiniteScrollSubscription: SubscriptionLike;

    public populateGridData(
        onFetchCallback: Function,
        onCompletedCallback: Function,
        onErrorCallback: Function,
        onSuccessCallback: Function,
        onEmptyDataCallback: Function,
        pager: Pager,
        preServiceCallback: Function,
        populateDataCallBack: Function,
        prepareDataCallback?: Function,
        postPopulateProcess?: Function,
    ): void {
        this.unsubscribeSubscriptions();
        preServiceCallback();
        if (onFetchCallback) {
            this.subscription = (onFetchCallback() as Observable<any>)
                .pipe(finalize(() => this.subscription.unsubscribe))
                .subscribe((data: any) => {
                    if (!pager.checkScrollIfEnabled(data.length)) {
                        pager.deactivateInfiniteScroll();
                    } else {
                        pager.activateInfiniteScroll();
                    }

                    if (data.length > 0) {
                        if (prepareDataCallback) {
                            prepareDataCallback(data);
                        }
                        populateDataCallBack(data);
                        if (onSuccessCallback) {
                            onSuccessCallback();
                        }
                    } else {
                        onEmptyDataCallback();
                    }
                    if (postPopulateProcess) {
                        postPopulateProcess();
                    }
                }, (err: any) => {
                    pager.deactivateInfiniteScroll();
                    console.log('err', err.message);
                    if (onErrorCallback) {
                        onErrorCallback(err);
                    } else {
                        throw err;
                    }
                }, () => {
                    if (onCompletedCallback) {
                        onCompletedCallback();
                    }
                });
        }
    }

    public addMoreGridData(
        onFetchCallback: Function,
        onCompletedCallback: Function,
        onErrorCallback: Function,
        pager: Pager,
        preServiceCallback: Function,
        populateMoreDataCallBack: Function,
        event: any,
        prepareDataCallback?: Function,
        postPopulateProcess?: Function,
    ): void {
        if (event) {
            event.stopPropagation();
        }

        if (this.infiniteScrollSubscription && !this.infiniteScrollSubscription.closed) {
            return;
        }
        if (pager.checkListHeight(event, this.infiniteScrollSubscription)) {
            if (onFetchCallback) {
                pager.nextPage();
                preServiceCallback(pager);
                this.infiniteScrollSubscription = (onFetchCallback() as Observable<any>)
                    .pipe(finalize(() => this.infiniteScrollSubscription.unsubscribe))
                    .subscribe((data: any) => {
                        if (!pager.checkScrollIfEnabled(data.length)) {
                            pager.deactivateInfiniteScroll();
                        }
                        if (prepareDataCallback) {
                            prepareDataCallback(data);
                        }
                        populateMoreDataCallBack(data);
                        if (postPopulateProcess) {
                            postPopulateProcess();
                        }
                    }, (err: any) => {
                        pager.deactivateInfiniteScroll();
                        if (onErrorCallback) {
                            onErrorCallback(err);
                        }
                    }, () => {
                        if (onCompletedCallback) {
                            onCompletedCallback();
                        }
                    });
            }
        }
    }

    private unsubscribeSubscriptions(): void {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
        if (this.infiniteScrollSubscription) {
            this.infiniteScrollSubscription.unsubscribe();
        }
    }
}
