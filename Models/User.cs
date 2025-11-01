using System;
using System.Collections.Generic;

namespace MyEventsApi.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
        public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    }
}