# Cracked.to Authentication System - Python

This repository contains the Python implementation of the authentication system designed for integration with the Cracked.to platform. This system ensures that only users with a valid authentication key and belonging to specific user groups (Premium+ and above) can access the application.

## Getting Started

To use the authentication system, follow these steps:

1. **Download the script:** Clone this repository or download the `auth.py` file.
2. **Execute the script:** Run `python auth.py` from your command line or terminal.
3. **Enter your authentication key:** On the first run, you will be prompted to enter your Cracked.to authentication key. This key will be saved for future use.
4. **Authenticate:** The script will attempt to authenticate you with the Cracked.to platform. If successful, you will be greeted with your username and a confirmation of access.

## Understanding the Script

### Key Storage

The authentication key is stored locally in `key.dat` for ease of use in subsequent authentication attempts.

### HWID

A hardware ID (HWID) unique to your device is generated and used as part of the authentication process. This binds your session to your hardware, enhancing security.

### Premium Group Check

The script checks if you belong to one of the specified Premium+ groups based on the response from the authentication server.

## Troubleshooting

### Authentication Key File Not Found

If `key.dat` is not found, you'll be prompted to enter your auth key again.

### Authentication Errors

Network issues or incorrect auth keys will result in an authentication failed message. Double-check your internet connection and auth key.

## Source Code

The source code for `auth.py` can be found in this repository.

This README provides a step-by-step guide on how to use the `auth.py` file.
