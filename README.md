# Kiukie

Kiukie implements a queue mechanism with tables using ASP.NET Core Hosted Services. You could use Kiukie to perfom background tasks from tables. It provides hosted services and  queue implementations to

* dequeue and process items
* dequeue, process and update items

## Usage

Kiukie offers default hosted services and queue implementations. You only need to plug in  what to do with dequeued items. But, you can add your own logic to dequeue items. Kiukie has two implementations: non-persistent and persistent.

### Process and forget

Kiukie has `OnlyProcessHostedService` and `OnlyProcessQueue`. Once an item is dequeued, it will be removed from the queue table.

### Process and update

Kiukie has `ProcessAndUpdateHostedService` and `ProcessAndUpdateQueue`. This queue implementation uses an status (`Enqueued`, `Processing`, `Success` and `Failed`) to dequeue, process and update items. Once an item is processed, it will remain in the queue table with either `Sucess` or `Failed` status. This implementation is suitable for operations likely to fail.

## Installation

Grab your own copy

## Contributing

Feel free to report any bug, ask for a new feature or just send a pull-request. All contributions are welcome.

## License

MIT