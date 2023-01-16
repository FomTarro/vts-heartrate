import { settingsStorage } from "settings";
import { peerSocket } from "messaging";

var content = {};
peerSocket.onmessage = evt => {
    const jsonObject = JSON.parse(evt.data);
    const message = jsonObject.message
    switch(message){
        case "data":
            content = jsonObject;
            console.log(content);
            break;
        default:
            console.log("Unknown message type: " + message)
            break;
    }
}

const headers = new Headers();
headers.append('Content-Type', 'application/json');

function post(val) {
  const servercode = JSON.parse(settingsStorage.getItem("servercode"));
  if(servercode !== undefined && servercode !== null && servercode.name.length > 0){
    
    const address = 'http://'+servercode.name+'/fitbit/ping';
    const options = {
      method: 'POST',
      headers: headers,
      body: JSON.stringify(val)
    };
    
    fetch(address, options)
      .then(function(response){
        return response.json();
      }) //Extract JSON from the response
      .then(function(data) {             
        returnError("POST successful.")
        console.log("Got response from server:", JSON.stringify(data));
      })
      .catch(function(error) {
        returnError("Unable to POST heartrate data to the provided Local IP: " + error);
        console.log(error);
      });
  }else{
    returnError("Please enter 'Your Local IP' under 'Settings'.")
  }
}

function returnError(message) {
  peerSocket.send(JSON.stringify({
    message: 'error',
    value: message
  }));
}

const invervalId = setInterval(async function(){
  if(content){
    post(content);
  }
}, 1000);
