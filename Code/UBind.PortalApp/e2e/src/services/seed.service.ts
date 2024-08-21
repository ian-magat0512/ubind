const fileStream = require('fs');
const request = require('request');

export class SeedService {
    private tenantId: string;
    private productId: string;
    private environment: string;
    baseUrl: string;

    public constructor(baseUrl, tenantId, productId, environment) {
        this.tenantId = tenantId;
        this.productId = productId;
        this.baseUrl = baseUrl;
        this.environment = environment;
    }

    seedInvoiceNumbers() {
        return this.processNumberCreations("invoiceNumbers", "INV-");
    }

    seedPolicyNumbers() {
        return this.processNumberCreations("policyNumbers", "P");
    }

    seedClaimNumbers() {
        return this.processNumberCreations("claimNumbers", "C");
    }

    getFiles(seedRelativePath) {
        var files = this.getSeedFiles(seedRelativePath);
        return this.filterFilesForProduct(this.tenantId, this.productId, files);
    }

    uploadFiles(userName: string, password: string, files: Array<object>): Promise<{}> {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.getAuth(userName, password).then(accessToken => {
                var promises: Promise<{}>[] = [];
                var i = 0;

                //upload them to api
                files.forEach(file => {
                    promises[i] = _this.postFiles(file, accessToken, _this);
                    i++;
                });

                Promise.all(promises).then(s => {
                    resolve(s);
                }, r => {
                    reject('Error');
                });
            });
        });
    }

    uploadFiles2(userName: string, password: string, files: Array<object>): Promise<Promise<{}>[]> {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.getAuth(userName, password).then(accessToken => {
                var promises: Promise<{}>[] = [];
                var i = 0;

                //upload them to api
                files.forEach(file => {
                    promises[i] = _this.postFiles(file, accessToken, _this);
                    i++;
                });

                return resolve(promises);
            });
        });
    }

    private processNumberCreations(type: string, numberPrefix: string) {
        var _this: SeedService = this;
        var numPrefix: string = numberPrefix;

        return new Promise(function (resolve, reject) {
            var url = _this.baseUrl + "/api/v1/" + _this.environment + "/" + _this.tenantId + "/" + _this.productId + "/" + type + "?all=true&environment=" + _this.environment;
            var array = [];

            for (var i = 0; i <= 999; i++) {
                array.push(numberPrefix + _this.pad(i, 4));
            }

            _this.get(url).then(result => {
                var recievedValue = JSON.parse(result);

                //gets the numbers that are not on the database yet.
                var newArray = array.filter(val => !recievedValue[type].includes(val));
                //newArray = array;
                if (newArray.length > 0) {
                    var bodyStringify = JSON.stringify(newArray);
                    //then post those numbers
                    _this.post(url, bodyStringify).then(result => {
                        resolve(result);
                    }).catch(err => {
                        //if just a duplicate error. ignore
                        if (err.statusCode == 500 && err.body.indexOf('Duplicate') >= 0) {
                            resolve(result);
                            console.log("successfully uploaded " + type);
                        }
                        else {
                            reject(err);
                        }
                    });
                } else {
                    resolve();
                }
            }).catch(err => {
                reject(err);
            });
        });
    }

    //generic post json body to url
    private post(url, jsonBody, token = null) {
        var _this = this;
        return new Promise(function (resolve, reject) {

            request({
                uri: url,
                method: 'POST',
                body: jsonBody,
                rejectUnauthorized: false,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
            },
                function (err, resp, body: string) {
                    if (err) {
                        reject(err);
                    } else {
                        if (resp.statusCode == 200) {
                            console.log("post request success!");
                            resolve(body);
                        }
                        else {
                            reject(resp);
                        }
                    }
                });
        });
    }

    //generic post json body to url
    private get(url, token = null): Promise<string> {
        var _this = this;
        return new Promise(function (resolve, reject) {

            request({
                uri: url,
                method: 'GET',
                rejectUnauthorized: false,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
            },
                function (err, resp, body) {
                    if (err) {
                        reject(err);
                    } else {
                        if (resp.statusCode == 200) {
                            console.log("get request successful!");
                            resolve(body);
                        }
                        else {
                            reject(resp.body);
                        }
                    }
                });
        });
    }

    //retrieves seed files
    private getSeedFiles(folder) {
        var files: Array<object> = null;
        var folders = fileStream.readdirSync(folder);

        folders!.forEach(x => {
            var path = folder + '\\' + x;
            var stat = fileStream.lstatSync(path);
            if (stat.isDirectory()) {
                if (files == null) {
                    files = this.getSeedFiles(path);
                }
                else {
                    files = files.concat(this.getSeedFiles(path));
                }
            } else {
                if (files == null) {
                    files = [{ fileName: x, path: path }];
                } else {
                    files.push({ fileName: x, path: path });
                }
            }
        });

        return files;
    }

    //filter the files for the product
    private filterFilesForProduct(tenantId, productId, files = null) {

        var filtered = [];
        files!.forEach(x => {
            if (x.path.indexOf(tenantId) > 0 && x.path.indexOf(productId) > 0) {
                filtered.push(x);
            }
        });

        return filtered;
    }

    private getAuth(username, password): Promise<string> {
        var _this = this;
        return new Promise(function (resolve, reject) {
            var url =
                // _this.baseUrl.indexOf('ubind') > -1 ?
                //     _this.baseUrl + '/api/v1/portal/ubind/' + this.environment + '/login' :
                _this.baseUrl + '/api/v1/ubind/' + _this.environment + '/login';

            var req = request({
                uri: url,
                method: 'POST',
                body: JSON.stringify({ emailAddress: username, plainTextPassword: password }),
                rejectUnauthorized: false,
                headers: {
                    'Content-Type': 'application/json'
                },
            },
                // callback
                function (err, resp, body) {
                    if (err) {
                        reject(err);
                    } else {
                        resolve(JSON.parse(body).accessToken);
                    }
                }
            );
        });
    }

    //calls an API to send the request
    private postFiles(file, token, _this): Promise<string> {

        return new Promise(function (resolve, reject) {
            var url = _this.baseUrl + "/api/" + _this.tenantId + "/" + _this.environment + "/" + _this.productId + "/product/seed";

            var stream = fileStream.createReadStream(file.path);
            var pathSplit = (file.path).replace('\\\\', '\\').split('\\');
            var subPath = pathSplit.slice(pathSplit.indexOf(_this.productId) + 1, pathSplit.length);
            var fileName = subPath.pop();

            if (subPath.length > 0) {
                url += "?folder=" + subPath.join(',');
            }

            request({
                uri: url,
                method: 'POST',
                formData: {
                    file: stream
                },
                rejectUnauthorized: false,
                headers: {
                    'Content-Length': stream.length,
                    'Authorization': 'Bearer ' + token
                },
            },
                function (err, resp, body) {
                    if (err) {
                        console.log('Error uploading ' + fileName);
                        reject('Error uploading ' + fileName);
                    } else {
                        if (resp.statusCode == 200) {
                            console.log(fileName + " uploaded!");
                            resolve('Successfully ' + fileName + " uploaded!");
                        }
                        else {
                            console.log('Error uploading ' + fileName);
                            reject('Error uploading ' + fileName);
                        }
                    }
                });
        });
    }

    //pad a number
    private pad(number, width, prefix = null) {
        prefix = prefix || '0';
        number = number + '';
        return number.length >= width ? number : new Array(width - number.length + 1).join(prefix) + number;
    }
}