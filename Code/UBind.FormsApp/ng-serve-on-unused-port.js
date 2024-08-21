const portfinder = require('portfinder');
const { spawn } = require('child_process');
const path = require('path');

const ngPath = path.join(__dirname, '/node_modules/.bin/ng.cmd');

portfinder.getPort({ port: 4300 }, (err, port) => {
    if (err) {
        console.error(err);
        process.exit(1);
    }

    const ngServe = spawn(ngPath, ['serve', `--port=${port}`]);

    ngServe.stdout.on('data', (data) => {
        console.log(`${data}`);
    });

    ngServe.stderr.on('data', (data) => {
        console.error(`${data}`);
    });

    ngServe.on('close', (code) => {
        console.log(`ng serve process exited with code ${code}`);
    });
});
