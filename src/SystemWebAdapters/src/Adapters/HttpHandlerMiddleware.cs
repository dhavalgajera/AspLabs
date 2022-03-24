// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Adapters;

internal class HttpHandlerMiddleware<T>
    where T : IHttpHandler
{
    private T? _reuseable;

    public async Task InvokeAsync(HttpContextCore coreContext)
    {
        var handler = GetHandler(coreContext.RequestServices);
        var context = coreContext.GetAdapter();

        if (handler is HttpTaskAsyncHandler taskHandler)
        {
            await taskHandler.ProcessRequestAsync(context);
        }
        else if (handler is IHttpAsyncHandler asyncHandler)
        {
            await Task.Factory.FromAsync(asyncHandler.BeginProcessRequest(context, null, null), asyncHandler.EndProcessRequest);
        }
        else
        {
            handler.ProcessRequest(context);
        }
    }

    private T GetHandler(IServiceProvider services)
    {
        if (_reuseable is not null)
        {
            return _reuseable;
        }

        var handler = ActivatorUtilities.CreateInstance<T>(services);

        if (handler.IsReusable)
        {
            _reuseable = handler;
        }

        return handler;
    }
}
