import { Component, OnInit } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { scrollbarStyle } from '@assets/scrollbar';

/**
 * Export product filter component class
 * To filter the products details.
 */
@Component({
    selector: 'app-product-filter',
    templateUrl: './product-filter.component.html',
    styleUrls: ['./product-filter.component.scss'],
    styles: [
        scrollbarStyle,
    ],
})
export class ProductFilterComponent implements OnInit {

    public productsCheckBoxes: Array<{ id: string; name: string; isChecked: boolean }> = [];
    public tempProductsCheckBoxes: Array<{ id: string; name: string; isChecked: boolean }> = [];
    public applyDisabled: boolean = false;

    public constructor(
        private modalCtrl: ModalController,
    ) { }

    public ngOnInit(): void {
        this.checkProductCheckBoxesAndUpdateApplyButton();
    }

    public cancel(): void {
        this.modalCtrl.dismiss();
    }

    public applyChanges(): void {
        this.modalCtrl.dismiss(this.productsCheckBoxes);
    }

    public checkProductCheckBoxesAndUpdateApplyButton(): void {
        this.applyDisabled =
            this.productsCheckBoxes.filter((p: any) => p.isChecked === true).length === 0 ? true : false;
    }

    public productCheckboxChange(event: any): void {
        const { name, checked }: any = event.target;

        this.productsCheckBoxes = this.productsCheckBoxes.map(
            (p: any) => {
                if (p.id === name) {
                    p.isChecked = checked;
                }
                return p;
            },
        );
        this.checkProductCheckBoxesAndUpdateApplyButton();
    }
}
