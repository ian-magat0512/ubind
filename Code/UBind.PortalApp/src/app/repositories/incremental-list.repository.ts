import { IncrementalDataRepository } from '@app/repositories/incremental-data.repository';
import { Pager } from '@app/helpers/pager';
import { LoadDataService } from '@app/services/load-data.service';

/**
 * Represents a constructor that creates an EntityViewModel, which is needed so that
 * a view model for any kind of entity can be constructed automatically by the list compoent
 * from each entity instance, to be rendered in the list view.
 */
export interface EntityViewModelConstructor<EntityViewModelType, EntityType> {
    new(entity: EntityType): EntityViewModelType;
}

/* eslint-disable @typescript-eslint/ban-types */

/**
 * Export Incremental list repository class.
 * TODO: Write a better class header: Incremental list repository function.
 * Populating of grid, rows and data.
 */
export class IncrementalListRepository<EntityViewModelType, EntityType> implements IncrementalDataRepository {

    public pager: Pager = new Pager();
    public boundList: Array<EntityViewModelType> = new Array<EntityViewModelType>();
    public errorMessage: string;
    public infiniteScrollIsLoading: boolean = false;
    public isDataLoading: boolean = false;
    public hasLoaded: boolean = false;

    public constructor(
        protected viewModelConstructor: EntityViewModelConstructor<EntityViewModelType, EntityType>,
        protected loadDataService: LoadDataService,
        protected repoName: string,
    ) { }

    public populateGrid(
        onFetchCallback: (Function),
        prepareDataCallback?: Function,
        postPopulateProcess?: Function,
    ): void {
        this.populateGridData(
            onFetchCallback.bind(this),
            (data: any) => this.populateData(data, ((item: any): any => new this.viewModelConstructor(item))),
            prepareDataCallback,
            postPopulateProcess,
        );
    }

    public addMoreRows(
        onFetchCallback: Function,
        event: any,
        prepareDataCallback?: Function,
        postPopulateProcess?: Function,
    ): void {
        this.addMoreGridData(
            onFetchCallback.bind(this),
            event,
            (data: any): any => this.populateMoreData(
                data,
                ((item: any): any => new this.viewModelConstructor(item)),
            ),
            prepareDataCallback,
            postPopulateProcess,
        );
    }

    private populateGridData(
        onFetchCallback: Function,
        populateData: Function,
        prepareDataCallback?: Function,
        postPopulateProcessCallback?: Function,
    ): void {
        this.pager = new Pager();
        this.isDataLoading = true;

        this.loadDataService.populateGridData(
            onFetchCallback.bind(this),
            this.completedCallback.bind(this),
            null,
            null,
            this.emptyBoundList.bind(this),
            this.pager,
            this.initializeData.bind(this),
            populateData,
            prepareDataCallback,
            postPopulateProcessCallback,
        );
    }

    private addMoreGridData(
        onFetchCallback: Function,
        event: any,
        populateMoreData: Function,
        prepareDataCallback?: Function,
        postPopulateProcessCallback?: Function,
    ): void {
        this.infiniteScrollIsLoading = true;

        this.loadDataService.addMoreGridData(
            onFetchCallback.bind(this),
            this.completedScrollLoadingCallback.bind(this),
            this.setErrorMessage.bind(this),
            this.pager,
            this.initializeErrorMessage.bind(this),
            populateMoreData,
            event,
            prepareDataCallback,
            postPopulateProcessCallback,
        );
    }

    public setErrorMessage(err: any): void {
        this.errorMessage = 'Unable to load ' + this.repoName;
        console.log('err', err.message);
    }

    public initializeData(): void {
        this.boundList = new Array<EntityViewModelType>();
        this.initializeErrorMessage();
    }

    public initializeErrorMessage(): void {
        this.errorMessage = '';
    }

    public emptyBoundList(): void {
        const tmp: Array<EntityViewModelType> = new Array<EntityViewModelType>();
        this.boundList = tmp;
    }

    public populateData(data: Array<EntityType>, instantiateModel: Function): void {
        const newList: Array<EntityViewModelType> = new Array<EntityViewModelType>();
        data.forEach((item: EntityType) => {
            newList.push(instantiateModel(item));
        });
        this.boundList = newList;
    }

    public completedCallback(): void {
        this.isDataLoading = false;
        this.hasLoaded = true;
    }

    public completedScrollLoadingCallback(): void {
        this.infiniteScrollIsLoading = false;
    }

    public populateMoreData(data: Array<EntityType>, instantiateModel: Function): void {
        data.forEach((item: EntityType) => {
            this.boundList.push(instantiateModel(item));
        });
    }
}
