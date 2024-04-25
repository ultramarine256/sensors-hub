# SensorsHub API

## Overview
SensorsHub API manages IoT device interactions like updates, commands, and status checks.

## Endpoints

### Request Update
- **GET** `/hub/requestupdate?deviceId=device123`
  - Initiates a firmware update for the specified device.

### Send Command
- **GET** `/hub/command?deviceId=device123&command=Restart`
  - Sends a specified command to the device.

### Check Status
- **GET** `/hub/status?deviceId=device123`
  - Checks if the specified device is online.

## Usage
Use the provided endpoints with the appropriate parameters to interact with devices.
