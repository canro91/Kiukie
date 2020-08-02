
# Kiukie

Kiukie implements a queue mechanism with tables using ASP.NET Core Hosted Services. You could use Kiukie to perfom background tasks from tables. It provides hosted services and queue implementations to

* dequeue and process items
* dequeue, process and update items

## Usage

Kiukie offers default hosted services and queue implementations. You only need to plug in what to do with the dequeued items. But, you can add your own logic to dequeue items. Kiukie uses [Insight.Database](https://github.com/jonwagner/Insight.Database) to execute SQL queries. Kiukie has two implementations: process-and-forget and process-and-update.

### Process-and-forget

Kiukie has `DefaultQueueProcessor` and `DefaultQueue`. Once an item is dequeued, it will be removed from the queue table. Similarly, to process more than one item at a time, you can use `DefaultBulkQueueProcessor` and `DefaultBulkQueue`. 

### Process-and-update

Kiukie has `StatefulQueueProcessor` and `StatefulQueue`. This queue implementation uses an status (`Pending`, `Processing`, `Succeeded` and `Failed`) to dequeue, process and update items. Once an item is processed, it will remain in the queue table with either `Succeeded` or `Failed` status. This implementation is suitable for operations likely to fail.

### How-to

1. Create a table `Kiukie.Queue` and populate it with your actions or events to process. You can create your own queue implementation to change the schema and the table names. Also, you can bring your ORM of choice.
2. Pick either the process-and-forget or process-and-update mechanism.
3. Write your own item handler.
4. Plug in everything into the provided hosted service.
5. Voilà!

You can take a look at the [Sample project](https://github.com/canro91/Kiukie/tree/master/Kiukie.Sample) to see the two implementations in action.

## Installation

Grab your own copy

## Contributing

Feel free to report any bug, ask for a new feature or just send a pull-request. All contributions are welcome.

## License

MIT
