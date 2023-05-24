using System;
using System.Collections.Generic;
using System.Threading;

namespace Event_Bus_Throttling
{
    // Define the delegate for the event handler
    public delegate void EventHandler(object sender, EventArgs e);

    // Define the event bus class
    public class EventBus
    {
        // Dictionary to hold the registered events and their handlers
        private Dictionary<string, EventHandler> eventHandlers = new Dictionary<string, EventHandler>();

        // Dictionary to hold the registered events and their throttling limits
        private Dictionary<string, int> eventThrottlingLimits = new Dictionary<string, int>();

        // Dictionary to hold the last time an event was fired
        private Dictionary<string, DateTime> lastEventFiredTime = new Dictionary<string, DateTime>();

        // Method to register an event and its handler
        public void RegisterEvent(string eventName, EventHandler eventHandler, int throttlingLimit)
        {
            // Check if the event already exists
            if (eventHandlers.ContainsKey(eventName))
            {
                // Add the new handler to the existing event
                eventHandlers[eventName] += eventHandler;
            }
            else
            {
                // Create a new event with the handler
                eventHandlers[eventName] = eventHandler;
            }

            // Add the throttling limit for the event
            eventThrottlingLimits[eventName] = throttlingLimit;
        }

        // Method to unregister an event and its handler
        public void UnregisterEvent(string eventName, EventHandler eventHandler)
        {
            // Check if the event exists
            if (eventHandlers.ContainsKey(eventName))
            {
                // Remove the handler from the event
                eventHandlers[eventName] -= eventHandler;

                // If there are no more handlers for the event, remove the event
                if (eventHandlers[eventName] == null)
                {
                    eventHandlers.Remove(eventName);
                    eventThrottlingLimits.Remove(eventName);
                    lastEventFiredTime.Remove(eventName);
                }
            }
        }

        // Method to fire an event
        public void FireEvent(string eventName, EventArgs e)
        {
            // Check if the event exists and if it has a throttling limit
            if (eventHandlers.ContainsKey(eventName) && eventThrottlingLimits.ContainsKey(eventName))
            {
                // Get the throttling limit for the event
                int throttlingLimit = eventThrottlingLimits[eventName];

                // Check if the event has been fired before
                if (lastEventFiredTime.ContainsKey(eventName))
                {
                    // Get the last time the event was fired
                    DateTime lastFiredTime = lastEventFiredTime[eventName];

                    // Calculate the time since the last event was fired
                    TimeSpan timeSinceLastFired = DateTime.Now - lastFiredTime;

                    // Check if the time since the last event was fired is less than the throttling limit
                    if (timeSinceLastFired.TotalMilliseconds < throttlingLimit)
                    {
                        // Wait for the remaining time before firing the event
                        Thread.Sleep(throttlingLimit - (int)timeSinceLastFired.TotalMilliseconds);
                    }
                }

                // Fire the event
                eventHandlers[eventName]?.Invoke(this, e);

                // Update the last time the event was fired
                lastEventFiredTime[eventName] = DateTime.Now;
            }
        }
    }

    // Define the publisher class
    public class Publisher
    {
        // Define the event for the publisher
        public event EventHandler Event;

        // Method to fire the event
        public void FireEvent(EventArgs e)
        {
            Event?.Invoke(this, e);
        }
    }

    // Define the subscriber class
    public class Subscriber
    {
        // Method to handle the event
        public void HandleEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Event handled by subscriber");
        }
    }

    // Define the priority subscriber class
    public class PrioritySubscriber
    {
        // Method to handle the event with priority
        public void HandleEventWithPriority(object sender, EventArgs e)
        {
            Console.WriteLine("Event handled by priority subscriber");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create a new event bus
            EventBus eventBus = new EventBus();

            // Create a new publisher
            Publisher publisher = new Publisher();

            // Create a new subscriber
            Subscriber subscriber = new Subscriber();

            // Create a new priority subscriber
            PrioritySubscriber prioritySubscriber = new PrioritySubscriber();

            // Register the event and its handler with the event bus
            eventBus.RegisterEvent("Event", subscriber.HandleEvent, 1000);

            // Register the priority event and its handler with the event bus
            eventBus.RegisterEvent("PriorityEvent", prioritySubscriber.HandleEventWithPriority, 500);

            // Subscribe the subscriber to the publisher's event
            publisher.Event += subscriber.HandleEvent;

            // Subscribe the priority subscriber to the publisher's event
            publisher.Event += prioritySubscriber.HandleEventWithPriority;

            // Fire the publisher's event
            publisher.FireEvent(new EventArgs());

            // Fire the event with throttling
            for (int i = 0; i < 10; i++)
            {
                eventBus.FireEvent("Event", new EventArgs());
            }

            // Fire the priority event with throttling
            for (int i = 0; i < 10; i++)
            {
                eventBus.FireEvent("PriorityEvent", new EventArgs());
            }
        }
    }
}