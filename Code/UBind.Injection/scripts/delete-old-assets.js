const fs = require('fs');
const path = require('path');

const filesToDelete = [
    path.resolve(__dirname, '../../UBind.Web/wwwroot/assets/ubind.js'),
    path.resolve(__dirname, '../../UBind.Web/wwwroot/assets/ubind.js.map')
];

filesToDelete.forEach((filePath) => {
    if (fs.existsSync(filePath)) {
        fs.unlinkSync(filePath);
        console.log(`Deleted ${filePath} because it being there would have stopped the new version from being served.`);
    }
});
