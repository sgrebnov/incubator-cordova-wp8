/*
   Copyright 2012 Wolfgang Koller - http://www.gofg.at/

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

cordova.define("cordova/plugin/bluetooth", function(require, exports, module) {
	var exec = require('cordova/exec');
	
	var Bluetooth = function() {}

    Bluetooth.prototype.connectionType = {
        PHONE_TO_PHONE: 0,
        PHONE_TO_DEVICE: 1
    }

	/**
	 * Check if bluetooth API is supported on this platform
	 * @returns true if bluetooth API is supported, false otherwise
	 */
	Bluetooth.prototype.isSupported = function() {
		// Currently only supported on android and Windows Phone 8
        if( device.platform.toLowerCase() == "android" || navigator.userAgent.toLowerCase().indexOf('windows phone 8') != -1) return true;
		return false;
	}

    /**
     * Checks if bluetooth is on
     */
    Bluetooth.prototype.isEnabled = function(successCallback,failureCallback) {
        return exec(successCallback, failureCallback, 'BluetoothManager', 'isEnabled', []);
    }

	/**
	 * Enable bluetooth
	 * 
	 * @param successCallback function to be called when enabling of bluetooth was successfull
	 * @param errorCallback function to be called when enabling was not possible / did fail
	 */
	Bluetooth.prototype.enable = function(successCallback,failureCallback) {
	    return exec(successCallback, failureCallback, 'BluetoothManager', 'enable', []);
	}
	
	/**
	 * Disable bluetooth
	 * 
	 * @param successCallback function to be called when disabling of bluetooth was successfull
	 * @param errorCallback function to be called when disabling was not possible / did fail
	 */
	Bluetooth.prototype.disable = function(successCallback,failureCallback) {
	    return exec(successCallback, failureCallback, 'BluetoothManager', 'disable', []);
	}
	
	/**
	 * Search for devices  and list them
	 * 
	 * @param successCallback function to be called when discovery of other devices has finished. Passed parameter is a JSONArray containing JSONObjects with 'name' and 'address' property.
	 * @param errorCallback function to be called when there was a problem while discovering devices
	 */
	Bluetooth.prototype.discoverDevices = function(successCallback,failureCallback) {
	    return exec(successCallback, failureCallback, 'BluetoothManager', 'discoverDevices', []);
	}

    /**
     * Starts the process of finding a peer app and makes an app discoverable to remote peers.
     *
     * @param successCallback function to be called when advertising of the device has been started.
     * @param errorCallback function to be called when there was a problem while starting advertising
     */
    Bluetooth.prototype.startAdvertising = function(successCallback,failureCallback) {
        return exec(successCallback, failureCallback, 'BluetoothManager', 'startAdvertising', []);
    }

	/**
	 * Open an RFComm channel for a given device & uuid endpoint
	 * 
	 * @param successCallback function to be called when the connection was successfull. Passed parameter is an integer containing the socket id for the connection
	 * @param errorCallback function to be called when there was a problem while opening the connection
	 */
	Bluetooth.prototype.connect = function(successCallback,failureCallback,options) {
	    return exec(successCallback, failureCallback, 'BluetoothManager', 'connect', [options]);
	}
	
	/**
	 * Close a RFComm channel for a given socket-id
	 * 
	 * @param successCallback function to be called when the connection was closed successfully
	 * @param errorCallback function to be called when there was a problem while closing the connection
	 */
	Bluetooth.prototype.disconnect = function(successCallback,failureCallback, options) {
	    return exec(successCallback, failureCallback, 'BluetoothManager', 'disconnect', [options]);
	}
	
	/**
	 * Read from a connected socket
	 * 
	 * @param successCallback function to be called when reading was successfull. Passed parameter is a string containing the read content
	 * @param errorCallback function to be called when there was a problem while reading
	 */
	Bluetooth.prototype.read = function(successCallback,failureCallback) {
	    return exec(successCallback, failureCallback, 'BluetoothManager', 'read', []);
	}

    /**
     * Write to a connected socket
     *
     * @param successCallback function to be called when writing was successfull.
     * @param errorCallback function to be called when there was a problem while writing
     */
    Bluetooth.prototype.send = function(successCallback,failureCallback,options) {
        return exec(successCallback, failureCallback, 'BluetoothManager', 'send', [options]);
    }
	
	var bluetooth = new Bluetooth();
	module.exports = bluetooth;
});
