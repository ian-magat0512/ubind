import { Component, OnInit, Input } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { saveAs } from 'file-saver';
import { PermissionService } from '@app/services/permission.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { DocumentViewModel } from '@app/viewmodels';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { EntityType } from '@app/models/entity-type.enum';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export quote document view component class
 * This is to load the quote document.
 */
@Component({
    selector: 'app-quote-document-view',
    templateUrl: './quote-document-view.component.html',
    styleUrls: [
        '../../../assets/css/scrollbar-segment.css',
        '../../../assets/css/scrollbar-div.css',
    ],
    styles: [
        scrollbarStyle,
    ],
    animations: [contentAnimation],
})
export class QuoteDocumentViewComponent implements OnInit {

    @Input() public documents: Array<DocumentViewModel>;
    @Input() public entityId: string;
    @Input() public isLoadingDocuments: boolean;
    @Input() public entityType: EntityType;
    protected destroyed: Subject<void>;

    public documentErrorMessage: string;
    public documentHeaders: Array<string> = [];
    protected downloading: any = {};
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
public navProxy: NavProxyService,
        public policyApiService: PolicyApiService,
        private permissionService: PermissionService,
        protected documentApiService: DocumentApiService,
    ) {
    }

    public ngOnInit(): void {
        if (!this.permissionService.hasViewQuotePermission()) {
            this.documentErrorMessage = 'You are not allowed to access quote document';
            return;
        }
        this.documentHeaders = Array.from(new Set(this.documents.map((item: DocumentViewModel) =>
            item.dateGroupHeader)));
        this.destroyed = new Subject<void>();
    }

    public download(documentId: string, fileName: string): void {
        if (this.downloading[documentId]) {
            return;
        }
        if (this.entityType == EntityType.Quote) {
            this.downloading[documentId] = true;
            this.documentApiService
                .downloadQuoteDocument(documentId, this.entityId)
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.downloading[documentId] = false),
                )
                .subscribe((blob: any) => saveAs(blob, fileName));
        } else if (this.entityType == EntityType.QuoteVersion) {
            this.downloading[documentId] = true;
            this.documentApiService
                .downloadQuoteVersionDocument(documentId, this.entityId)
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.downloading[documentId] = false),
                )
                .subscribe((blob: any) => saveAs(blob, fileName));
        }
    }
}
