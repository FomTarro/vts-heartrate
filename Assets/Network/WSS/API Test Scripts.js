// Data Pipe test
const dataWS = new WebSocket('ws://localhost:8214/data');
dataWS.onmessage = (e) => {console.log(JSON.parse(e.data))}

// Events
const eventWS = new WebSocket('ws://localhost:8214/events');
eventWS.onmessage = (e) => {console.log(JSON.parse(e.data))}

//Input/Auth
const inputWS = new WebSocket('ws://localhost:8214/input');
const reconnect = {}
inputWS.onmessage = (e) => {
    const parsed = JSON.parse(e.data);
    console.log(parsed)
    if(!parsed.data.authenticated && parsed.messageType == "AuthenticationResponse"){
        inputWS.send(JSON.stringify({
            messageType: "AuthenticationRequest",
            data: {
                pluginName: "My Cool New Plugin",
                pluginAuthor: "Skeletom",
                token: parsed.data.token
            }
        }))
        reconnect.reconnect = () => {
            let ws = new WebSocket('ws://localhost:8214/input');
            reconnect.send = (num) => {
                ws.send(JSON.stringify({
                    messageType: "InputRequest",
                    data: {
                        heartrate: num
                    }
                }))
            }
            ws.onopen = () => {
                ws.send(JSON.stringify({
                    messageType: "AuthenticationRequest",
                    data: {
                        pluginName: "My Cool New Plugin",
                        pluginAuthor: "Skeletom",
                        token: parsed.data.token
                    }
                }))
            }
        }
    }else{
        inputWS.send(JSON.stringify({
            messageType: "InputRequest",
            data: {
                heartrate: (120 * Math.random()) + 10
            }
        }))
    }
}
inputWS.onopen = () => {
    inputWS.send(JSON.stringify({
        messageType: "AuthenticationRequest",
        data: {
            pluginName: "My Cool New Plugin",
            pluginAuthor: "Skeletom",
            pluginAbout: "A plugin for connecting a my exercise bike to vts-heartrate!"
        }
    }));
}
