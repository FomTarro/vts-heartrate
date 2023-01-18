import * as document from "document";
import { display } from "display";
import { peerSocket } from "messaging";
import { HeartRateSensor } from "heart-rate";
import { me } from "appbit";

const hrmLabel = document.getElementById("hrm-label");
const hrmData = document.getElementById("hrm-data");
const errorDisplay = document.getElementById("error-display");
const sensors = [];

// Cannot allow the app to fall asleep and stop sending data!
me.appTimeoutEnabled = false;
errorDisplay.text = "Awaiting connection to Phone Bridge..."

peerSocket.onmessage = evt => {
    const jsonObject = JSON.parse(evt.data);
    const message = jsonObject.message
    switch(message){
        case "error":
            errorDisplay.text = jsonObject.value;
            break;
        default:
            console.log("Unknown message type: " + message)
            break;
    }
}

peerSocket.onopen = evt => {
  errorDisplay.text = "Phone Bridge OPEN!"
}

function post(val){
  if(peerSocket.readyState === peerSocket.OPEN){
    const message = JSON.stringify({
      message: 'data',
      appID: me ? me.applicationId : "MISSING",
      buildID: me ? me.buildId : "MISSING",
      value: val
    });
    peerSocket.send(message);
  }
}

if (HeartRateSensor) {
  const hrm = new HeartRateSensor({ frequency: 1 });
  hrm.addEventListener("reading", () => {
    const bpm = hrm.heartRate ? hrm.heartRate : 0;
    hrmData.text = JSON.stringify({ heartRate: bpm });
    post(bpm);
  });
  sensors.push(hrm);
  hrm.start();
} else {
  hrmLabel.style.display = "none";
  hrmData.style.display = "none";
  errorDisplay.text = "No heart rate sensor detected on device!";
}

// display.addEventListener("change", () => {
//   // Automatically stop all sensors when the screen is off to conserve battery
//   display.on ? sensors.map(sensor => sensor.start()) : sensors.map(sensor => sensor.stop());
// });
