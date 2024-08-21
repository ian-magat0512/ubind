import { NgModule } from '@angular/core';
import { CreateQuotePage } from './create-quote/create-quote.page';
import { EditQuotePage } from './edit-quote/edit-quote.page';
import { EditQuoteVersionPage } from './edit-quote-version/edit-quote-version.page';
import { ListModule } from '@app/list.module';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ListQuoteVersionPage } from './list-quote-version/list-quote-version.page';
import { DetailQuoteVersionPage } from './detail-quote-version/detail-quote-version.page';
import { ListQuoteMessagePage } from './list-quote-message/list-quote-message.page';
import { ListQuoteVersionMessagePage } from './list-quote-version-message/list-quote-version-message.page';
import { ReviewQuotePage } from './edit-quote/review-quote.page';
import { EndorseQuotePage } from './edit-quote/endorse-quote.page';

/**
 * Export Quote Common Module class.
 * This class manage Ng Module declarations quote common.
 */
@NgModule({
    declarations: [
        CreateQuotePage,
        EditQuotePage,
        ReviewQuotePage,
        EndorseQuotePage,
        EditQuoteVersionPage,
        ListQuoteVersionPage,
        DetailQuoteVersionPage,
        ListQuoteMessagePage,
        ListQuoteVersionMessagePage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
    exports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        CreateQuotePage,
        EditQuotePage,
        ReviewQuotePage,
        EndorseQuotePage,
        EditQuoteVersionPage,
        ListQuoteVersionPage,
        DetailQuoteVersionPage,
        ListQuoteMessagePage,
    ],
})
export class QuoteCommonModule { }
