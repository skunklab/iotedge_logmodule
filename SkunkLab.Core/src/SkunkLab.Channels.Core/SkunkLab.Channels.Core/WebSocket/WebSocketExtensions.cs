namespace SkunkLab.Channels.WebSocket
{
   
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebSockets;
    using System;
    using System.Net.WebSockets;
    using System.Threading.Tasks;

    public static class WebSocketExtensions
    {

        //public static void AcceptWebSocketRequest(this HttpContext httpContext, WebSocketServerChannel channel)
        //{
        //    if (httpContext == null)
        //    {
        //        throw new ArgumentNullException("httpContext");
        //    }
        //    if (channel == null)
        //    {
        //        throw new ArgumentNullException("channel");
        //    }

        //    httpContext.AcceptWebSocketRequest(new Func<WebSocketContext, Task>(channel.ProcessWebSocketRequestAsync));

        //}

        public static void AcceptWebSocketRequest(this HttpContext httpContext, WebSocketHandler webSocketHandler)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (webSocketHandler == null)
            {
                throw new ArgumentNullException("webSocketHandler");
            }

            //httpContext.AcceptWebSocketRequest(new Func<WebSocketContext, Task>(webSocketHandler.ProcessWebSocketRequestAsync));
        }





        //public static void AcceptWebSocketRequest(this HttpContext httpContext, WebSocketHandler webSocketHandler)
        //{
        //    if (httpContext == null)
        //    {
        //        throw new ArgumentNullException("httpContext");
        //    }
        //    if (webSocketHandler == null)
        //    {
        //        throw new ArgumentNullException("webSocketHandler");
        //    }

        //    httpContext.AcceptWebSocketRequest(new Func<WebSocketContext, Task>(webSocketHandler.ProcessWebSocketRequestAsync));
        //}
    }

}
