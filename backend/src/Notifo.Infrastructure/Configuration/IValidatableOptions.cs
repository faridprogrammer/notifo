﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Notifo.Infrastructure.Configuration
{
    public interface IValidatableOptions
    {
        IEnumerable<ConfigurationError> Validate();
    }
}
