# reposUpdater

[![CodeFactor](https://www.codefactor.io/repository/github/jarmyo/reposupdater/badge)](https://www.codefactor.io/repository/github/jarmyo/reposupdater)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/c8465e3791e647dcafcd1fb39e9ce276)](https://www.codacy.com/gh/jarmyo/reposUpdater/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=jarmyo/reposUpdater&amp;utm_campaign=Badge_Grade)

There are simple deployment tools for autoupdate applications. A lightweight alternative to clickOnce that allows run as administrator.
It conssist in two tools, ReposUploader and ReposUpdate:

## ReposUploader
Tool to zip each file in bin/release folder of project, creates a json file that contains info of the package (filename, hash, version) and then upload these files to ftp server. 

### Config
Change contents of [ExampleConfig.json file](https://github.com/jarmyo/reposUpdater/blob/main/ReposUploader/ExampleConfig.json) and rename to config.json. this file must have been in the same path as the executable, in this file put ftp server address, app folder, user, password, publisher info, etc.

The first run needs create a file named 'globalhash.json' that contains CRC hash of files already in the server. This is because the tool dont upload all the files each time, just the files with changes.
To make the first global hash file, run the app with commands /force and /noupload these commands creates the globalhash.json file needed to execute later instances of the program.

## ReposUpdate
This tool check for changes in a json file on the ftp server, download request files fron a certain site, copy to local folder, make registry entries (to make it visible on control panel Programs) and launch new files.

### Usage
Only replace all params on [Common class](https://github.com/jarmyo/reposUpdater/blob/main/ReposUpdate/Common.cs) : compile and distribute. You can change the icon, the description, etc.

## Contributing
This is my first project in github, be gentile :)
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.
