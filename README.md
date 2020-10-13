# reposUpdater
Simple tools to deploy your app. A lightweight alternative to clickOnce that allows run as administrator.


## ReposUploader
 Tool to upload changed files of release on ftp server, creates a local.json file in the server used by https://github.com/jarmyo/ReposUpdate

### Usage

change contents of ExampleConfig.json and rename to config.json. this file must have been in the same path as the executable.
To make the first global hash file, run the app with commands /force and /noupload these commands creates the globalhash.json file needed to execute the program.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


Please make sure to update tests as appropriate.
