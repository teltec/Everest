# CHANGELOG

This CHANGELOG attemps to follow most convetions established by http://keepachangelog.com/.

## [0.7.0] - 2016-01-08

### Added

- Restore: Only set original modified date if the restored file is the latest complete
  version;
- GUI: Log session ending event;

### Changed

- Logging: Minimize log polution;
- Sync: Make cancellation work and improve performance;

### Fixed

- GUI: Fix possible NPE triggered by tab switching;
- GUI: Always show minutes and seconds for duration periods;
- Sync: Skip and log S3 keys that don't comply with the standard path conventions,
  instead of failing the sync;
- Sync: Fix UQ violation;


## [0.6.0] - 2015-12-22

### Added

- GUI: Implement '<Create new account>';
- S3: New window to create an S3 bucket;
- Transfer: Make upload chunk size configurable.
- Test: New Demo application to test upload performance.
- IPC: Add errorCode parameter to ERROR messages.

### Changed

- Global: Update dependency packages;
- Logging: Log is now sensitive to active configuration;
- Executor: Log progress only when a transfer did complete, fail, or was canceled;
- Executor: In DEBUG and user interactive mode, make the PlanExecutor cancel
  the current operation in the following conditions:
	- Received Ctrl+C;
	- The parent console is closed;
	- The user is logging out;
	- The machine is shutting down;
- Executor: Refactor the storage backend interfaces to be synchronous so we
  can take advantage of Parallel.ForEach;
- S3: Prefer S3 high-level API over low-level API.
- S3: Increase read buffer size to improve upload performance.
- IPC: Change IPC port from 8000 to 35832.
- IPC: Adjust polling timeout for write to 1/5 of a second;
- IPC: Avoid spamming the sender of ROUTE messages with ERROR messages when
  the intended target is not connected.

### Fixed

- Executor: Resuming a backup now processes only pending files;
- Executor: Fix missing LIMIT on GetLatestByPlan query;
- Executor: Make operation cancelling work properly;
- Executor: Improve memory usage: No longer create Tasks for all transfers at
  once;
- GUI: Remove all previously listed buckets upon bucket listing failure;
- GUI: Correctly display the operation's duration after it ends.
- GUI: Fix unresponsiveness issue that occured when the user canceled a
  `StorageAccount` edit. The application became unresponsive
  for some time because the `Refresh()` method also does
  refresh all child models that have the Refresh cascade, such as
  the one-to-many relationship with `BackupPlanFile`s.

  The unresponsiveness was more noticeable when the account had a
  large number of associated files (10000+), making the application
  load all of them during the `Refresh()`.

  We did fix the issue by removing the Refresh cascade from the
  beforementioned relationship.
- IPC: Improve REGISTER and NOT_AUTHORIZED handling.
- IPC: Fix command ordering to avoid receiving "Not authorized" messages
  in the GUI. Register early - before the application gets a chance
  to send other messages that require registration.
- IPC: Fix port validation.
- IPC: Catch `ObjectDisposedException` during socket recv;
- IPC: Detect when the TCP port is already in use and handle it during
  the Service starting process so it can fail and exit correctly.

