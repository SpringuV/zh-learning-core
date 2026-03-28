global using HanziAnhVu.Shared.Domain;
global using MediatR;
global using FluentValidation;
global using Auth.Application.DomainEvents;
global using Auth.Contracts.IntegrationEvents;
global using HanziAnhVu.Shared.EventBus.Abstracts;
global using Auth.Application.Interfaces;

global using Auth.Contracts;
global using Auth.Contracts.DTOs;
global using Auth.Application.Command.Login;
global using System.IdentityModel.Tokens.Jwt;
global using Auth.Application.Command.Refresh;
global using Auth.Application.Command.Register;
global using Auth.Application.Command.Logout;
global using Auth.Application.Command.ActivateAccount;
global using Auth.Application.Command.ResendMail;
