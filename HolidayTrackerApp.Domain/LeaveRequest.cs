
using System;
using HolidayTrackerApp.Domain; // LeaveStatus ve LeaveType burada tanımlı olabilir
using HolidayTrackerApp.Domain.Entities; // Employee sınıfı burada

namespace HolidayTrackerApp.Domain
    {
        public sealed class LeaveRequest
        {
            public Guid Id { get; set; }
            public Guid EmployeeID { get; set; }

            // KRİTİK EKLENTİ: İlişkisel nesne - Controller'ın ihtiyacı olan parça budur.
            public Employee? Employee { get; set; }

            public LeaveType LeaveType { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

            // LeaveRange alanın zaten var.
            public int LeaveRange => (EndDate - StartDate).Days + 1;
        }
    }


