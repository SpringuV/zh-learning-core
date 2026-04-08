global using MediatR;
global using Microsoft.Extensions.Logging;
global using Search.Infrastructure.Queries.Users.Delete;
global using Search.Infrastructure.Queries.Users.Get;
global using Elastic.Clients.Elasticsearch;
global using Search.Infrastructure.Queries.Users.Patch;
global using Search.Infrastructure.Queries.Users.Search;
global using Search.Contracts.DTOs;
global using Search.Contracts.Interfaces;
global using Search.Application.Entities;
global using Search.Infrastructure.Queries.Users.Indexs;

global using Auth.Contracts;
global using HanziAnhVu.Shared.EventBus.Abstracts;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Search.Application.EventHandlers.Users;
global using Search.Infrastructure.Services;