using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Modas.Models
{
    // static class cannot be instantiated
    public static class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            var locations = new List<Location>();

            if (!context.Locations.Any())
            {
                locations.Add(new Location { Name = "Front Door" });
                locations.Add(new Location { Name = "Family Room" });
                locations.Add(new Location { Name = "Rear Door" });
                // first, add Locations
                foreach (var l in locations)
                {
                    context.Locations.Add(l);
                }

                context.SaveChanges();
            }

            if (context.Events.Any()) return;
            {
                var localDate = DateTime.Now;
                // subtract 6 months from date
                var eventDate = localDate.AddMonths(-6);
                // random numbers will be needed
                var rnd = new Random();
                // loop for each day in the range from 6 months ago to today
                while (eventDate < localDate)
                {
                    // random between 0 and 5 determines the number of events to occur on a given day
                    var num = rnd.Next(0, 6);

                    // a sorted list will be used to store daily events sorted by time
                    // each time an event is added, the list is re-sorted
                    var dailyEvents = new SortedList<DateTime, Event>();
                    // for loop to generate times for each event
                    for (var i = 0; i < num; i++)
                    {
                        // random between 0 and 23 for hour of the day
                        var hour = rnd.Next(0, 24);
                        // random between 0 and 59 for minute of the day
                        var minute = rnd.Next(0, 60);
                        // random between 0 and 59 for seconds of the day
                        var second = rnd.Next(0, 60);

                        // random location
                        var loc = rnd.Next(0, locations.Count());

                        // generate event date/time
                        var x = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, hour, minute, second);
                        // create event from date/time and location
                        var e = new Event { TimeStamp = x, Flagged = false, Location = context.Locations.FirstOrDefault(l => l.Name == locations[loc].Name) };
                        // add daily events to sorted list
                        dailyEvents.Add(e.TimeStamp, e);
                    }
                    // using sorted list for daily events, add to event list
                    foreach (var item in dailyEvents)
                    {
                        context.Events.AddRange(item.Value);
                    }
                    // add 1 day
                    eventDate = eventDate.AddDays(1);
                }
            }
            // save changes to database
            context.SaveChanges();
        }
    }
}