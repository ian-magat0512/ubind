import { browser } from 'protractor';
import { TestDataService } from './services/testdata.service';
import { QuoterAppPage } from '../../../webFormApp/e2e/app.quoter.po';

let sampleData = new TestDataService().retrieve();

describe('UBIND Quoter App/', () => {
  let page: QuoterAppPage;

  let frame: string = 'ubindProduct0';

  beforeEach(() => {
      page = new QuoterAppPage(frame);
      browser.waitForAngularEnabled(false);
      page.setBaseUrl(sampleData.endpoint.quoter);
      page.navigateTo('/assets/landing-page.html?tenant=' + sampleData.tenantId + '&productId=' + sampleData.productId + '&environment=' + sampleData.environment + '');
      page.sleepInSeconds(5);
  });

  it('Fillup Quoter App ', () => {

      browser.waitForAngularEnabled(false);
      //wait for it to load some more
      page.sleepInSeconds(10).then(sleep => {
          let p = Promise.resolve(true);
          return p.then(function () {
              return page.fillupForm(sampleData.quoter.model).then(x => {
                  page.sleepInSeconds(30);
              });
          })
      }).catch(e => {
          console.log(e);
          throw JSON.stringify(e);
      });
  });

  afterEach(() => {
      page.setBaseUrl(sampleData.endpoint.portal);
  });
});
