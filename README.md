# reposUpdater
Simple deployment tools. A lightweight alternative to clickOnce that allows run as administrator.
It conssist in two tools, ReposUploader and ReposUpdate:

## ReposUploader
Tool to zip each file in bin/release folder of project, then upload to ftp server, creates a json configuration file that contains info of the package and upload on the ftp server used.

### Usage
Change contents of ExampleConfig.json and rename to config.json. this file must have been in the same path as the executable, in this fule put ftp server address, app folder, user, password, publisher info, etc.
To make the first global hash file, run the app with commands /force and /noupload these commands creates the globalhash.json file needed to execute later instances of the program.

## ReposUpdate
This tool check for changes in a json file on the ftp server, download request files fron a certain site, copy to local folder, make registry entries (to make it visible on control panel Programs) and launch new files.

The second tool, (ReposUploader), look in certain bin/relase folder, check in CRC is diferent and upload only the new files to the ftp server. write a settings file or add params

### Usage
Only replace all params on 'common' class 


## Contributing
This is my first project in github, be gentile :)
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.
