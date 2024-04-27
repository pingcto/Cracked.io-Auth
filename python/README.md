# Python Authentication Implementation

This folder contains the Python implementation of the authentication system. The system handles user authentication by verifying credentials against an endpoint and managing local key storage.

## Features
- Read and save authentication key
- Generate and verify hardware ID
- Communicate with the authentication endpoint
- Handle responses and manage user access

## Code Example
Refer to `code_examples.py` for the full implementation, illustrating the process of checking for an existing key, requesting authentication, and handling new users.

## Dependencies
- `requests`: For making HTTP requests to the authentication endpoint.
- `json`: For parsing the response from the authentication endpoint.
- `uuid`: For generating the hardware ID.
- `os`: For file operations related to the local storage of the authentication key.

## Security
The local storage of the authentication key (`key.dat`) should be secured, and the HWID is used to bind the authentication to the user's hardware.

## Usage
The script can be executed directly and will interactively prompt for an authentication key if not previously stored.
