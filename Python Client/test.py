# import websocket;
# import ssl;

# ws = websocket.WebSocket(sslopt={"cert_reqs": ssl.CERT_NONE, "check_hostname": False})
# ws.connect("ws://globaldeviceservice.dev.koreone/api/v1")

from websocket import create_connection
import socket
ws = create_connection("ws://echo.websocket.org/")
ws.send("asdf")
temp = ws.recv()
print(temp)