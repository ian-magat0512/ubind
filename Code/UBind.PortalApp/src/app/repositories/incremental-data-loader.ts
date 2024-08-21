/* eslint-disable @typescript-eslint/ban-types */

/**
 * Represents something which loads data incrementally, e.g. for populating a 
 * list when someone scrolls down.
 */
export interface IncrementalDataLoader {

    infiniteScrollIsLoading: boolean;
    isDataLoading: boolean;
    setErrorMessage(err: any): void;
    initializeData(): void;
    initializeErrorMessage(): void;
    emptyBoundList(): void;
    populateData(data: any, instantiateModel: Function): void;
    populateMoreData(data: any, instantiateModel: Function): void;
    completedCallback(): void;
    completedScrollLoadingCallback(): void;
}
