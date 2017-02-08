# DataEntry CLI

## Build Status

|Develop|Master|
|:--:|:--:|
|[![Build status](https://ci.appveyor.com/api/projects/status/ui4lsw0yb7g926tf/branch/develop?svg=true)](https://ci.appveyor.com/project/eugene-sergueev/dataentry-cli/branch/develop)|[![Build status](https://ci.appveyor.com/api/projects/status/ui4lsw0yb7g926tf/branch/develop?svg=true)](https://ci.appveyor.com/project/eugene-sergueev/dataentry-cli/branch/master)|

## Table of Contents

- [Install](#install)
- [Usage](#usage)
- [Contribute](#contribute)
- [License](#license)

## Install

[![GitHub release](https://img.shields.io/github/release/ibrsp/dataentry-cli.svg)](https://github.com/ibrsp/dataentry-cli/releases/latest)

Download the latest stable release from [GitHub release](https://github.com/ibrsp/dataentry-cli/releases/latest) or use PowerShell script:

```PowerShell 
$uri = 'https://api.github.com/repos/ibrsp/dataentry-cli/releases/latest'
$latestRelease = Invoke-RestMethod -Uri $uri -Method GET
$outFileName =  "./$($latestRelease.name).zip"

Invoke-WebRequest -Uri $LatestRelease.zipball_url -OutFile $outFileName -Method Get

Expand-Archive -Path $outFileName -DestinationPath $latestRelease.name  -Force

cd  $latestRelease.name
```

## Usage

Get list supported commands and options:

```
de -?
```

Get help for sequence-upload command:

```
de sequence-upload -?
```

Imaging you have the following directory structure:

```
results
└───BY
│   │   2016.csv
│   │   2015.csv
|   |
|   └───2014
│       │   results.csv
│   
└───AZ
    │   results.csv
```
To upload all results from all sub folders, you could use glogging in file path argumnet:
```
de sequence-upload ./results/**/*.csv http://localhost -o ./report.csv -t
```
To simulate upload use `--dryrun` option:
```
de sequence-upload ./results/**/*.csv http://localhost -o ./report.csv -t --dryrun
```

## Important

Sequence upload support append only mode, in case of data modification or getting new result please use `--truncate` option to drop existing data.

## Contribute

PRs accepted.

## License

MIT © Richard McRichface
