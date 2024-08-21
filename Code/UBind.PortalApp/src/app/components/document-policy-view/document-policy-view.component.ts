import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { Document } from '@app/models';
import { saveAs } from 'file-saver';
import { formatDate } from '@angular/common';
import { PolicyDocumentsDetailResourceModel } from '@app/resource-models/policy.resource-model';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export document policy view component class
 * This class component is for downloading and loading of
 * documents policy.
 */
@Component({
    selector: 'app-document-policy-view',
    templateUrl: './document-policy-view.component.html',
})
export class DocumentPolicyViewComponent implements OnInit {

    @Input() public policyId: string;
    @Input() public quoteOrPolicyTransactionId: string;
    public documents: Array<Document>;
    public documentHeaders: Array<string>;
    public isTransactionView: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private downloading: any = {};

    public constructor(
private policyApiService: PolicyApiService,
        private route: ActivatedRoute,
        private documentApiService: DocumentApiService,
    ) {

    }

    public ngOnInit(): void {
        this.load();
    }

    public load(): void {
        this.isTransactionView = this.route.snapshot.url.toString().search(/transaction/g) >= 0;
        this.policyApiService.getPolicyDocuments(this.policyId).subscribe((dt: PolicyDocumentsDetailResourceModel) => {
            if (!this.isTransactionView) {
                this.documents = dt.documents;
            } else {
                this.documents = dt.documents.filter((d: Document) =>
                    d.quoteOrPolicyTransactionId == this.quoteOrPolicyTransactionId);
            }

            this.documentHeaders = Array.from(
                new Set(this.documents.map((item: Document) =>
                    formatDate(item.createdDateTime, "dd MMM yyyy", 'en-AU'))),
            );
        });

    }

    public downloadDocument(documentId: string, fileName: string): void {
        let targetDoc: Document = this.documents.find((d: Document) => d.id == documentId);
        if (this.downloading[documentId] && !targetDoc) {
            return;
        }

        this.downloading[documentId] = true;
        this.documentApiService
            .downloadPolicyDocument(this.policyId, targetDoc.quoteOrPolicyTransactionId, documentId)
            .subscribe(
                (blob: any) => {
                    saveAs(blob, fileName);
                    this.downloading[documentId] = false;
                },
                () => {
                    this.downloading[documentId] = false;
                },
            );
    }
}
