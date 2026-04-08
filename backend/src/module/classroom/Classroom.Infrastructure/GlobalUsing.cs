global using Classroom.Domain.Entities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

global using Classroom.Domain.Entities.Assignment;
global using Classroom.Infrastructure.Config;
global using Classroom.Domain.Interface;
global using System.Reflection;
global using System.Text.Json;
global using HanziAnhVu.Shared.EventBus.Abstracts;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Shared.Infrastructure.Outbox;
global using Shared.Infrastructure;