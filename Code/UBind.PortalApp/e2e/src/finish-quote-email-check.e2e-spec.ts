import { browser, by, element, $, protractor } from 'protractor';
import { AppPage } from './app.po';
import { MailTrapService } from './services/mailtrap.service';
import { TestDataService } from './services/testdata.service';

let sampleData = new TestDataService().retrieve();

describe('Quote Email Verification/', () => {
  let mailTrapService: MailTrapService;
  let page: AppPage;
  beforeEach(() => {
    page = new AppPage();
    mailTrapService = new MailTrapService(sampleData.mailtrap.apiToken, Date.now());
    page.sleepInSeconds(2);
  });

  var chain = function () {
    var defer = protractor.promise.defer();
    defer.fulfill(true);
    return defer.promise;
  };

  it('Verify Email Sent', (done) => {
    if (sampleData.checkResults.email) {
      //filter messages
      var searchFilter = mailTrapService[sampleData.mailtrap.emailCheckFunction];

      chain()
        .then(function () { })
        .then(async function () {
          let continueLoop = true;
          let cycle = 15;
          let seconds = 10;
          //retieve emails on mailtrap
          //call this every 10seconds until the timeout happens
          while (continueLoop && cycle > 0) {
            await page.sleepInSeconds(seconds);
            await mailTrapService.searchMessageAsync(sampleData, searchFilter).then(filteredMessages => {
              if (filteredMessages.length > 0) {
                continueLoop = false;
                done();
                expect(filteredMessages.length).toBeGreaterThan(0);
                return true;
              } else {
                console.log("there is no message recieved on this cycle yet. will retry in " + seconds + " sec");
                cycle -= 1;
              }
            }).catch(e => {
              console.log(e);
              throw JSON.stringify(e);
            });
          }
          if (cycle <= 0) {
            //cycle finishes but there is no message recieved
            expect(cycle).toBeGreaterThan(0, "cycle finished without finding email");
            done();
          }
        });
    } else {
      console.log("skipped");
      done();
    }
  });
});
