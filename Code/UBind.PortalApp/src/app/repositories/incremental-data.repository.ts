import { Pager } from "@app/helpers/pager";
import { IncrementalDataLoader } from '@app/repositories/incremental-data-loader';

/* eslint-disable @typescript-eslint/ban-types */

/**
 * A repository for retreiving data in increments
 */
export interface IncrementalDataRepository extends IncrementalDataLoader {
    pager: Pager;
    boundList: Array<any>;
    errorMessage: any;
    hasLoaded: boolean;

    populateGrid(
        onFetchCallback: Function,
        prepareDataCallback?: Function,
        postPopulateProcessCallBack?: Function): any;
    addMoreRows(
        onFetchCallback: Function,
        event: any,
        prepareDataCallback?: Function,
        postPopulateProcessCallBack?: Function): any;
}
