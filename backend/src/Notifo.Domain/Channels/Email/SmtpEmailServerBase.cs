﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.ObjectPool;
using MimeKit;
using MimeKit.Text;

namespace Notifo.Domain.Channels.Email
{
    public abstract class SmtpEmailServerBase : IEmailServer
    {
        private readonly ObjectPool<SmtpClient> clientPool;
        private readonly SmtpOptions options;

        internal sealed class SmtpClientPolicy : PooledObjectPolicy<SmtpClient>
        {
            public override SmtpClient Create()
            {
                return new SmtpClient();
            }

            public override bool Return(SmtpClient obj)
            {
                return true;
            }
        }

        protected SmtpEmailServerBase(SmtpOptions options)
        {
            clientPool = new DefaultObjectPoolProvider().Create(new SmtpClientPolicy());

            this.options = options;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            var smtpClient = clientPool.Get();
            try
            {
                await EnsureConnectedAsync(smtpClient);

                var smtpMessage = new MimeMessage();

                smtpMessage.From.Add(new MailboxAddress(
                    message.FromName,
                    message.FromEmail));

                smtpMessage.To.Add(new MailboxAddress(
                    message.ToName,
                    message.ToEmail));

                var hasHtml = !string.IsNullOrWhiteSpace(message.BodyHtml);
                var hasText = !string.IsNullOrWhiteSpace(message.BodyText);

                if (hasHtml && hasText)
                {
                    smtpMessage.Body = new MultipartAlternative
                    {
                        new TextPart(TextFormat.Plain)
                        {
                            Text = message.BodyText
                        },

                        new TextPart(TextFormat.Html)
                        {
                            Text = message.BodyHtml
                        }
                    };
                }
                else if (hasHtml)
                {
                    smtpMessage.Body = new TextPart(TextFormat.Html)
                    {
                        Text = message.BodyHtml
                    };
                }
                else if (hasText)
                {
                    smtpMessage.Body = new TextPart(TextFormat.Plain)
                    {
                        Text = message.BodyText
                    };
                }
                else
                {
                    throw new InvalidOperationException("Cannot send email without text body or html body");
                }

                smtpMessage.Subject = message.Subject;

                await smtpClient.SendAsync(smtpMessage, ct);
            }
            finally
            {
                clientPool.Return(smtpClient);
            }
        }

        private async Task EnsureConnectedAsync(SmtpClient smtpClient)
        {
            if (!smtpClient.IsConnected)
            {
                await smtpClient.ConnectAsync(options.Host, options.Port);
            }

            if (!smtpClient.IsAuthenticated)
            {
                await smtpClient.AuthenticateAsync(options.Username, options.Password);
            }
        }

        public virtual Task RemoveEmailAddressAsync(string emailAddress, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public virtual Task<EmailVerificationStatus> AddEmailAddressAsync(string emailAddress, CancellationToken ct = default)
        {
            return Task.FromResult(EmailVerificationStatus.Verified);
        }

        public virtual Task<Dictionary<string, EmailVerificationStatus>> GetStatusAsync(HashSet<string> emailAddresses, CancellationToken ct = default)
        {
            var result = new Dictionary<string, EmailVerificationStatus>();

            foreach (var address in emailAddresses)
            {
                result[address] = EmailVerificationStatus.Verified;
            }

            return Task.FromResult(result);
        }
    }
}
