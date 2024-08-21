const fs = require('fs');

// Read version from version.txt
fs.readFile('../version.txt', 'utf8', (err, version) => {
  if (err) {
    console.error('Error reading version.txt:', err);
    process.exit(1);
  }

  // Read package.json
  fs.readFile('package.json', 'utf8', (err, packageJsonData) => {
    if (err) {
      console.error('Error reading package.json:', err);
      process.exit(1);
    }

    // Update version in package.json
    const packageJson = JSON.parse(packageJsonData);
    packageJson.version = version.trim();

    // Write the updated package.json
    fs.writeFile('package.json', JSON.stringify(packageJson, null, 2), 'utf8', (err) => {
      if (err) {
        console.error('Error updating package.json:', err);
        process.exit(1);
      }

      console.log('Version updated successfully in package.json');
    });
  });
});