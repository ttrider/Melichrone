var file = require("file");
var fileExists = require('file-exists');
var fs = require("fs");
var nconf = require('nconf');


nconf.argv().env().file({ file: './config.json' });
nconf.defaults({
    "metadata": { "path": "./metadata" }
});


var rules = [];
var types = {};

//common.person.
var race = {};
var origin = {};
var ethnicity = {};


var metadatPath = file.path.abspath(nconf.get('metadata:path'));
file.walkSync(metadatPath, (dirPath, dirs, files) => {

    for (var i = 0; i < files.length; i++) {
        var filePath = file.path.join(dirPath, files[i]);

        if (fileExists(filePath)) {
            console.log('loading metadata from ' + filePath);


            var mf = require(filePath);

            if (Array.isArray(mf.rules)) {
                for (var j = 0; j < mf.rules.length; j++) {
                    var rule = mf.rules[j];

                    rules.push(rule);

                    if (rule.match) {
                        for (var name in rule.match) {
                            types[name] = name;

                            var val = rule.match[name];
                            if (name === "common.person.origin") {
                                origin[val] = 0;
                            }

                            else if (name === "common.person.ethnicity") {
                                ethnicity[val] = 0;
                            }
                        }
                    }
                    if (rule.produce) {
                        for (var name2 in rule.produce) {
                            types[name2] = name2;

                            var val = rule.produce[name2];

                            if (name2 === "common.person.race") {
                                for (var k = 0; k < val.length; k++) {
                                    race[val[i]] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
});


console.log("race =======================================");
for (var namerace in race) {
    console.log(namerace);
}

console.log("origin =====================================");
for (var nameorigin in origin) {
    console.log(nameorigin);
}


console.log("ethnicity ==================================");
for (var nameethnicity in ethnicity) {
    console.log(nameethnicity);
}


var readline = require('readline');
var rl = readline.createInterface(process.stdin, process.stdout);
rl.setPrompt('guess> ');
rl.prompt();
rl.on('line', (line) => {
    if (line === "right") rl.close();
    rl.prompt();
}).on('close', () => {
        process.exit(0);
    });
