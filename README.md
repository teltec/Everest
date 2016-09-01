![everest logo](everest-logo-small.png)

## What is Everest?

Everest is an Open Source software that aims to help you automate your Backup and Restore operations.

## Requirements

- .NET Framework 4.5
- SQL Server 2012 Express
- Windows

## Installation notes

- The Windows user you're using to install Everest must have administrative rights on your instance of SQL Server Express. We may improve this by asking the user during the installation process to provide database details (host, credentials, etc);
- After you install, you need to change the SQL Server authentication mode to "SQL Server and Windows Authentication mode". We may automate this in the future, but for now you have to do it manually;

## Features

- Primarily designed for Amazon S3 and Windows;
- Backup and restore plans;
- Scheduled execution of backup and restore plans;
- Synchronization;
- Automatic network shares mapping;
- Runs backup/restore operations without requiring an active user session (a logged user);
- Automatically deletes old backuped files (adjustable);
- Automatically sends reports upon failure or completion;

## Changelog

You can read the entire changelog [here](CHANGELOG.md).

## Known Problems

- \#4: FileSystemTreeView: unchecking a parent node without expanding it doesn't uncheck its sub-nodes;
- \#8: Task scheduler does not DELETE existing tasks for plans that no longer exist;
- \#14: Editing a plan and changing its storage account won't work properly.
- Can't run the GUI in more than one user session simultaneously;

## Future Work

- Support long paths (>260 characters - Windows' MAX_PATH) -- Currently limited by lack of support from the AWSSDK - see https://github.com/aws/aws-sdk-net/issues/294;
- Write unit tests;
- Attempt to make abstractions more developer friendly;
- Do not backup files of user-specified types;
- Upload and download retry policy with exponential backoff - See http://docs.aws.amazon.com/general/latest/gr/api-retries.html
- Bandwidth throttling;
- Restore files to a specified point in time;
- Add lifecycle rules to transition to Glacier after a configurable amount of days - Note that object restoration from an archive can take up to five hours;
- Versions for Linux and OS X?

## How to contribute

1. Fork the repository;
2. Clone your repository: `git clone <repo-url>`;
3. Execute `install-nuget-dependencies.run`;
4. Create a new branch for your changes: `git checkout -b my-branch-name`;
5. Make your changes and commit: `git add file1 file2` and `git commit -m 'Describe your changes.'`;
6. Push the branch to your fork: `git push origin my-branch-name`;
7. Open a Pull Request;

## Licensing Policy

We try our best to comply with all requirements imposed by the licenses from code and libraries we directly or indirectly use.
If you believe we have infringed a license, please, let us know by opening an issue in our GitHub repository or contacting us directly.

## License

Still to be defined.
