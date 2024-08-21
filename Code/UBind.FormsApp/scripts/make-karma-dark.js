const util = require('util');
const fs = require('fs');
const exec = util.promisify(require('child_process').exec);
const karmaChromeLauncherIndexJsPath = './node_modules/karma-chrome-launcher/index.js';

async function getUsername() {
    const { stdout } = await exec('echo %username%');
    return stdout.trim();
}

function getDarkReaderExtensionBasePath(username) {
    return `C:\\Users\\${username}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Extensions\\eimadpbcbfnmbkopoojfekhnkhdbieeh`;
}

async function getFirstSubfolder(directoryPath) {
    const readdir = util.promisify(fs.readdir);
    const subfolders = await readdir(directoryPath);
    return subfolders[0];
}

getUsername().then(username => {
    console.log(`Logged in user is: ${username}`);
    const darkReaderExtensionBasePath = getDarkReaderExtensionBasePath(username);
    console.log(`Dark Reader extension base path is: ${darkReaderExtensionBasePath}`);
    getFirstSubfolder(darkReaderExtensionBasePath).then(subfolder => {
        console.log(`Dark Reader extension subfolder is: ${subfolder}`);
        const darkReaderExtensionPath = `${darkReaderExtensionBasePath}\\${subfolder}`;
        console.log(`Dark Reader extension path is: ${darkReaderExtensionPath}`);
        const fs = require('fs');
        fs.readFile(`${karmaChromeLauncherIndexJsPath}`, 'utf8', (err, fileContents) => {
            if (err) {
                console.error(`${karmaChromeLauncherIndexJsPath}:`, err);
            } else {
                const escapedPath = darkReaderExtensionPath.replace(/\\/g, '\\\\');
                const loadExtensionArg = `--load-extension=${escapedPath}`;
                let result;

                // If the file already contains the load-extension argument, replace it, otherwise insert it.
                if (fileContents.includes('--load-extension=')) {
                    const regex = /--load-extension=.*',/;
                    result = fileContents.replace(regex, `${loadExtensionArg}',`);
                } else {
                    const index = fileContents.indexOf(`'--no-default-browser-check'`);
                    if (index !== -1) {
                        const firstPart = fileContents.substring(0, index);
                        const secondPart = fileContents.substring(index);
                        result = firstPart + secondPart.replace(
                            `'--no-default-browser-check',`,
                            `'--no-default-browser-check',\r\n      '${loadExtensionArg}',`);
                    }
                }

                fs.writeFile(`${karmaChromeLauncherIndexJsPath}`, result, 'utf8', (err) => {
                    if (err) {
                        console.error(`Error updating ${karmaChromeLauncherIndexJsPath}:`, err);
                    } else {
                        console.log(`Dark Reader extension path updated successfully in ${karmaChromeLauncherIndexJsPath}`);
                    }
                });
            }
        });
    });
});