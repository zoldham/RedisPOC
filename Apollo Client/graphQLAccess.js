const gql = require("graphql-tag");
const { ApolloClient } = require("apollo-client");
const { ApolloClientWS } = require("apollo-client-ws");
const { InMemoryCache } = require("apollo-cache-inmemory");

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

const link = new ApolloClientWS({
    uri: "wss://globaldeviceservice.dev.koreone/api/v1",
    opts: {
        debug: 0,
        protocols: [],
        compress: false,
        encoding: "json",
        keepalive: 0,
        reconnectattempts: 10,
        reconnectdelay: 2 * 1000,
    }
})

const client = new ApolloClient({
    link: link,
    cache: new InMemoryCache()
})

client.query({ query: gql`
        { 
            deviceById(deviceId: 1) 
            { 
                serial 
            } 
        }`})
    .then((response) => { 
        console.log("We got a response")
        console.log(response)
    })
    .catch((err) => { 
        console.log("We got an error")
        console.log(err) 
    })





// const WebSocket = require('ws');

// process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

// const socket = new WebSocket("wss://globaldeviceservice.dev.koreone/api/v1", {
//     rejectUnauthorized: false
// })

// socket.on('error', function(err) {
//     console.log("error");
//     console.log(err);
// })

// socket.on('close', function(event) {
//     console.log("close");
//     console.log(event);
// })