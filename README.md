# reposUpdater
Simple tools to deploy your app. A lightweight alternative to clickOnce that allows run as administrator.

## ReposUpdate
Application updater from ReposSoftware designed to update our app repos restaurant, its a replace to ClickOnce.
You can use for your app, just replace some variables like your ftp server and instalation path.

In order to use this implementation you need two tools, Updater and Uploader, the fist one (this) check for changes in a json file in the server, download request files fron a certain site, copy to local folder, make registry entries (to make it visible on control panel Programs) and launch new files.

The second tool, https://github.com/jarmyo/ReposUploader (ReposUploader), look in certain bin/relase folder, check in CRC is diferent and upload only the new files to the ftp server. write a settings file or add params

this is my first project in github.

### Usage

Only replace all params on 'common' class 

## ReposUploader
 Tool to upload changed files of release on ftp server, creates a local.json file in the server used by https://github.com/jarmyo/ReposUpdate

### Usage

change contents of ExampleConfig.json and rename to config.json. this file must have been in the same path as the executable.
To make the first global hash file, run the app with commands /force and /noupload these commands creates the globalhash.json file needed to execute the program.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


Please make sure to update tests as appropriate.
