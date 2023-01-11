import { settingsStorage } from "settings";
import { peerSocket } from "messaging";

var bpm = 0;
peerSocket.onmessage = evt => {
    const jsonObject = JSON.parse(evt.data);
    const message = jsonObject.message
    switch(message){
        case "data":
            bpm = jsonObject.value;
            break;
        default:
            console.log("Unknown message type: " + message)
            break;
    }
}

function post(val) {
  const xhr = new XMLHttpRequest();
  const servercode = JSON.parse(settingsStorage.getItem("servercode")).name;
  // const passcode = JSON.parse(settingsStorage.getItem("passcode")).name;
  // console.log("POSTing:" + val);
  // console.log("POSTing:" + val + " to " + servercode);
  const address = 'http://'+servercode+'/fitbit/ping';
  xhr.open("POST", address, true);
  xhr.setRequestHeader('Content-Type', 'application/json');
  xhr.onerror = function(){
    console.error("Unable to POST heart rate data to address: " + address);
    peerSocket.send(JSON.stringify({
      message: 'error',
      value: "Unable to POST to provided address."
    }));
  }
  xhr.onload = function(){
    peerSocket.send(JSON.stringify({
      message: 'error',
      value: "POST successful."
    }));
  }
  xhr.send(JSON.stringify({
      value: val
  }));
}

const invervalId = setInterval(async function(){
  post(bpm);
}, 1000);
