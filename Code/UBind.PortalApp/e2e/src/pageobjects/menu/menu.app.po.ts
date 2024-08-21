import { AppPage } from '../../app.po';
import { MenuModel } from './menu-page.model';

export class MenuAppPage extends AppPage {


    menuModel: MenuModel;

    public constructor() {
        super();
        this.menuModel = new MenuModel();
    }

    clickMenuButton() {
        //this chaining is necessary to handle the button, it searches for the right button to click.  
        //as there are duplicate elements produced
        if (this.menuModel.MenuButtonElAll.first() == undefined)
            throw "MENUBUTTON DOESNT EXIST";
            
        return this.menuModel.MenuButtonElAll.first().isPresent().then(present => {
            if (present) {
                return this.menuModel.MenuButtonElAll.first().click().then(x => {
                }).catch(x => {
                    return this.menuModel.MenuButtonElAll.last().click().then(x => {
                    });
                });
            }
            else {
                return this.menuModel.GenericMenuButtonElAll.last().click().then(x => {
                }).catch(x => {
                    return this.menuModel.GenericMenuButtonElAll.first().click().then(x => {
                    });
                });
            }
        });
    }
    navigateToTenant() {
        return this.clickMenuButton().then(x => {
            return this.clickIonicMenu('Tenants');
        });
    }

    navigateToProducts() {
        return this.clickMenuButton().then(x => {
            return this.clickIonicMenu('Products');
        });
    }

    navigateToCustomers() {
        return this.clickMenuButton().then(x => {
            return this.clickIonicMenu('Customers');
        });
    }

    navigateToPolicies() {
        return this.clickMenuButton().then(x => {
            return this.clickIonicMenu('Policies');
        });
    }

    navigateToEmails() {
        return this.clickMenuButton().then(x => {
            return this.clickIonicMenu('Emails');
        });
    }

    navigateToQuotes() {
        return this.clickMenuButton().then(x => {
            return this.clickIonicMenu('Quotes');
        });
    }

    navigateToSignout() {
        return this.clickMenuButton().then(x => {
            return this.clickIonicMenu('Sign Out');
        });
    }
}
