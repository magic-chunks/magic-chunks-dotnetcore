/// <reference path="../../typings/index.d.ts" />

import path = require('path');
import fs = require('fs');
import tl = require("vsts-task-lib/task");

function setAtKeyPath(obj: any, path: string, value: any) {
    let r = path.split("/");
    if (r.length > 1) {
        // Recursively call until we are at terminal key for object
        setAtKeyPath(obj[r.shift()], r.join("/"), value);
    } else {
        // Once at terminal property, set value
        obj[r[0]] = value;
    }
}

let fileType = tl.getInput('fileType');
if (fileType !== 'JSON' || fileType !== 'Auto') {
    tl.warning('x-plat service only supports JSON at this time');
}

let sourcePath = tl.getPathInput('sourcePath');
let sourceObj = JSON.parse(fs.readFileSync(sourcePath, 'utf8'));

let targetPathType = tl.getInput('targetPathType');
var targetPath = sourcePath;
if (targetPathType === 'specific') {
    targetPath = tl.getPathInput('targetPath');
}

let transformations = tl.getInput('transformations');
let transformationObj = JSON.parse(transformations);

try {
    Object.keys(transformationObj).forEach(tKey => {
        setAtKeyPath(sourceObj, tKey, transformationObj[tKey]);
    });

    let outputString = JSON.stringify(sourceObj, null, '\t');
    fs.writeFileSync(targetPath, outputString, { encoding: 'utf8' });   

    tl.debug(`File transformed to ${targetPath}`);

} catch (error) {
    tl.setResult(tl.TaskResult.Failed, error);
}