// const gql = require("graphql-tag");
// const { ApolloClient } = require("apollo-client");
// const { ApolloClientWS } = require("apollo-client-ws");
// const { InMemoryCache } = require("apollo-cache-inmemory");
// require('win-ca')

// process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
// process.env.NODE_EXTRA_CA_CERTS="Certs/cert1.cer Certs/cert1.cer Certs/cert1.cer"

// const link = new ApolloClientWS({
//     uri: "wss://globaldeviceservice.dev.koreone/api/v1",
//     opts: {
//         debug: 0,
//         protocols: [],
//         compress: false,
//         encoding: "json",
//         keepalive: 0,
//         reconnectattempts: 10,
//         reconnectdelay: 2 * 1000,
//     }
// })

// const client = new ApolloClient({
//     link: link,
//     cache: new InMemoryCache()
// })

// client.query({ query: gql`
//     { 
//         deviceById(deviceId: 1) 
//         { 
//             serial 
//         } 
//     }`})
// .then((response) => { 
//     console.log("We got a response")
//     console.log(response)
// })
// .catch((err) => { 
//     console.log("We got an error")
//     console.log(err.networkError.target._req.res) 
// })






// const WebSocket = require('ws');
// require('win-ca')

// process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
// process.env.NODE_EXTRA_CA_CERTS="C:\Users\zoldham\Downloads\kore_rootca.crt C:\Users\zoldham\Downloads\intermediate.crt C:\Users\zoldham\Downloads\issuing.crt"

// const socket = new WebSocket("wss://globaldeviceservice.dev.koreone/status/0", {
//     rejectUnauthorized: false
// })

// socket.on('open', function(event) {
//     console.log("asdf");
// })

// socket.on('error', function(err) {
//     console.log("error");
//     console.log(err);
// })

// socket.on('close', function(event) {
//     console.log("close");
//     console.log(event);
// })


require('win-ca')
process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
process.env.NODE_EXTRA_CA_CERTS="C:\Users\zoldham\Downloads\kore_rootca.crt C:\Users\zoldham\Downloads\intermediate.crt C:\Users\zoldham\Downloads\issuing.crt"
var WebSocketClient = require('websocket').client;

var client = new WebSocketClient();

client.on('connectFailed', function(error) {
    console.log('Connect Error: ' + error.toString());
});
 
client.on('connect', function(connection) {
    console.log('WebSocket Client Connected');
    connection.on('error', function(error) {
        console.log("Connection Error: " + error.toString());
    });
    connection.on('close', function() {
        console.log('echo-protocol Connection Closed');
    });
    connection.on('message', function(message) {
        if (message.type === 'utf8') {
            console.log("Received: '" + message.utf8Data + "'");
        }
    });
    
    function sendNumber() {
        if (connection.connected) {
            var number = Math.round(Math.random() * 0xFFFFFF);
            connection.sendUTF(number.toString());
            setTimeout(sendNumber, 1000);
        }
    }
    sendNumber();
});

client.connect('wss://gps.dev.koreone/api/v1/status')



// const WebSocket = require('ws')

// const WebSocketLink = require('apollo-link-ws');
// const SubscriptionClient = require('subscriptions-transport-ws');

// const GRAPHQL_ENDPOINT = 'wss://globaldeviceservice.dev.koreone/status';

// const client = new SubscriptionClient.SubscriptionClient(GRAPHQL_ENDPOINT, {
//     reconnect: true
// })

// const link = new WebSocketLink(client)