﻿using Puppet.Common.Devices;
using Puppet.Common.Events;
using Puppet.Common.Models.Automation;
using Puppet.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Puppet.Automation
{
    public class SafetyAlert : IAutomation
    {
        List<Speaker> _speakers;
        HomeAutomationPlatform _hub;

        public SafetyAlert(HomeAutomationPlatform hub)
        {
            _hub = hub;
            _speakers = new List<Speaker>();
            _speakers.Add(new Speaker(hub, DeviceMap.Speaker.KitchenSpeaker));
            _speakers.Add(new Speaker(hub, DeviceMap.Speaker.WebhookNotifier));
        }

        public void Handle(HubEvent evt, CancellationToken token)
        {
            string message = "";
            DateTime when = DateTime.Now;

            switch(evt.value)
            {
                case "wet":  //water sensor positive
                    message = $"Water has been detected by the {evt.display_name}.";
                    break;

                case "detected":  //smoke alarm positive
                    message = $"Smoke or carbon monoxide has been detected.";
                    break;

                default:
                    return;
            }

            _speakers.Speak($"{message} Please investigate.");

            int count = 1;
            while (true)
            {
                Task.Delay(TimeSpan.FromHours(1)).Wait();
                if (token.IsCancellationRequested) return;
                string reminderMessage = $"Reminder from {count} hour{((count == 1) ? "" : "s")} ago: {message}";
                if (count > 5)
                {
                    _speakers.Speak($"{reminderMessage} This will be your final reminder.");
                    return;
                }
                else
                {
                    _speakers.Speak(reminderMessage);
                }
                count++;
            }                
        }
    }
}