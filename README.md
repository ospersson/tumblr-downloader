# TDC a tumblr-downloader
A command line Tumblr image downloader written in C#.
It uses the Tumblr API V1. V2 is used for downloading avatars.

## Usage
```
Usage: tdc.exe <url> [<directory>]

eg. tdc.exe xyz.tumblr.com
eg. tdc.exe xyz.tumblr.com c:\pictures\xxx.tumblr.com\
eg. tdc.exe xyz.tumblr.com c:\pictures\xxx.tumblr.com\ -l

Options:

  -l,   saves json file(s) to a JSON folder. 
```
## Development environment
Windows 10.

Visual Studio 2015 Update 2.

## Known limitations
Tumblr API V1 only returns one image per blog post. 

## License
MIT
