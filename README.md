# Teltec Cloud Backup

## Features

- Primarily designed for Amazon S3 and Windows;
- Backup and restore plans;
- Scheduled execution of backup and restore plans;
- Synchronization;
- Supports paths with more than 260 characters (Windows' MAX_PATH);
- Automatic network shares mapping;
- Runs backup/restore operations without requiring an active user session (a logged user);
- Automatically deletes old backuped files;

## Known Problems

- \#4: FileSystemTreeView: unchecking a parent node without expanding it doesn't uncheck its sub-nodes;
- \#8: Task scheduler does not DELETE existing tasks for plans that no longer exist;
- Can't run the GUI in more than one user session simultaneously;

## Future Work

- Support pre/post command execution;
- Upload/Download retry policy (with exponential backoff) - See http://docs.aws.amazon.com/general/latest/gr/api-retries.html
- Automatically send reports upon failure or completion;
- Improve concurrency;
- Limit bandwidth;
- Restore files to a specified point in time;
- Add lifecycle rules to transition to Glacier after a configurable amount of days - Note that object restoration from an archive can take up to five hours;

## Licensing Policy

We try our best to comply with all requirements imposed by the licenses we directly or indirectly use.
If you believe we have infringed a license, please, let us know by opening an issue in our GitHub repository.

CC-BY-SA-3.0
	[Creative Commons Wiki - Best practices for attribution](https://wiki.creativecommons.org/wiki/Best_practices_for_attribution#Examples_of_attribution)
	[GPL compatibility use cases - Sharing computer code between GPL software and CC-BY-SA communities](https://wiki.creativecommons.org/wiki/GPL_compatibility_use_cases#Sharing_computer_code_between_GPL_software_and_CC-BY-SA_communities)
	[Stack Exchange Blog - Attribution Required](https://blog.stackexchange.com/2009/06/attribution-required/)
Microsoft
	Code samples from MSDN - [Microsoft Developer Services Agreement](https://msdn.microsoft.com/en-us/cc300389.aspx#D) requires [Microsoft Limited Public License](http://opensource.org/licenses/MS-PL)
AWS
	[AWS Site Terms](http://aws.amazon.com/terms/)

## License

TODO

## Copyright

Copyright (C) 2015 Teltec Solutions Ltda.
