import { AfterViewInit, Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { Clipboard } from '@angular/cdk/clipboard';
import { ActionButtonHelper } from "@app/helpers/action-button.helper";
import { RouteHelper } from "@app/helpers/route.helper";
import { ActionButton } from "@app/models/action-button";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { SamlApiService } from "@app/services/api/saml-api.service";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";
import { contentAnimation } from '@assets/animations';
import { SharedAlertService } from "@app/services/shared-alert.service";
import hljs from 'highlight.js';
import xml from 'highlight.js/lib/languages/xml';

/**
 * Page for displaying the saml metadata.
 */
@Component({
    selector: "app-show-saml-metadata",
    templateUrl: "./show-saml-metadata.page.html",
    animations: [contentAnimation],
    styleUrls: [
        "./show-saml-metadata.page.scss",
        '../../../../../../assets/css/scrollbar-form.css',
        '../../../../../../assets/css/form-toolbar.scss',
    ],
})
export class ShowSamlMetadataPage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {

    public actionButtonList: Array<ActionButton>;
    private authenticationMethodId: string;
    public metadataXml: string;
    public highlightedMetadataXml: string;
    public title: string = "SAML Metadata";

    public constructor(
        private routeHelper: RouteHelper,
        private navProxy: NavProxyService,
        private samlApiService: SamlApiService,
        public layoutManager: LayoutManagerService,
        eventService: EventService,
        private elementRef: ElementRef,
        injector: Injector,
        private clipboard: Clipboard,
        private sharedAlertService: SharedAlertService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.authenticationMethodId = this.routeHelper.getParam('authenticationMethodId');
        hljs.registerLanguage('xml', xml);
        this.load();
        this.initializeActionButtonList();
    }

    public ngAfterViewInit(): void {
        this.elementRef.nativeElement.querySelectorAll('pre code').forEach((block: HTMLElement) => {
            hljs.highlightBlock(block);
        });
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public goBack(): void {
        this.navProxy.navigateBackOne();
    }

    public load(): void {
        this.isLoading = true;
        this.samlApiService.getMetadata(this.routeHelper.getContextTenantAlias(), this.authenticationMethodId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe((metadataXml: string) => {
                this.metadataXml = metadataXml;
                this.highlightedMetadataXml = hljs.highlight(this.metadataXml, { language: 'xml' }).value;
            },
            (err: any) => {
                this.errorMessage = 'We couldn\'t load the saml metadata';
                throw err;
            });
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];

        actionButtonList.push(ActionButton.createActionButton(
            "Copy",
            "copy",
            IconLibrary.IonicV4,
            false,
            "Copy Metadata",
            true,
            (): void => {
                return this.didSelectCopy();
            },
        ));
        actionButtonList.push(ActionButton.createActionButton(
            "Download",
            "cloud-download",
            IconLibrary.IonicV4,
            false,
            "Download Metadata",
            true,
            (): void => {
                return this.didSelectDownload();
            },
        ));
        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private didSelectDownload(): void {
        // navigate to the endpoint url
        window.open(this.samlApiService.getMetadataUrl(
            this.routeHelper.getContextTenantAlias(),
            this.authenticationMethodId),
        '_blank');
    }

    private didSelectCopy(): void {
        this.clipboard.copy(this.metadataXml);
        this.sharedAlertService.showToast("Xml metadata has been copied to the clipboard");
    }
}
