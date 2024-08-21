import { Component, OnInit, Input, ViewChild, ElementRef, Renderer2, AfterViewInit } from '@angular/core';
import { IconLibrary } from '@app/models/icon-library.enum';
import { contentAnimation } from '@assets/animations';

/**
 * Export expandable item component class
 * This is to expand the items.
 */
@Component({
    selector: 'app-expandable-item',
    templateUrl: './expandable-item.component.html',
    styleUrls: ['./expandable-item.component.scss'],
    animations: [contentAnimation],
})
export class ExpandableItemComponent implements OnInit, AfterViewInit {
    /*
     * titleData - Accepts { title: "sample", titleData: "sample value"} for the clickable header
     * expandContent - Accepts [{ "label": "labelData" }, { "label2": "labelData2"} ] for the expandable content 
     *                 where label is the left side label and labael data is the right side label
     */
    @ViewChild("expandWrapper", { read: ElementRef }) public expandWrapper: ElementRef;
    @Input("expanded") public expanded: boolean = false;
    @Input("expandHeight") public expandHeight: string = "250px";
    @Input("expandContent") public expandContent: any;
    @Input("titleData") public titleData: any;
    @Input("isExpandable") public isExpandable: boolean = false;

    public title: string;
    public titleText: string;
    public expandContentHeaders: Array<string>;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(public renderer: Renderer2) { }

    public ngOnInit(): void {
        this.title = this.titleData.title;
        this.titleText = this.titleData.titleText;
        this.expandContentHeaders = Object.keys(this.expandContent);
    }

    public ngAfterViewInit(): void {
        if (this.expandWrapper) {
            this.renderer.setStyle(this.expandWrapper.nativeElement, "max-height", this.expandHeight);
        }
    }

    public expandItem(): void {
        if (this.isExpandable && this.hasContent()) {
            this.expanded = !this.expanded;
        }
    }

    public hasContent(): boolean {
        return (Object.keys(this.expandContent).length > 0);
    }
}
