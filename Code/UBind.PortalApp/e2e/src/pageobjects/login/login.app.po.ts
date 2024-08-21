import { AppPage } from '../../app.po';
import { LoginPageModel } from './login-page.model';
import { browser } from 'protractor';

export class LoginAppPage extends AppPage {

    loginPageModel: LoginPageModel;

    public constructor() {
        super();
        this.loginPageModel = new LoginPageModel();
    }

    navigate(tenantId, environment) {
        let tenantUrl = this.loginPageModel.URL.replace('%tenantid%', tenantId);
        tenantUrl = tenantUrl.replace('%environment%', environment);
        if (!(browser.baseUrl.indexOf("localhost:8100") > 0))
            tenantUrl = "portal/" + tenantUrl;
        return this.navigateTo(tenantUrl).then(async x => {
            await this.sleepInSeconds(7);
            return browser.refresh();
        });
    }

    fillLoginPage(username, password) {
        return this.waitForElement(this.loginPageModel.Forms, "forms", 90).then(x => {
            return this.loginPageModel.Forms.isPresent().then(x => {
                return this.setIonicInput(this.loginPageModel.Username, username).then(x => {
                    return this.setIonicInput(this.loginPageModel.Password, password);
                });
            });
        });
    }

    submit() {
        return this.waitForElement(this.loginPageModel.Forms, "forms").then(x => {
            return this.loginPageModel.Forms.submit();
        });
    }
}
