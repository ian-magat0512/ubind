import { by, element } from "protractor";

export class LoginPageModel {
    public constructor() {
    }
    
    public URL = '/%tenantid%/%environment%/login';
    public Forms = element(by.id('form'));
    public Username = element(by.id('email'));
    public Password = element(by.id('password'));
}