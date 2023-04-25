﻿using MassTransit;
using MessageContracts;
var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("localhost");
    cfg.ReceiveEndpoint("invoice-service", e =>
    {
        e.UseInMemoryOutbox();
        e.Consumer<EventConsumer>(c =>
        c.UseMessageRetry(m => m.Interval(5, new TimeSpan(0, 0, 10))));
    });
});
var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));
await busControl.StartAsync(source.Token);
Console.WriteLine("Invoice Microservice Now Listening");
try
{
    while (true)
    {
        //sit in while loop listening for messages
        await Task.Delay(100);
    }
}
finally
{
    await busControl.StopAsync();
}

public class EventConsumer : IConsumer<IInvoiceToCreate>
{
    public async Task Consume(ConsumeContext<IInvoiceToCreate> context)
    {
        var newInvoiceNumber = new Random().Next(10000, 99999);
        Console.WriteLine($"Creating invoice {newInvoiceNumber} for customer:{ context.Message.CustomerNumber}");
    context.Message.InvoiceItems.ForEach(i =>
    {
        Console.WriteLine($"With items: Price: {i.Price}, Desc:{ i.Description}");
    Console.WriteLine($"Actual distance in miles: {i.ActualMileage}, BaseRate: { i.BaseRate}");
    Console.WriteLine($"Oversized: {i.IsOversized}, Refrigerated:{ i.IsRefrigerated}, Haz Mat: { i.IsHazardousMaterial}");
    });
        await context.Publish<IInvoiceCreated>(new
        {
            InvoiceNumber = newInvoiceNumber,
            InvoiceData = new
            {
                context.Message.CustomerNumber,
                context.Message.InvoiceItems
            }
        });
    }
}