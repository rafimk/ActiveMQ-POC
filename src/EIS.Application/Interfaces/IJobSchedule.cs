using System;

namespace EIS.Application.Interfaces;

public interface IJobSchedule
{
    Type JobType { get; }
    string GetCronExpression();
}