namespace SkunkLab.Channels.WebSocket
{
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.WebSockets;

    public static class WebSocketExtensions
    {

        public static void AcceptWebSocketRequest(this HttpContext httpContext, WebSocketServerChannel channel)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            httpContext.AcceptWebSocketRequest(new Func<AspNetWebSocketContext, Task>(channel.ProcessWebSocketRequestAsync));
        }

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
            httpContext.AcceptWebSocketRequest(new Func<AspNetWebSocketContext, Task>(webSocketHandler.ProcessWebSocketRequestAsync));
        }





        public static void AcceptWebSocketRequest(this HttpContextBase httpContext, WebSocketHandler webSocketHandler)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (webSocketHandler == null)
            {
                throw new ArgumentNullException("webSocketHandler");
            }

            httpContext.AcceptWebSocketRequest(new Func<AspNetWebSocketContext, Task>(webSocketHandler.ProcessWebSocketRequestAsync));
        }
    }

}
